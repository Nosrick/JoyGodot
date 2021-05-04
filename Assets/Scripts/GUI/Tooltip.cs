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
            if (@event is InputEventMouseMotion motion)
            {
                this.UpdatePosition(motion);
            }
        }

        protected void UpdatePosition(InputEventMouseMotion motion)
        {
            Vector2 mousePosition = motion.Position;

            Vector2 offset = Vector2.Zero;
            /*
            if (mousePosition.x < Screen.width - this.RectTransform.sizeDelta.x)
            {
                offset.x += this.CursorRect.width / 2;
            }
            else
            {
                offset.x -= this.CursorRect.width / 2 + this.RectTransform.rect.width;
            }

            if (mousePosition.y > Screen.height - this.RectTransform.sizeDelta.y)
            {
                offset.y -= this.CursorRect.height / 2;
            }
            else
            {
                offset.y += this.CursorRect.height / 2 + this.RectTransform.rect.height;
            }
            
            switch (this.Canvas.renderMode)
            {
                case RenderMode.ScreenSpaceOverlay:
                    mousePosition += offset;
                    break;
                
                case RenderMode.ScreenSpaceCamera:
                    mousePosition = this.MainCamera.ScreenToWorldPoint(mousePosition + offset);
                    break;
                
                case RenderMode.WorldSpace:
                    mousePosition = this.MainCamera.ScreenToWorldPoint(mousePosition + offset);
                    break;
            }
            */

            //this.transform.position = this.Canvas.transform.TransformPoint(mousePosition + offset);
            this.RectPosition = mousePosition + this.PositionOffset;
        }

        public virtual void Show(
            string title = null,  
            ISpriteState sprite = null, 
            ICollection<string> data = null, 
            bool showBackground = true)
        {
            this.Display();
            
            if (!string.IsNullOrEmpty(title))
            {
                this.Title.Visible = true;
                this.Title.Text = title;
            }
            else
            {
                this.Title.Visible = false;
            }

            if (sprite is null == false)
            {
                this.IconSlot.Visible = true;
                this.SetIcon(sprite);
            }
            else
            {
                this.IconSlot.Visible = false;
            }

            if (data.IsNullOrEmpty() == false)
            {
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
        }

        protected void SetIcon(ISpriteState state)
        {
            this.Icon.Clear();
            this.Icon.AddSpriteState(state, true);
        }
    }
}