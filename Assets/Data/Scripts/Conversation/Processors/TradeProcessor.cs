using JoyGodot.Assets.Scripts;
using JoyGodot.Assets.Scripts.Conversation.Conversations;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.GUI;
using JoyGodot.Assets.Scripts.GUI.WorldState;
using JoyGodot.Assets.Scripts.Helpers;

namespace JoyGodot.Assets.Data.Scripts.Conversation.Processors
{
    public class TradeProcessor : TopicData
    {
        protected TradeWindow TradeWindow { get; set; }
        protected IGUIManager GUIManager { get; set; }

        public TradeProcessor()
            : base(
                new ITopicCondition[0],
                "TradeTopic",
                new string[0],
                "words",
                0,
                null,
                Speaker.INSTIGATOR)
        {
            this.Initialise();
        }

        protected void Initialise()
        {
            if (this.TradeWindow is null || this.GUIManager is null)
            {
                try
                {
                    this.GUIManager = GlobalConstants.GameManager?.GUIManager;
                    //this.TradeWindow = this.GUIManager?.Get(GUINames.TRADE) as TradeWindow;
                }
                catch
                {
                }
            }
        }

        public override ITopic[] Interact(IEntity instigator, IEntity listener)
        {
            this.Initialise();
            //this.TradeWindow?.SetActors(instigator, listener);
            
            this.GUIManager.OpenGUI(this, GUINames.TRADE);
            
            return base.Interact(instigator, listener);
        }
    }
}