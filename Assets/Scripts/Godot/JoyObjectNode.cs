using Castle.Core.Internal;
using Godot;
using JoyLib.Code.Unity;
using JoyLib.Code.Unity.GUI;

namespace JoyLib.Code.Godot
{
    public class JoyObjectNode : Node2D
    {
        protected IJoyObject JoyObject { get; set; }

        protected GUIManager GuiManager { get; set; }
        
        public ManagedSprite Icon { get; protected set; }

        public JoyObjectNode()
        {
            this.Icon = new ManagedSprite();
            this.AddChild(this.Icon);
            this.Icon.Awake();
        }

        public JoyObjectNode(IJoyObject joyObject)
            : this()
        {
            this.AttachJoyObject(joyObject);
            //this.GuiManager = GlobalConstants
        }

        public void AttachJoyObject(IJoyObject joyObject)
        {
            this.JoyObject = joyObject;
            this.Icon.Clear();
            this.Icon.AddSpriteState(this.JoyObject.States[0]);
        }

        public void OnPointerEnter()
        {
            if (this.JoyObject.Tooltip.IsNullOrEmpty())
            {
                return;
            }
            
            Tooltip tooltip = (Tooltip) this.GuiManager.OpenGUI(GUINames.TOOLTIP);
            tooltip.Show(
                this.JoyObject.JoyName,
                null,
                this.JoyObject.States[0],
                this.JoyObject.Tooltip);
        }

        public void OnPointerExit()
        {
            this.GuiManager.CloseGUI(GUINames.TOOLTIP);
        }
    }
}