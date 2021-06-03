using Godot;
using JoyGodot.addons.Managed_Assets;
using JoyLib.Code.Conversation.Conversations;

namespace JoyGodot.Assets.Scripts.GUI.WorldState
{
    public delegate void ConversationMenuItemClick(ITopic myTopic);
    
#if TOOLS
    [Tool]
#endif
    public class ConversationMenuItem : ManagedTextButton
    {
        public ITopic MyTopic { get; set; }

        public event ConversationMenuItemClick OnClick;

        protected override void Press()
        {
            base.Press();

            if (this.Pressed)
            {
                this.OnClick?.Invoke(this.MyTopic);
            }
        }
    }
}