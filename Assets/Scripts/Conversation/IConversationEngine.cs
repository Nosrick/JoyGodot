using System;
using System.Collections.Generic;
using JoyLib.Code.Conversation.Conversations;
using JoyLib.Code.Entities;
using JoyLib.Code.Events;

namespace JoyLib.Code.Conversation
{
    public interface IConversationEngine : IGuidHolder
    {
        void SetActors(IEntity instigator, IEntity listener);
        ICollection<ITopic> Converse(ITopic selectedTopic = null);
        
        ITopic[] CurrentTopics { get; }
        ITopic[] AllTopics { get; }
        IEntity Instigator { get; }
        IEntity Listener { get; }

        string ListenerInfo { get; }
        string LastSaidWords { get; }

        event ConversationEventHandler OnConverse;
        event ConversationEventHandler OnOpen;
        event EmptyEventHandler OnClose;
    }
}