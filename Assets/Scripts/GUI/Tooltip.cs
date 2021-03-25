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
        [Export] protected Label m_Title;
        [Export] protected Label m_Text;
        [Export] protected Node2D m_IconSlot;
        [Export] protected ManagedUIElement m_Icon;
        [Export] protected Node2D m_Background;
        [Export] protected StringPairContainer m_ItemPrefab;
        [Export] protected BoxContainer m_ParentLayout;
        [Export] protected Vector2 m_PositionOffset;
        protected List<StringPairContainer> ItemCache { get; set; }

        public void Awake()
        {
            if (this.ItemCache.IsNullOrEmpty() == false)
            {
                foreach (var item in this.ItemCache)
                {
                    item.Visible = false;
                }
            }
            else
            {
                this.ItemCache = new List<StringPairContainer>();
            }
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
            this.RectPosition = mousePosition;
        }

        public virtual void Show(
            string title = null, 
            string content = null, 
            ISpriteState sprite = null, 
            IEnumerable<Tuple<string, string>> data = null, 
            bool showBackground = true)
        {
            if (!string.IsNullOrEmpty(title))
            {
                this.m_Title.Visible = true;
                this.m_Title.Text = title;
            }
            else
            {
                this.m_Title.Visible = false;
            }

            this.m_Text.Text = content;
            this.m_Text.Visible = !content.IsNullOrEmpty();

            if (sprite is null == false)
            {
                this.m_IconSlot.Visible = true;
                this.SetIcon(sprite);
            }
            else
            {
                this.m_IconSlot.Visible = false;
            }

            if (data.IsNullOrEmpty() == false)
            {
                Tuple<string,string>[] dataArray = data.ToArray();
                if (this.ItemCache.Count < dataArray.Length)
                {
                    for (int i = this.ItemCache.Count; i < dataArray.Length; i++)
                    {
                        this.ItemCache.Add((StringPairContainer) this.m_ItemPrefab.Duplicate());
                    }
                }
                else if (this.ItemCache.Count > dataArray.Length)
                {
                    for (int i = dataArray.Length; i < this.ItemCache.Count; i++)
                    {
                        this.ItemCache[i].Visible = false;
                    }
                }
                    
                for (int i = 0; i < dataArray.Length; i++)
                {
                    this.ItemCache[i].Target = dataArray[i];
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
           
            this.m_Background.Visible = showBackground;

            /*
            float height = this.ItemCache.Count(container => container.gameObject.activeSelf) * 0.055f;
            this.RectTransform.anchorMin = new Vector2(0.4f, 0);
            this.RectTransform.anchorMax = new Vector2(0.6f, height);
            this.RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0);
            this.RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
            */
        }

        protected void SetIcon(ISpriteState state)
        {
            this.m_Icon.Clear();
            this.m_Icon.AddSpriteState(state, true);
        }
    }
}