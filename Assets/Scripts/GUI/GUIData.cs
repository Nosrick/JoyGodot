using JoyLib.Code.Events;
using UnityEngine;
using UnityEngine.EventSystems;

namespace JoyLib.Code.Unity.GUI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public class GUIData : MonoBehaviour, IPointerDownHandler
    {
        protected IGUIManager m_GUIManager;
        
        public Canvas MyCanvas { get; protected set; }
        
        public int DefaultSortingOrder { get; protected set; }

        public IGUIManager GUIManager
        {
            get => this.m_GUIManager ?? (this.m_GUIManager = GlobalConstants.GameManager.GUIManager);
            set
            {
                this.m_GUIManager = value;
                foreach (GUIData data in this.GetComponentsInChildren<GUIData>())
                {
                    data.m_GUIManager = value;
                }
            }
        }

        public virtual void Awake()
        {
            this.MyCanvas = this.GetComponent<Canvas>();
            this.DefaultSortingOrder = this.MyCanvas.sortingOrder;

            this.MyCanvas.sortingLayerName = "GUI";
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            this.GUIManager.BringToFront(this.name);
        }

        public virtual void Show()
        {
            this.gameObject.SetActive(true);
            GUIData[] children = this.gameObject.GetComponentsInChildren<GUIData>(true);
            foreach (GUIData child in children)
            {
                if (child.Equals(this) == false)
                {
                    child.Show();
                }
            }

            this.OnGUIOpen?.Invoke(this);
        }

        public virtual void Close()
        {
            if (this.m_AlwaysOpen)
            {
                return;
            }
            
            this.gameObject.SetActive(false);
            GUIData[] children = this.gameObject.GetComponentsInChildren<GUIData>(true);
            foreach (GUIData child in children)
            {
                if (child.Equals(this) == false)
                {
                    child.Close();
                }
            }

            this.OnGUIClose?.Invoke(this);
        }

        public virtual void ButtonClose()
        {
            this.GUIManager.CloseGUI(this.name);
        }

        public bool m_RemovesControl;

        public bool m_ClosesOthers;

        public bool m_AlwaysOpen;

        public bool m_AlwaysOnTop;

        public virtual event GUIClosedEventHandler OnGUIClose;
        public virtual event GUIOpenedEventHandler OnGUIOpen;
    }
}