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
                this.GUIManager?.BringToFront(this.Name);
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
                    data.Display();
                }
            }

            this.OnGUIOpen?.Invoke(this);
        }

        public virtual bool Close(object sender)
        {
            if (this.AlwaysOpen)
            {
                return false;
            }
            
            this.Visible = false;
            Array children = this.GetChildren();
            foreach (var child in children)
            {
                if (child is GUIData data)
                {
                    data.Close(sender);
                }
            }

            this.OnGUIClose?.Invoke(this);
            return true;
        }

        public override void _Ready()
        {
            this.GUIManager = GlobalConstants.GameManager.GUIManager;
            //this.GUIManager.Add(this);
        }

        public virtual void ButtonClose()
        {
            this.GUIManager.CloseGUI(this, this.Name);
        }

        [Export] public bool RemovesControl { get; protected set; }

        [Export] public bool ClosesOthers { get; protected set; }

        [Export] public bool AlwaysOpen { get; protected set; }

        [Export] public bool AlwaysOnTop { get; protected set; }

        public virtual event GUIClosedEventHandler OnGUIClose;
        public virtual event GUIOpenedEventHandler OnGUIOpen;
    }
}