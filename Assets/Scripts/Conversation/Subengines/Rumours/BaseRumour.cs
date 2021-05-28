using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core;
using Castle.Core.Internal;
using JoyLib.Code.Conversation.Subengines.Rumours;
using JoyLib.Code.Entities;

namespace JoyLib.Code.Conversation.Conversations.Rumours
{
    public class BaseRumour : IRumour
    {
        public const int DEFAULT_LIFETIME = 5000;
        public IJoyObject[] Participants { get; protected set; }
        public string[] Tags { get; protected set; }
        public float ViralPotential { get; protected set; }
        
        public float LifetimeMultiplier { get; protected set; }
        public ITopicCondition[] Conditions { get; protected set; }
        public string[] Parameters { get; protected set; }

        public int Lifetime { get; protected set; }

        public bool IsAlive => this.Lifetime > 0;

        public string Words
        {
            get
            {
                if (this.m_Words.Contains("<") == false)
                {
                    return this.m_Words;
                }

                this.m_Words = this.ConstructString();
                return this.m_Words;
            }
            protected set
            {
                this.m_Words = value;
            }
        }
        public bool Baseless { get; protected set; }

        protected string m_Words;

        protected IParameterProcessorHandler ProcessorHandler
        {
            get;
            set;
        }

        public BaseRumour()
        {
            this.Initialise();
        }

        public BaseRumour(
            IEnumerable<IJoyObject> participants,
            IEnumerable<string> tags,
            float viralPotential,
            IEnumerable<ITopicCondition> conditions,
            IEnumerable<string> parameters,
            string words,
            float lifetimeMultiplier = 1f,
            int lifetime = DEFAULT_LIFETIME,
            bool baseless = false)
        {
            this.Participants = participants is null ? new IJoyObject[0] : participants.ToArray();
            this.Tags = tags is null ? new string[0] : tags.ToArray();
            this.ViralPotential = viralPotential;
            this.Conditions = conditions is null ? new ITopicCondition[0] : conditions.ToArray();
            this.Parameters = parameters is null ? new string[0] : parameters.ToArray();
            this.Words = words;
            this.LifetimeMultiplier = lifetimeMultiplier;
            this.Lifetime = (int)Math.Ceiling(lifetime * this.LifetimeMultiplier);
            this.Baseless = baseless;

            this.Initialise();
        }

        protected void Initialise()
        {
            if (GlobalConstants.GameManager is null == false && this.ProcessorHandler is null)
            {
                this.ProcessorHandler = GlobalConstants.GameManager.ParameterProcessorHandler;
            }
        }

        public bool FulfilsConditions(IEnumerable<Tuple<string, object>> values)
        {
            if (this.Baseless)
            {
                return true;
            }
            
            foreach (ITopicCondition condition in this.Conditions)
            {
                if (values.Any() == false)
                {
                    if (condition.FulfillsCondition(0) == false)
                    {
                        return false;
                    }
                }
                else
                {
                    if (values.Any(pair => pair.Item1.Equals(condition.Criteria)) == false)
                    {
                        if (condition.FulfillsCondition(0) == false)
                        {
                            return false;
                        }
                    }
                    
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

            return true;
        }
        
        public bool FulfilsConditions(IEnumerable<IJoyObject> participants)
        {
            if (this.Baseless)
            {
                return true;
            }
            
            string[] criteria = this.Conditions.Select(c => c.Criteria).ToArray();

            List<Tuple<string, object>> values = new List<Tuple<string, object>>();
            foreach (IJoyObject participant in participants)
            {
                if (participant is Entity entity)
                {
                    IJoyObject[] others = participants.Where(p => p.Guid.Equals(participant.Guid) == false).ToArray();
                    values.AddRange(entity.GetData(criteria, others));                    
                }
            }

            return this.FulfilsConditions(values);
        }

        public int Tick()
        {
            return --this.Lifetime;
        }

        public string ConstructString()
        {
            if (this.Participants.IsNullOrEmpty())
            {
                return this.m_Words;
            }
            
            int count = 0;
            for (int i = 1; i <= this.Parameters.Length; i++)
            {
                if (this.m_Words.Contains("<" + i + ">"))
                {
                    count++;
                }
            }
            
            if (count != this.Parameters.Length)
            {
                this.m_Words = "PARAMETER NUMBER MISMATCH. SOMEONE ENTERED THE WRONG NUMBER OF PARAMETER REPLACEMENTS.";
                return this.m_Words;
            }

            int participantNumber = 0;
            IJoyObject obj = null;
            for (int i = 0; i < count; i++)
            {
                if (this.Parameters[i].Equals("participant", StringComparison.OrdinalIgnoreCase))
                {
                    if (participantNumber >= this.Participants.Length)
                    {
                        this.m_Words = "PARTICIPANT/PARAMETER COUNT MISMATCH.";
                    }
                    obj = this.Participants[participantNumber];
                    participantNumber++;
                }

                string replacement = this.ProcessorHandler?
                    .Get(this.Parameters[i])
                    .Parse(this.Parameters[i], obj);

                this.m_Words = this.m_Words.Replace("<" + (i + 1) + ">", replacement);
            }

            return this.m_Words;
        }

        public IRumour Create(
            IEnumerable<IJoyObject> participants,
            IEnumerable<string> tags,
            float viralPotential,
            IEnumerable<ITopicCondition> conditions,
            IEnumerable<string> parameters,
            string words,
            float lifetimeMultiplier = 1F,
            int lifetime = 5000,
            bool baseless = false)
        {
            return new BaseRumour(
                participants,
                tags,
                viralPotential,
                conditions,
                parameters,
                words,
                lifetimeMultiplier,
                lifetime,
                baseless);
        }
    }
}