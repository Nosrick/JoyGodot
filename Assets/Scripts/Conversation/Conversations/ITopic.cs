using System;
using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Relationships;
using JoyGodot.Assets.Scripts.Rollers;
using JoyGodot.Assets.Scripts.Scripting;

namespace JoyGodot.Assets.Scripts.Conversation.Conversations
{
    public interface ITopic
    {
        ITopicCondition[] Conditions
        {
            get;
        }

        string ID
        {
            get;
        }

        string[] NextTopics
        {
            get;
        }

        string Words
        {
            get;
        }

        int Priority
        {
            get;
        }

        IJoyAction[] CachedActions
        {
            get;
        }

        Speaker Speaker
        {
            get;
        }

        string Link
        {
            get;
        }
        
        RNG Roller { get; }

        string[] GetConditionTags();

        bool FulfilsConditions(IEnumerable<Tuple<string, object>> values);
        bool FulfilsConditions(IEnumerable<JoyObject.JoyObject> participants);

        ITopic[] Interact(IEntity instigator, IEntity listener);

        void Initialise(
            ITopicCondition[] conditions,
            string ID,
            string[] nextTopics,
            string words,
            int priority,
            IEnumerable<IJoyAction> cachedActions,
            Speaker speaker,
            string link = "",
            IConversationEngine conversationEngine = null,
            IEntityRelationshipHandler relationshipHandler = null);
    }

    public class TopicComparer : IComparer<ITopic>
    {
        public int Compare(ITopic x, ITopic y)
        {
            return x.Priority.CompareTo(y.Priority);
        }
    }

    public enum Speaker
    {
        LISTENER,
        INSTIGATOR
    }
}
