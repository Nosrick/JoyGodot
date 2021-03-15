using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using JoyLib.Code.Graphics;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace JoyLib.Code.Unity.GUI
{
    [RequireComponent(typeof(GUIData))]
    public class Tooltip : MonoBehaviour
    {
        [SerializeField] protected TextMeshProUGUI m_Title;
        [SerializeField] protected TextMeshProUGUI m_Text;
        [SerializeField] protected RectTransform m_IconSlot;
        [SerializeField] protected ManagedUISprite m_Icon;
        [SerializeField] protected GameObject m_Background;
        [SerializeField] protected StringPairContainer m_ItemPrefab;
        [SerializeField] protected LayoutGroup m_ParentLayout;
        [SerializeField] protected Vector2 m_PositionOffset;

        protected Canvas Canvas { get; set; }
        protected Camera MainCamera { get; set; }
        protected RectTransform RectTransform { get; set; }
        
        protected List<StringPairContainer> ItemCache { get; set; }
        
        protected Rect CursorRect { get; set; }

        public void Awake()
        {
            if (this.Canvas is null)
            {
                this.ItemCache = new List<StringPairContainer>();
                this.Canvas = this.GetComponentInParent<Canvas>();
                this.Canvas.sortingLayerName = "GUI";
                this.RectTransform = this.GetComponent<RectTransform>();
                this.m_Icon.Awake();
                this.MainCamera = Camera.main;
            }
        }

        public void Update()
        {
            this.CursorRect = GlobalConstants.GameManager.GUIManager
                .Get(GUINames.CURSOR)
                .GetComponent<RectTransform>()
                .rect;
            this.UpdatePosition();
        }

        protected void UpdatePosition()
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();

            Vector2 offset = Vector2.zero;
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

            //this.transform.position = this.Canvas.transform.TransformPoint(mousePosition + offset);
            this.transform.position = mousePosition;
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
                this.m_Title.gameObject.SetActive(true);
                this.m_Title.text = title;
            }
            else
            {
                this.m_Title.gameObject.SetActive(false);
            }

            this.m_Text.text = content;
            this.m_Text.gameObject.SetActive(!content.IsNullOrEmpty());

            if (sprite is null == false)
            {
                this.m_IconSlot.gameObject.SetActive(true);
                this.SetIcon(sprite);
            }
            else
            {
                this.m_IconSlot.gameObject.SetActive(false);
            }

            if (data.IsNullOrEmpty() == false)
            {
                Tuple<string,string>[] dataArray = data.ToArray();
                if (this.ItemCache.Count < dataArray.Length)
                {
                    for (int i = this.ItemCache.Count; i < dataArray.Length; i++)
                    {
                        this.ItemCache.Add(Instantiate(this.m_ItemPrefab, this.m_ParentLayout.transform, false));
                    }
                }
                else if (this.ItemCache.Count > dataArray.Length)
                {
                    for (int i = dataArray.Count(); i < this.ItemCache.Count; i++)
                    {
                        this.ItemCache[i].gameObject.SetActive(false);
                    }
                }
                    
                for (int i = 0; i < dataArray.Length; i++)
                {
                    this.ItemCache[i].Target = dataArray[i];
                    this.ItemCache[i].gameObject.SetActive(true);
                }
            }
            else
            {
                foreach(var item in this.ItemCache)
                {
                    item.gameObject.SetActive(false);
                }
            }
           
            this.m_Background.SetActive(showBackground);

            /*
            float height = this.ItemCache.Count(container => container.gameObject.activeSelf) * 0.055f;
            this.RectTransform.anchorMin = new Vector2(0.4f, 0);
            this.RectTransform.anchorMax = new Vector2(0.6f, height);
            this.RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0);
            this.RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
            */
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(this.RectTransform);
        }

        protected void SetIcon(ISpriteState state)
        {
            this.m_Icon.Clear();
            this.m_Icon.AddSpriteState(state, true);
        }
    }
}