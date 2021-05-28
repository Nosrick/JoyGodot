using System;
using System.Collections.Generic;
using System.Linq;
using JoyLib.Code.Entities;
using JoyLib.Code.Entities.Relationships;
using JoyLib.Code.Entities.Statistics;
using JoyLib.Code.Rollers;
using JoyLib.Code.Scripting;

namespace JoyLib.Code.Conversation.Conversations
{
    public class TopicData : ITopic
    {
        public ITopicCondition[] Conditions { get; protected set; }
        public string ID { get; protected set; }
        public string[] NextTopics { get; protected set; }
        public string Words { get; protected set; }
        public int Priority { get; protected set; }

        public Speaker Speaker { get; protected set; }

        public RNG Roller { get; protected set; }

        public string Link { get; protected set; }

        public IJoyAction[] CachedActions { get; protected set; }

        public IConversationEngine ConversationEngine { get; set; }

        public IEntityRelationshipHandler RelationshipHandler { get; set; }

        public TopicData(
            ITopicCondition[] conditions,
            string ID,
            string[] nextTopics,
            string words,
            int priority,
            IEnumerable<IJoyAction> cachedActions,
            Speaker speaker,
            RNG roller = null,
            string link = "",
            IConversationEngine conversationEngine = null,
            IEntityRelationshipHandler relationshipHandler = null)
        {
            this.Roller = roller ?? new RNG();

            this.Initialise(
                conditions,
                ID,
                nextTopics,
                words,
                priority,
                cachedActions,
                speaker,
                link,
                conversationEngine,
                relationshipHandler);
        }

        public string[] GetConditionTags()
        {
            return this.Conditions.Select(c => c.Criteria).ToArray();
        }

        public bool FulfilsConditions(IEnumerable<Tuple<string, object>> values)
        {
            bool any = values.Any();

            foreach (ITopicCondition condition in this.Conditions)
            {
                try
                {
                    if (!any)
                    {
                        if (condition.FulfillsCondition(0) == false)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (values.Any(
                            pair => pair.Item1.Equals(condition.Criteria, StringComparison.OrdinalIgnoreCase)) == false)
                        {
                            if (condition.FulfillsCondition(0) == false)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            int value = values.Where(pair =>
                                    pair.Item1.Equals(condition.Criteria, StringComparison.OrdinalIgnoreCase)
                                    && pair.Item2 is int)
                                .Select(pair => (int) pair.Item2)
                                .Max();

                            if (condition.FulfillsCondition(value) == false)
                            {
                                return false;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    //suppress this
                    return false;
                }
            }

            return true;
        }

        public bool FulfilsConditions(IEnumerable<JoyObject> participants)
        {
            string[] criteria = this.Conditions.Select(c => c.Criteria).ToArray();

            List<Tuple<string, object>> values = new List<Tuple<string, object>>();
            foreach (IJoyObject participant in participants)
            {
                if (participant is IEntity entity)
                {
                    IJoyObject[] others = participants.Where(p => p.Guid.Equals(participant.Guid) == false).ToArray();
                    values.AddRange(entity.GetData(criteria, others));
                }
            }

            return this.FulfilsConditions(values);
        }

        public void Initialise(
            ITopicCondition[] conditions,
            string ID,
            string[] nextTopics,
            string words,
            int priority,
            IEnumerable<IJoyAction> cachedActions,
            Speaker speaker,
            string link = "",
            IConversationEngine conversationEngine = null,
            IEntityRelationshipHandler relationshipHandler = null)
        {
            this.Conditions = conditions;
            this.ID = ID;
            this.NextTopics = nextTopics;
            this.Words = words;
            this.Priority = priority;

            List<IJoyAction> actions = cachedActions is null ? new List<IJoyAction>() : cachedActions.ToList();
            actions.AddRange(this.GetStandardActions());

            this.CachedActions = actions.ToArray();

            this.ConversationEngine = conversationEngine ?? GlobalConstants.GameManager?.ConversationEngine;
            this.RelationshipHandler = relationshipHandler ?? GlobalConstants.GameManager?.RelationshipHandler;

            this.Speaker = speaker;
            this.Link = link;
        }

        protected IJoyAction[] GetStandardActions()
        {
            string[] standardActions = {"fulfillneedaction", "modifyrelationshippointsaction"};
            IJoyAction[] actions = (ScriptingEngine.Instance.FetchActions(standardActions)).ToArray();

            return actions;
        }

        protected void GetBits()
        {
            if (this.ConversationEngine is null)
            {
                this.ConversationEngine = GlobalConstants.GameManager?.ConversationEngine;
            }

            if (this.RelationshipHandler is null)
            {
                this.RelationshipHandler = GlobalConstants.GameManager?.RelationshipHandler;
            }
        }

        public virtual ITopic[] Interact(IEntity instigator, IEntity listener)
        {
            this.GetBits();

            IJoyAction fulfillNeed = this.CachedActions.First(action =>
                action.Name.Equals("fulfillneedaction", StringComparison.OrdinalIgnoreCase));
            IJoyAction influence = this.CachedActions.First(action =>
                action.Name.Equals("modifyrelationshippointsaction", StringComparison.OrdinalIgnoreCase));

            fulfillNeed.Execute(
                new IJoyObject[] {instigator, listener},
                new[] {"friendship"},
                new Dictionary<string, object>
                {
                    {"need", "friendship"},
                    {"value", instigator.Statistics[EntityStatistic.PERSONALITY].Value},
                    {"counter", 0},
                    {"doAll", true}
                });

            string[] tags = this.RelationshipHandler is null
                ? new string[0]
                : this.RelationshipHandler.Get(
                    new IJoyObject[] {instigator, listener}).SelectMany(relationship => relationship.Tags).ToArray();

            influence.Execute(
                new IJoyObject[] {instigator, listener},
                tags,
                new Dictionary<string, object>
                {
                    {"value", instigator.Statistics[EntityStatistic.PERSONALITY].Value}
                });

            influence.Execute(
                new IJoyObject[] {listener, instigator},
                tags,
                new Dictionary<string, object>
                {
                    {"value", listener.Statistics[EntityStatistic.PERSONALITY].Value}
                });

            bool? isFamily = this.RelationshipHandler?.IsFamily(instigator, listener);

            if (isFamily is null == false && isFamily == true)
            {
                fulfillNeed.Execute(
                    new IJoyObject[] {instigator, listener},
                    new string[] {"family"},
                    new Dictionary<string, object>
                    {
                        {"need", "family"},
                        {"value", instigator.Statistics[EntityStatistic.PERSONALITY].Value},
                        {"counter", 0},
                        {"doAll", true}
                    });
            }

            return this.FetchNextTopics();
        }

        protected virtual ITopic[] FetchNextTopics()
        {
            List<ITopic> nextTopics = this.ConversationEngine.AllTopics
                .Where(topic => this.NextTopics.Contains(topic.ID))
                .ToList();

            return nextTopics.ToArray();
        }
    }
}