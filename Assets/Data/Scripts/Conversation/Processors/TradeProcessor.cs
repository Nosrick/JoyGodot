using Godot;
using JoyGodot.Assets.Scripts;
using JoyGodot.Assets.Scripts.Conversation.Conversations;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.GUI;
using JoyGodot.Assets.Scripts.GUI.Inventory_System;
using JoyGodot.Assets.Scripts.GUI.SettingsScreen;
using JoyGodot.Assets.Scripts.GUI.WorldState;

namespace JoyGodot.Assets.Data.Scripts.Conversation.Processors
{
    public class TradeProcessor : TopicData
    {
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
        }

        public override ITopic[] Interact(IEntity instigator, IEntity listener)
        {
            //this.TradeWindow?.SetActors(instigator, listener);
            
            var tradeWindow = GlobalConstants.GameManager.GUIManager.Get<TradeWindow>(GUINames.TRADE);
            tradeWindow.Left = instigator;
            tradeWindow.Right = listener;
            GlobalConstants.GameManager.GUIManager.OpenGUI(this, GUINames.TRADE);
            
            return base.Interact(instigator, listener);
        }
    }
}