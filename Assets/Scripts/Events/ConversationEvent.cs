using System;
using System.Collections.Generic;
using JoyLib.Code.Conversation.Conversations;

namespace JoyLib.Code.Events
{
    public delegate void ConversationEventHandler(ITopic selectedTopic, ICollection<ITopic> currentTopics);
    
    public class ConversationEventArgs : EventArgs
    {
        public ITopic LastTopic { get; set; }
        public ICollection<ITopic> CurrentTopics { get; set; }
    }
}