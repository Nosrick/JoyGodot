﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Godot;
using Godot.Collections;
using JoyGodot.Assets.Scripts.Conversation.Conversations;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Relationships;
using JoyGodot.Assets.Scripts.Events;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Scripting;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace JoyGodot.Assets.Scripts.Conversation
{
    public class ConversationEngine : IConversationEngine
    {
        protected List<ITopic> m_Topics;
        protected List<ITopic> m_CurrentTopics;

        protected JSONValueExtractor ValueExtractor { get; set; }

        protected ITopic LastSaid { get; set; }

        public string LastSaidWords { get; protected set; }

        public IEntity Instigator { get; protected set; }

        public IEntity Listener { get; protected set; }

        public Guid Guid { get; protected set; }

        public string ListenerInfo { get; protected set; }

        public event ConversationEventHandler OnConverse;
        public event ConversationEventHandler OnOpen;
        public event EmptyEventHandler OnClose;

        protected IEntityRelationshipHandler RelationshipHandler { get; set; }

        public ConversationEngine(
            IEntityRelationshipHandler relationshipHandler,
            Guid guid)
        {
            this.ValueExtractor = new JSONValueExtractor();
            this.RelationshipHandler = relationshipHandler;

            this.m_Topics = this.LoadTopics();

            this.m_CurrentTopics = new List<ITopic>();

            this.Guid = guid;
        }

        protected List<ITopic> LoadTopics()
        {
            List<ITopic> topics = new List<ITopic>();

            string[] files = Directory.GetFiles(
                Directory.GetCurrentDirectory() +
                GlobalConstants.ASSETS_FOLDER +
                GlobalConstants.DATA_FOLDER +
                "Conversation",
                "*.json",
                SearchOption.AllDirectories);

            foreach (string file in files)
            {
                JSONParseResult result = JSON.Parse(File.ReadAllText(file));

                if (result.Error != Error.Ok)
                {
                    this.ValueExtractor.PrintFileParsingError(result, file);
                    continue;
                }

                if (!(result.Result is Dictionary dictionary))
                {
                    GlobalConstants.ActionLog.Log("Failed to parse JSON from " + file + " into a Dictionary.",
                        LogLevel.Warning);
                    continue;
                }

                Dictionary conversations =
                    this.ValueExtractor.GetValueFromDictionary<Dictionary>(dictionary, "Conversations");

                string name = this.ValueExtractor.GetValueFromDictionary<string>(conversations, "Name");

                ICollection<Dictionary> lines =
                    this.ValueExtractor.GetArrayValuesCollectionFromDictionary<Dictionary>(conversations, "Lines");

                foreach (Dictionary line in lines)
                {
                    string text = this.ValueExtractor.GetValueFromDictionary<string>(line, "Text");
                    string processorName = line.Contains("Processor")
                        ? this.ValueExtractor.GetValueFromDictionary<string>(line, "Processor")
                        : "";

                    ICollection<string> conditionStrings = line.Contains("Conditions")
                        ? this.ValueExtractor.GetArrayValuesCollectionFromDictionary<string>(line, "Conditions")
                        : new string[0];

                    ICollection<string> next = line.Contains("Next")
                        ? this.ValueExtractor.GetArrayValuesCollectionFromDictionary<string>(line, "Next")
                        : new string[0];

                    int priority = line.Contains("Priority")
                        ? this.ValueExtractor.GetValueFromDictionary<int>(line, "Priority")
                        : 0;

                    string speaker = line.Contains("Speaker")
                        ? this.ValueExtractor.GetValueFromDictionary<string>(line, "Speaker")
                        : "instigator";
                    string link = line.Contains("Link")
                        ? this.ValueExtractor.GetValueFromDictionary<string>(line, "Link")
                        : "";

                    Speaker speakerEnum = (Speaker) Enum.Parse(typeof(Speaker), speaker, true);

                    IEnumerable<ITopicCondition> conditions =
                        conditionStrings.Select(this.ParseCondition);

                    string[] actionStrings = line.Contains("Actions")
                        ? this.ValueExtractor.GetArrayValuesCollectionFromDictionary<string>(line, "Actions").ToArray()
                        : new string[0];

                    ICollection<string> tags = line.Contains("Tags")
                        ? this.ValueExtractor.GetArrayValuesCollectionFromDictionary<string>(line, "Tags").ToArray()
                        : new string[0];

                    IEnumerable<IJoyAction> actions = GlobalConstants.ScriptingEngine.FetchActions(actionStrings);

                    var processorBase = processorName.IsNullOrEmpty() ? null : GlobalConstants.ScriptingEngine.FetchAndInitialise(processorName);
                    if (processorBase is ITopic topicProcessor)
                    {
                        topicProcessor.Initialise(
                            conditions.ToArray(),
                            name,
                            next.ToArray(),
                            text,
                            priority,
                            actions,
                            speakerEnum,
                            link,
                            this,
                            this.RelationshipHandler);
                        topics.Add(topicProcessor);
                    }
                    else
                    {
                        topics.Add(new TopicData(
                            conditions.ToArray(),
                            name,
                            next.ToArray(),
                            text, 
                            tags,
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
            
            this.CalculateListenerInfo();

            //this.OnOpen?.Invoke();
        }

        protected void CalculateListenerInfo()
        {
            try
            {
                IEnumerable<IRelationship> relationships =
                    this.RelationshipHandler.Get(new[] {this.Instigator.Guid, this.Listener.Guid});

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

                this.ListenerInfo = this.Listener.JoyName + ", " + chosenRelationship?.DisplayName;
            }
            catch (Exception e)
            {
                this.ListenerInfo = this.Listener.JoyName + ", acquaintance.";
            }
        }

        public ICollection<ITopic> Converse(ITopic selectedTopic = null)
        {
            if (selectedTopic is null)
            {
                this.CurrentTopics = this.m_Topics
                    .Where(t => t.ID.Equals("greeting", StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                this.CurrentTopics = this.SanitiseTopics(this.CurrentTopics);
                var greeting = this.CurrentTopics.FirstOrDefault();
                this.DoInteractions(greeting);
                this.OnOpen?.Invoke(greeting, this.CurrentTopics);
            }
            else
            {
                this.DoInteractions(selectedTopic);
            }

            if (this.CurrentTopics.Length == 0)
            {
                this.OnClose?.Invoke();
            }
            else
            {
                this.CalculateListenerInfo();
            }

            return this.CurrentTopics;
        }

        protected void DoInteractions(ITopic currentTopic)
        {
            if (currentTopic is null)
            {
                this.OnClose?.Invoke();
                this.CurrentTopics = new ITopic[0];
                this.Listener = null;
                this.Instigator = null;
                return;
            }
            
            ITopic[] next = currentTopic.Interact(this.Instigator, this.Listener);

            next = this.SanitiseTopics(next);

            if (next.Length == 0)
            {
                this.OnClose?.Invoke();
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
                this.OnClose?.Invoke();
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
                List<Tuple<string, object>> tuples = new List<Tuple<string, object>>();

                foreach (ITopicCondition condition in conditions)
                {
                    tuples.AddRange(this.Listener.GetData(
                        new[] {condition.Criteria},
                        this.Listener));
                }

                if (topic.FulfilsConditions(tuples))
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

            for (int i = 0; i < topics.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(topics[i].Words) == false)
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
                string[] split = conditionString.Split(new char[] {'<', '>', '=', '!'},
                    StringSplitOptions.RemoveEmptyEntries);

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
            get { return this.m_CurrentTopics.ToArray(); }
            set { this.m_CurrentTopics = value.ToList(); }
        }

        public ITopic[] AllTopics => this.m_Topics.ToArray();
    }
}