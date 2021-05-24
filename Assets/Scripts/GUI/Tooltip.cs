using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Godot;
using JoyGodot.Assets.Scripts.GUI.Managed_Assets;
using JoyLib.Code.Graphics;

namespace JoyLib.Code.Unity.GUI
{
    public class Tooltip : GUIData
    {
        [Export] protected Vector2 PositionOffset { get; set; }

        [Export]
        public DynamicFont CustomFont
        {
            get => this.m_CustomFont;
            set
            {
                this.m_CustomFont = value;

                if (this.ItemCache.IsNullOrEmpty())
                {
                    return;
                }
                
                foreach (var label in this.ItemCache)
                {
                    label.AddFontOverride("font", value);
                }
            }
        }
        
        protected DynamicFont m_CustomFont;
        protected ManagedLabel Title { get; set; }
        protected Control IconSlot { get; set; }
        protected ManagedUIElement Icon { get; set; }
        protected Control Background { get; set; }
        
        protected Control MainContainer { get; set; }
        protected BoxContainer ContentContainer { get; set; }
        protected List<Label> ItemCache { get; set; }
        
        protected bool ShouldShow { get; set; }
        
        protected bool Empty { get; set; }

        public override void _Ready()
        {
            base._Ready();
            
            if (this.ItemCache.IsNullOrEmpty() == false)
            {
                foreach (var item in this.ItemCache)
                {
                    item.Visible = false;
                }
            }
            else
            {
                this.ItemCache = new List<Label>();
            }

            this.Title = this.FindNode("Title") as ManagedLabel;
            this.IconSlot = this.FindNode("Icon Container") as Control;
            this.Icon = this.IconSlot?.GetNode<ManagedUIElement>("Icon");
            this.Background = this.GetNode<Control>("Margin Container/Background");
            this.MainContainer = this.GetNode<Control>("Margin Container/Main Container");
            this.ContentContainer = this.MainContainer.GetNode<BoxContainer>("Content Container");
            
            this.Hide();
        }

        public override void _Input(InputEvent @event)
        {
            if (this.GUIManager is null)
            {
                this.GUIManager = GlobalConstants.GameManager?.GUIManager;
                return;
            }

            bool actionPressed = Input.IsActionPressed("tooltip_show");
            
            if (actionPressed && this.ShouldShow == false)
            {
                this.ShouldShow = true;
            }
            else if (actionPressed == false && this.ShouldShow)
            {
                this.ShouldShow = false;
            }

            if (this.ShouldShow 
                && this.Visible == false
                && this.Empty == false)
            {
                this.ShowCurrent();
            }
            else if(this.ShouldShow == false && this.Visible)
            {
                this.GUIManager?.CloseGUI(this.Name);
            }
            
            if (@event is InputEventMouseMotion motion)
            {
                this.UpdatePosition(motion);
            }
        }

        protected void UpdatePosition(InputEventMouseMotion motion)
        {
            if (this.GUIManager?.Cursor is null)
            {
                return;
            }
            
            Vector2 mousePosition = motion.Position;

            Vector2 rectSize = this.MainContainer.RectSize;
            Vector2 offset = Vector2.Zero;
            Vector2 viewportSize = this.GetViewport().Size;
            int cursorSize = this.GUIManager.Cursor.CursorSize;
            if (mousePosition.x < viewportSize.x - rectSize.x)
            {
                offset.x += cursorSize / 2;
            }
            else
            {
                offset.x -= rectSize.x;
            }

            if (mousePosition.y > viewportSize.y - rectSize.y)
            {
                offset.y -= rectSize.y;
            }
            else
            {
                offset.y += cursorSize / 2;
            }

            this.RectPosition = mousePosition + this.PositionOffset + offset;
        }

        public virtual void Show(
            string title = null,  
            ISpriteState sprite = null, 
            ICollection<string> data = null, 
            bool showBackground = true)
        {
            GD.Print("TOOLTIP SHOW");

            bool allEmpty = true;
            
            if (!title.IsNullOrEmpty())
            {
                this.Title.Visible = true;
                this.Title.Text = title;
                allEmpty = false;
            }
            else
            {
                this.Title.Visible = false;
            }

            if (sprite is null == false)
            {
                this.IconSlot.Visible = true;
                this.SetIcon(sprite);
                allEmpty = false;
            }
            else
            {
                this.IconSlot.Visible = false;
            }

            if (data.IsNullOrEmpty() == false)
            {
                allEmpty = false;
                if (this.ItemCache.Count < data.Count)
                {
                    for (int i = this.ItemCache.Count; i < data.Count; i++)
                    {
                        Label item = new Label
                        {
                            SizeFlagsHorizontal = 1,
                            SizeFlagsVertical = 9,
                            Align = Label.AlignEnum.Center,
                            Valign = Label.VAlign.Center,
                            Autowrap = true
                        };
                        item.AddFontOverride("font", this.CustomFont);
                        this.ContentContainer.AddChild(item);
                        this.ItemCache.Add(item);
                    }
                }
                else if (this.ItemCache.Count > data.Count)
                {
                    for (int i = data.Count; i < this.ItemCache.Count; i++)
                    {
                        this.ItemCache[i].Visible = false;
                    }
                }
                    
                for (int i = 0; i < data.Count; i++)
                {
                    this.ItemCache[i].Text = data.ElementAt(i);
                    this.ItemCache[i].Visible = true;
                }
            }
            else
            {
                foreach(var item in this.ItemCache)
                {
                    item.Visible = false;
                }
            }
           
            this.Background.Visible = showBackground;

            if (allEmpty)
            {
                this.Empty = true;
            }
            
            if (this.ShouldShow)
            {
                this.Display();
                this.GUIManager?.OpenGUI(this.Name);
            }
        }

        public virtual void ShowCurrent()
        {
            this.Show(
                this.Title.Text,
                this.Icon.CurrentSpriteState,
                this.ItemCache.Select(item => item.Text).ToList());
        }

        public override void Display()
        {
            if (this.ShouldShow
                && this.Title.Text.IsNullOrEmpty() == false)
            {
                base.Display();
            }
        }

        public void WipeData()
        {
            this.Icon.Clear();
            this.IconSlot.Visible = false;
            this.Title.Text = null;
            foreach (var item in this.ItemCache)
            {
                item.Visible = false;
                item.Text = null;
            }

            this.Empty = true;
        }

        protected void SetIcon(ISpriteState state)
        {
            this.Icon.Clear();
            this.Icon.AddSpriteState(state);
            this.Icon.OverrideAllColours(state.SpriteData.GetCurrentPartColours());
        }
    }
}