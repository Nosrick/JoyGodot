using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Castle.Core.Internal;
using JoyLib.Code.Conversation.Conversations;
using JoyLib.Code.Entities;
using JoyLib.Code.Entities.Relationships;
using Directory = System.IO.Directory;

namespace JoyLib.Code.Conversation
{
    public class ConversationEngine : IConversationEngine
    {
        protected List<ITopic> m_Topics;
        protected List<ITopic> m_CurrentTopics;

        protected ITopic LastSaid
        {
            get;
            set;
        }

        public string LastSaidWords { get; protected set; }

        public IEntity Instigator
        {
            get;
            protected set;
        }

        public IEntity Listener
        {
            get;
            protected set;
        }

        public Guid Guid
        {
            get;
            protected set;
        }
        
        public string ListenerInfo { get; protected set; }

        public event EventHandler OnConverse;
        public event EventHandler OnOpen;
        public event EventHandler OnClose;
        
        protected IEntityRelationshipHandler RelationshipHandler { get; set; }

        public ConversationEngine(
            IEntityRelationshipHandler relationshipHandler,
            Guid guid)
        {
            this.RelationshipHandler = relationshipHandler;

            this.m_Topics = this.LoadTopics();

            this.m_CurrentTopics = new List<ITopic>();

            this.Guid = guid;
        }

        protected List<ITopic> LoadTopics()
        {
            List<ITopic> topics = new List<ITopic>();

            string[] files = Directory.GetFiles(
                Directory.GetCurrentDirectory() + GlobalConstants.DATA_FOLDER + "Conversation",
                "*.json",
                SearchOption.AllDirectories);

            foreach (string file in files)
            {
                /*
                using (StreamReader reader = new StreamReader(file))
                {
                    using (JsonTextReader jsonReader = new JsonTextReader(reader))
                    {
                        try
                        {
                            JObject jToken = JObject.Load(jsonReader);

                            if (jToken.IsNullOrEmpty())
                            {
                                continue;
                            }

                            foreach (JToken child in jToken.Values())
                            {
                                string topicName = (string) child["Name"];
                                foreach (JToken line in child["Lines"])
                                {
                                    string text = (string) line["Text"];
                                    string processor = ((string) line["Processor"]) ?? "NONE";

                                    string[] conditionStrings = line["Conditions"] is null 
                                        ? new string[0]
                                        : line["Conditions"].Select(token => (string) token).ToArray();

                                    string[] nextTopics = line["Next"] is null
                                        ? new string[0]
                                        : line["Next"].Select(token => (string) token).ToArray();

                                    int priority = (int) (line["Priority"] ?? 0);

                                    string speaker = (string) line["Speaker"] ?? "instigator";

                                    string link = (string) line["Link"] ?? "";

                                    Speaker speakerEnum = (Speaker) Enum.Parse(typeof(Speaker), speaker, true);

                                    List<ITopicCondition> conditions = new List<ITopicCondition>();
                                    foreach (string condition in conditionStrings)
                                    {
                                        conditions.Add(this.ParseCondition(condition));
                                    }

                                    string[] actionStrings = line["Actions"] is null
                                        ? new string[0]
                                        : line["Actions"].Select(token => (string) token).ToArray();

                                    IEnumerable<IJoyAction> actions = ScriptingEngine.Instance.FetchActions(actionStrings);

                                    if (processor.Equals("NONE", StringComparison.OrdinalIgnoreCase) == false)
                                    {
                                        try
                                        {
                                            ITopic processorObject = (ITopic)ScriptingEngine.Instance.FetchAndInitialise(processor);
                                            processorObject.Initialise(
                                                conditions.ToArray(),
                                                topicName,
                                                nextTopics.ToArray(),
                                                text,
                                                priority,
                                                actions,
                                                speakerEnum,
                                                link);
                                            
                                            topics.Add(processorObject);
                                        }
                                        catch
                                        {
                                            GlobalConstants.ActionLog.AddText("Could not find topic processor " + processor);
                                            topics.Add(new TopicData(
                                                conditions.ToArray(),
                                                topicName,
                                                nextTopics.ToArray(),
                                                text,
                                                priority,
                                                actions,
                                                speakerEnum,
                                                null,
                                                link));
                                        }
                                    }
                                    else
                                    {
                                        topics.Add(new TopicData(
                                                conditions.ToArray(),
                                                topicName,
                                                nextTopics.ToArray(),
                                                text,
                                                priority,
                                                actions,
                                                speakerEnum,
                                                null,
                                                link,
                                                this,
                                                this.RelationshipHandler));
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            GlobalConstants.ActionLog.AddText("Could not load conversations for " + file);
                            GlobalConstants.ActionLog.StackTrace(e);
                        }
                    }
                }
                    */
            }

            topics = this.PerformLinks(topics);

            return topics;
        }

        protected List<ITopic> PerformLinks(List<ITopic> topics)
        {
            List<ITopic> linked = new List<ITopic>(topics);

            foreach (ITopic topic in topics)
            {
                if (topic.Link.IsNullOrEmpty() == false)
                {
                    ITopic[] links = topics.Where(left =>
                            topics.Any(right => right.Link.Equals(left.ID, StringComparison.OrdinalIgnoreCase)))
                        .ToArray();

                    foreach (ITopic link in links)
                    {
                        link.Initialise(
                            link.Conditions,
                            topic.ID,
                            link.NextTopics,
                            link.Words,
                            link.Priority,
                            link.CachedActions,
                            link.Speaker,
                            link.Link,
                            this,
                            this.RelationshipHandler);
                    }
                    linked.Remove(topic);
                }
            }
            
            return linked;
        }

        public void SetActors(IEntity instigator, IEntity listener)
        {
            this.Instigator = instigator;
            this.Listener = listener;

            try
            {
                IEnumerable<IRelationship> relationships = this.RelationshipHandler.Get(new IJoyObject[] {this.Instigator, this.Listener});

                IRelationship chosenRelationship = null;
                int best = Int32.MinValue;
                foreach (IRelationship relationship in relationships)
                {
                    if (relationship.HasTag("romantic"))
                    {
                        chosenRelationship = relationship;
                        break;
                    }
                    
                    int value = relationship.GetRelationshipValue(this.Instigator.Guid, this.Listener.Guid);
                    if (value > best)
                    {
                        best = value;
                        chosenRelationship = relationship;
                    }
                }

                this.ListenerInfo = this.Listener.JoyName + ", " + chosenRelationship.DisplayName;
            }
            catch (Exception e)
            {
                this.ListenerInfo = this.Listener.JoyName + ", acquaintance.";
            }

            this.OnOpen?.Invoke(this, EventArgs.Empty);
        }

        public ITopic[] Converse(string topic, int index = 0)
        {
            if (this.CurrentTopics.Length == 0)
            {
                this.CurrentTopics = this.m_Topics.Where(t => t.ID.Equals(topic, StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                this.CurrentTopics = this.SanitiseTopics(this.CurrentTopics);
            }
            
            ITopic currentTopic = this.CurrentTopics[index];

            this.DoInteractions(currentTopic);

            if (this.Instigator is null == false && this.Listener is null == false)
            {
                this.SetActors(this.Instigator, this.Listener);
                this.OnConverse?.Invoke(this, EventArgs.Empty);
            }
            return this.CurrentTopics;
        }

        public ITopic[] Converse(int index = 0)
        {
            return this.Converse("greeting", index);
        }

        protected void DoInteractions(ITopic currentTopic)
        {
            ITopic[] next = currentTopic.Interact(this.Instigator, this.Listener);

            next = this.SanitiseTopics(next);

            if (next.Length == 0)
            {
                this.OnClose?.Invoke(this, EventArgs.Empty);
                this.CurrentTopics = next;
                this.Listener = null;
                this.Instigator = null;
                return;
            }
            
            switch (currentTopic.Speaker)
            {
                case Speaker.LISTENER:
                    this.LastSaid = currentTopic;
                    this.LastSaidWords = this.LastSaid.Words;
                    break;
                
                case Speaker.INSTIGATOR:
                    currentTopic = next[0];
                    if (currentTopic.Speaker == Speaker.LISTENER)
                    {
                        next = currentTopic.Interact(this.Instigator, this.Listener);
                        next = this.SanitiseTopics(next);
                        this.LastSaid = currentTopic;
                        this.LastSaidWords = this.LastSaid.Words;
                    }
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }

            this.CurrentTopics = next;

            if (next.Length == 0)
            {
                this.OnClose?.Invoke(this, EventArgs.Empty);
                this.CurrentTopics = next;
                this.Listener = null;
                this.Instigator = null;
            }
        }

        protected ITopic[] SanitiseTopics(ITopic[] topics)
        {
            ITopic[] next = topics;
            next = this.GetValidTopics(next);
            next = this.TrimEmpty(next);
            next = this.SortByPriority(next);

            return next;
        }

        protected ITopic[] GetValidTopics(ITopic[] topics)
        {
            List<ITopic> validTopics = new List<ITopic>();
            foreach (ITopic topic in topics)
            {
                ITopicCondition[] conditions = topic.Conditions;
                List<Tuple<string, int>> tuples = new List<Tuple<string, int>>();

                foreach (ITopicCondition condition in conditions)
                {
                    tuples.AddRange(this.Listener.GetData(
                        new [] {condition.Criteria}, 
                        this.Listener));
                }

                if(topic.FulfilsConditions(tuples.ToArray()))
                {
                    validTopics.Add(topic);
                }
            }

            return validTopics.ToArray();
        }

        protected ITopic[] SortByPriority(ITopic[] topics)
        {
            List<ITopic> sorting = new List<ITopic>(topics);

            return sorting.OrderByDescending(t => t.Priority).ToArray();
        }

        

        protected ITopic[] TrimEmpty(ITopic[] topics)
        {
            List<ITopic> newTopics = new List<ITopic>(topics.Length);

            for(int i = 0; i < topics.Length; i++)
            {
                if(string.IsNullOrWhiteSpace(topics[i].Words) == false)
                {
                    newTopics.Add(topics[i]);
                }
            }

            return newTopics.ToArray();
        }

        protected ITopicCondition ParseCondition(string conditionString)
        {
            try
            {
                string[] split = conditionString.Split(new char[] {'<', '>', '=', '!'}, StringSplitOptions.RemoveEmptyEntries);

                string criteria = split[0].Trim();
                string operand = conditionString.First(c => c.Equals('!')
                                                            || c.Equals('=')
                                                            || c.Equals('<')
                                                            || c.Equals('>')).ToString();
                string stringValue = split[1].Trim();
            
                TopicConditionFactory factory = new TopicConditionFactory();

                int value = int.MinValue;
                if (!int.TryParse(stringValue, out value))
                {
                    value = 1;
                    criteria = stringValue;
                }

                return factory.Create(criteria, operand, value);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Could not parse conversation condition line " + conditionString);
            }
        }

        ~ConversationEngine()
        {
            GlobalConstants.GameManager.GUIDManager.ReleaseGUID(this.Guid);
        }

        public ITopic[] CurrentTopics
        {
            get
            {
                return this.m_CurrentTopics.ToArray();
            }
            set
            {
                this.m_CurrentTopics = value.ToList();
            }
        }

        public ITopic[] AllTopics => this.m_Topics.ToArray();
    }
}
