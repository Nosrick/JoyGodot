using System;
using System.Collections.Generic;
using System.Linq;
using JoyGodot.Assets.Scripts;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Needs;
using JoyGodot.Assets.Scripts.Entities.Relationships;
using JoyGodot.Assets.Scripts.Entities.Statistics;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Managed_Assets;
using JoyGodot.Assets.Scripts.Quests;

namespace JoyGodot.Assets.Data.Scripts.Needs
{
    public class Purpose : AbstractNeed
    {
        public override string Name => "purpose";

        protected const int DECAY_MIN = 4;
        protected const int DECAY_MAX = 128;

        protected const int PRIORITY_MIN = 0;
        protected const int PRIORITY_MAX = 12;

        protected const int HAPPINESS_THRESHOLD_MIN = 0;
        protected const int HAPPINESS_THRESHOLD_MAX = 24;

        protected const int MAX_VALUE_MIN = HAPPINESS_THRESHOLD_MAX;
        protected const int MAX_VALUE_MAX = MAX_VALUE_MIN * 4;

        protected IEntityRelationshipHandler RelationshipHandler
        {
            get;
            set;
        }

        protected IQuestProvider QuestProvider
        {
            get;
            set;
        }

        public Purpose()
        : base(
            1,
            1,
            true,
            1,
            1,
            1,
            1,
            new string[0])
        {
        }

        public Purpose(
            int decayRef, 
            int decayCounterRef, 
            bool doesDecayRef, 
            int priorityRef, 
            int happinessThresholdRef, 
            int valueRef, 
            int maxValueRef,
            ISpriteState fulfillingSprite,
            IEntityRelationshipHandler relationshipHandler = null,
            IQuestProvider questProvider = null,
            int averageForDayRef = 0, 
            int averageForWeekRef = 0) 
            : base(
                decayRef, 
                decayCounterRef, 
                doesDecayRef, 
                priorityRef, 
                happinessThresholdRef, 
                valueRef, 
                maxValueRef, 
                new string[0], 
                fulfillingSprite,
                averageForDayRef,
                averageForWeekRef)
        {
            this.RelationshipHandler = relationshipHandler ?? GlobalConstants.GameManager?.RelationshipHandler;
            this.QuestProvider = questProvider ?? GlobalConstants.GameManager?.QuestProvider;
        }

        //Currently, the questing and employment systems are not (fully) in.
        //This will just seek out a random person and ask for a quest stub.
        public override bool FindFulfilmentObject(IEntity actor)
        {
            this.GetBits();
            
            IEnumerable<string> tags = actor.Tags.Where(x => x.IndexOf("sentient", StringComparison.OrdinalIgnoreCase) >= 0);

            List<IEntity> possibleListeners = actor.MyWorld.SearchForEntities(actor, tags).ToList();

            IEntity bestMatch = null;
            int bestRelationship = int.MinValue;
            foreach (IEntity possible in possibleListeners)
            {
                List<IJoyObject> participants = new List<IJoyObject> {actor, possible};

                string[] relationshipTags = new[] {"friendship"};
                IEnumerable<IRelationship> relationships = this.RelationshipHandler?.Get(participants, relationshipTags);

                if (relationships is null)
                {
                    return false;
                }

                foreach (IRelationship relationship in relationships)
                {
                    int thisRelationship = relationship.GetRelationshipValue(actor.Guid, possible.Guid);
                    if (bestRelationship < thisRelationship)
                    {
                        bestRelationship = thisRelationship;
                        bestMatch = possible;
                    }
                }
            }

            if (bestMatch is null)
            {
                this.m_CachedActions["wanderaction"].Execute(
                    new IJoyObject[] {actor},
                    new[] {"wander", "need", "purpose"});
                return false;
            }

            this.m_CachedActions["seekaction"].Execute(
                new IJoyObject[] {actor, bestMatch},
                new[] {"need", "seek", "purpose"},
                new Dictionary<string, object>
                {
                    {"need", "purpose"}
                });
            return true;
        }

        public override bool Interact(IEntity actor, IJoyObject obj)
        {
            if (!(obj is IEntity listener))
            {
                return false;
            }
            
            this.GetBits();
            
            //Asking to do something for your friend increases your relationship
            this.m_CachedActions["fulfillneedaction"].Execute(
                new IJoyObject[] {actor, listener},
                new[] {"need", "friendship", "fulfill"},
                new Dictionary<string, object>
                {
                    {"need", "friendship"}, 
                    {"value" , actor.Statistics[EntityStatistic.PERSONALITY].Value},
                    {"counter", 0},
                    {"doAll", true}
                });

            if (this.RelationshipHandler.IsFamily(actor, listener))
            {
                this.m_CachedActions["fulfillneedaction"].Execute(
                    new IJoyObject[] {actor, listener},
                    new[] {"need", "family", "fulfill"},
                    new Dictionary<string, object>
                    {
                        {"need", "family"}, 
                        {"value" , actor.Statistics[EntityStatistic.PERSONALITY].Value},
                        {"counter", 0},
                        {"doAll", true}
                    });
            }

            if (this.QuestProvider is null == false)
            {
                actor.AddQuest(this.QuestProvider.MakeRandomQuest(
                        actor, 
                        listener, 
                        actor.MyWorld.GetOverworld()));
            }

            this.m_CachedActions["fulfillneedaction"].Execute(
                new IJoyObject[] {actor},
                new[] {"need", "purpose", "fulfill"},
                new Dictionary<string, object>
                {
                    {"need", "purpose"}, 
                    {"value" , listener.Statistics[EntityStatistic.PERSONALITY].Value},
                    {"counter", 0},
                    {"doAll", true}
                });

            return true;
        }

        public override INeed Copy()
        {
            this.GetBits();
            
            return new Purpose(
                this.m_Decay,
                this.m_DecayCounter,
                this.m_DoesDecay,
                this.m_Priority,
                this.m_HappinessThreshold,
                this.m_Value,
                this.m_MaximumValue,
                this.FulfillingSprite,
                this.RelationshipHandler,
                this.QuestProvider,
                this.AverageForDay);
        }

        protected void GetBits()
        {
            if (this.QuestProvider is null)
            {
                this.QuestProvider = GlobalConstants.GameManager?.QuestProvider;
            }

            if (this.RelationshipHandler is null)
            {
                this.RelationshipHandler = GlobalConstants.GameManager?.RelationshipHandler;
            }
        }

        public override INeed Randomise()
        {
            int decay = this.Roller.Roll(DECAY_MIN, DECAY_MAX);
            int decayCounter = this.Roller.Roll(0, DECAY_MAX);
            int priority = this.Roller.Roll(PRIORITY_MIN, PRIORITY_MAX);
            int happinessThreshold = this.Roller.Roll(HAPPINESS_THRESHOLD_MIN, HAPPINESS_THRESHOLD_MAX);
            int value = this.Roller.Roll(0, HAPPINESS_THRESHOLD_MAX);
            int maxValue = this.Roller.Roll(MAX_VALUE_MIN, MAX_VALUE_MAX);

            this.GetBits();
            
            return new Purpose(
                decay,
                decayCounter,
                true,
                priority,
                happinessThreshold,
                value,
                maxValue,
                this.FulfillingSprite,
                this.RelationshipHandler,
                this.QuestProvider);
        }
    }
}