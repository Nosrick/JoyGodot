using Godot;
using Godot.Collections;
using JoyLib.Code.Events;

namespace JoyLib.Code.Unity.GUI
{
    public class GUIData : Control
    {
        protected IGUIManager m_GUIManager;

        public int DefaultSortingOrder { get; protected set; }

        public IGUIManager GUIManager
        {
            get => this.m_GUIManager; /*?? (this.m_GUIManager = GlobalConstants.GameManager.GUIManager)*/
            set
            {
                this.m_GUIManager = value;
                Array children = this.GetChildren();
                foreach (var child in children)
                {
                    if (child is GUIData data)
                    {
                        data.GUIManager = value;
                    }
                }
            }
        }

        public override void _Input(InputEvent @event)
        {
            base._Input(@event);
            if (@event.IsAction("ui_accept"))
            {
                this.GUIManager.BringToFront(this.Name);
            }
        }

        public virtual void Display()
        {
            this.Visible = true;
            Array children = this.GetChildren();
            foreach (var child in children)
            {
                if (child is GUIData data)
                {
                    data.Show();
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
            
            this.Visible = false;
            Array children = this.GetChildren();
            foreach (var child in children)
            {
                if (child is GUIData data)
                {
                    data.Close();
                }
            }

            this.OnGUIClose?.Invoke(this);
        }

        public override void _Ready()
        {
            this.GUIManager = GlobalConstants.GameManager.GUIManager;
            this.GUIManager.Add(this);
        }

        public virtual void ButtonClose()
        {
            this.GUIManager.CloseGUI(this.Name);
        }

        public bool m_RemovesControl;

        public bool m_ClosesOthers;

        public bool m_AlwaysOpen;

        public bool m_AlwaysOnTop;

        public virtual event GUIClosedEventHandler OnGUIClose;
        public virtual event GUIOpenedEventHandler OnGUIOpen;
    }
}