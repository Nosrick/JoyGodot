using System;
using JoyLib.Code.Conversation.Conversations;
using JoyLib.Code.Entities;

namespace JoyLib.Code.Conversation
{
    public interface IConversationEngine : IGuidHolder
    {
        void SetActors(IEntity instigator, IEntity listener);
        ITopic[] Converse(string topic, int index = 0);
        ITopic[] Converse(int index = 0);
        
        ITopic[] CurrentTopics { get; }
        ITopic[] AllTopics { get; }
        IEntity Instigator { get; }
        IEntity Listener { get; }

        string ListenerInfo { get; }
        string LastSaidWords { get; }

        event EventHandler OnConverse;
        event EventHandler OnOpen;
        event EventHandler OnClose;
    }
}