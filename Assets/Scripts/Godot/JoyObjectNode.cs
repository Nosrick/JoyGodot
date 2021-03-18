using Castle.Core.Internal;
using JoyLib.Code.Unity;
using JoyLib.Code.Unity.GUI;

namespace JoyLib.Code.Godot
{
    public class JoyObjectNode : ManagedSprite
    {
        protected IJoyObject JoyObject { get; set; }

        protected IGUIManager GuiManager { get; set; }

        public JoyObjectNode()
        {
            this.Awake();
        }

        public JoyObjectNode(IJoyObject joyObject)
            : this()
        {
            this.AttachJoyObject(joyObject);
            this.GuiManager = GlobalConstants.GameManager.GUIManager;
        }

        public void AttachJoyObject(IJoyObject joyObject)
        {
            this.JoyObject = joyObject;
            this.Clear();
            this.AddSpriteState(this.JoyObject.States[0]);
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