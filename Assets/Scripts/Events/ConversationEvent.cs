using System;
using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Conversation.Conversations;

namespace JoyGodot.Assets.Scripts.Events
{
    public delegate void ConversationEventHandler(ITopic selectedTopic, ICollection<ITopic> currentTopics);
    
    public class ConversationEventArgs : EventArgs
    {
        public ITopic LastTopic { get; set; }
        public ICollection<ITopic> CurrentTopics { get; set; }
    }
}