using System.Collections.Generic;
using JoyLib.Code.Graphics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JoyLib.Code.Unity.GUI
{
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(GUIData))]
    public class Cursor : MonoBehaviour
    {
        [SerializeField] protected ManagedUISprite m_PartPrefab;
        
        protected ManagedUISprite CursorObject { get; set; }
        protected CanvasGroup CanvasGroup { get; set; }
        protected RectTransform MyRect { get; set; }

        protected bool Initialised { get; set; }
        protected ManagedUISprite DragObject { get; set; }
        
        protected Canvas Canvas { get; set; }
        protected Camera MainCamera { get; set; }

        public void Awake()
        {
            this.CanvasGroup = this.GetComponent<CanvasGroup>();
            this.MyRect = this.GetComponent<RectTransform>();
            this.Canvas = this.GetComponent<Canvas>();
            this.MainCamera = Camera.main;

            this.Canvas.sortingLayerName = "GUI";

            if (this.DragObject is null)
            {
                this.DragObject = Instantiate(this.m_PartPrefab, this.transform);
                this.DragObject.Awake();
                this.DragObject.gameObject.SetActive(false);
                RectTransform dragRect = this.DragObject.GetComponent<RectTransform>();
                dragRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, this.MyRect.rect.width * 2);
                dragRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, this.MyRect.rect.height * 2);
            }
            
            UnityEngine.Cursor.visible = false;

            if (this.CursorObject is null)
            {
                this.CursorObject = Instantiate(this.m_PartPrefab, this.transform);
                this.CursorObject.Awake();
            }

            this.Initialised = true;
        }

        public void Update()
        {
            if (!this.enabled)
            {
                return;
            }

            Rect rect = this.MyRect.rect;
            // this.transform.position = Mouse.current.position.ReadValue() + new Vector2(rect.width / 4, -(rect.height / 4));
            
            Vector3 mousePosition = Mouse.current.position.ReadValue();
            
            switch (this.Canvas.renderMode)
            {
                case RenderMode.ScreenSpaceOverlay:
                    break;
                
                case RenderMode.ScreenSpaceCamera:
                    mousePosition = this.MainCamera.ScreenToWorldPoint(mousePosition);
                    break;
                
                case RenderMode.WorldSpace:
                    mousePosition = this.MainCamera.ScreenToWorldPoint(mousePosition);
                    break;
            }
            
            this.transform.position = new Vector3(mousePosition.x, mousePosition.y, this.transform.position.z);
        }

        public void SetCursorSize(int width, int height)
        {
            if (this.Initialised == false)
            {
                this.Awake();
            }
            this.MyRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            this.MyRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            RectTransform cursorRect = this.CursorObject.GetComponent<RectTransform>();
            cursorRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            cursorRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            RectTransform dragRect = this.DragObject.GetComponent<RectTransform>();
            dragRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width * 2);
            dragRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height * 2);
        }

        public void SetCursorSprites(ISpriteState state)
        {
            if (this.Initialised == false)
            {
                this.Awake();
            }
            this.CursorObject.gameObject.SetActive(true);
            this.CursorObject.Clear();
            this.CursorObject.AddSpriteState(state, true);
        }

        public void SetCursorColours(IDictionary<string, Color> colours)
        {
            if (this.Initialised == false)
            {
                this.Awake();
            }
            foreach (ISpriteState state in this.CursorObject.States)
            {
                for (int j = 0; j < state.SpriteData.m_Parts.Count; j++)
                {
                    SpritePart part = state.SpriteData.m_Parts[j];
                    if (!colours.ContainsKey(part.m_Name))
                    {
                        continue;
                    }

                    part.m_PossibleColours = new List<Color> {colours[part.m_Name]};
                    state.SpriteData.m_Parts[j] = part;
                    this.CursorObject.Clear();
                }
                this.CursorObject.Clear();
                this.CursorObject.AddSpriteState(state);
            }
        }

        public void Show(ISpriteState replacement)
        {
            if (this.Initialised == false)
            {
                this.Awake();
            }
            this.DragObject.Clear();
            this.DragObject.gameObject.SetActive(false);
            if (replacement is null)
            {
                return;
            }
            this.DragObject.gameObject.SetActive(true);
            this.DragObject.AddSpriteState(replacement, true);
        }

        public void Reset()
        {
            if (this.Initialised == false)
            {
                this.Awake();
            }
            this.DragObject.Clear();
        }
    }
}