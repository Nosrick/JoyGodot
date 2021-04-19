﻿using JoyLib.Code.Conversation.Conversations;
using JoyLib.Code.Helpers;
using JoyLib.Code.Unity.GUI;

namespace JoyLib.Code.Entities.Conversation.Processors
{
    public class TradeProcessor : TopicData
    {
        //protected TradeWindow TradeWindow { get; set; }
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
            /*
            if (this.TradeWindow == null || this.GUIManager is null)
            {
                try
                {
                    this.GUIManager = GlobalConstants.GameManager.GUIManager;
                    this.TradeWindow = this.GUIManager.Get(GUINames.TRADE).GetComponent<TradeWindow>();
                }
                catch
                {
                    GlobalConstants.ActionLog.AddText("Could not load TradeProcessor bits. Trying again later.", LogLevel.Warning);
                }
            }
            */
        }

        public override ITopic[] Interact(IEntity instigator, IEntity listener)
        {
            this.Initialise();
            //this.TradeWindow.SetActors(instigator, listener);
            
            this.GUIManager.OpenGUI("Trade");
            
            return base.Interact(instigator, listener);
        }
    }
}