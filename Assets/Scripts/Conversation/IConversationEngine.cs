using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Base_Interfaces;
using JoyGodot.Assets.Scripts.Conversation.Conversations;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Events;

namespace JoyGodot.Assets.Scripts.Conversation
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