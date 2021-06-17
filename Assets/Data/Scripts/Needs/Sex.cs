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

namespace JoyGodot.Assets.Data.Scripts.Needs
{
    public class Sex : AbstractNeed
    {
        public override string Name => "sex";
        
        protected IEntityRelationshipHandler RelationshipHandler { get; set; }

        protected const int DECAY_MIN = 200;
        protected const int DECAY_MAX = 600;

        protected const int PRIORITY = 12;

        protected const int HAPPINESS_THRESHOLD_MIN = 5;
        protected const int HAPPINESS_THRESHOLD_MAX = 24;
        
        protected const int MAX_VALUE_MIN = HAPPINESS_THRESHOLD_MAX;
        protected const int MAX_VALUE_MAX = MAX_VALUE_MIN * 4;
        

        public Sex() : 
            base(
                0, 
                1, 
                true, 
                1, 
                1, 
                1, 
                1, 
                new string[0])
        {
        }

        public Sex(
            int decayRef,
            int decayCounterRef,
            bool doesDecayRef,
            int priorityRef,
            int happinessThresholdRef,
            int valueRef,
            int maxValueRef,
            ISpriteState fulfillingSprite,
            IEntityRelationshipHandler relationshipHandler = null,
            int averageForDayRef = 0,
            int averageForWeekRef = 0) :

            base(
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
        }

        public override INeed Copy()
        {
            return new Sex(
                this.m_Decay,
                this.m_DecayCounter,
                this.m_DoesDecay,
                this.m_Priority,
                this.m_HappinessThreshold,
                this.m_Value,
                this.m_MaximumValue,
                this.FulfillingSprite,
                this.RelationshipHandler,
                this.m_AverageForDay,
                this.m_AverageForWeek);
        }

        public override bool FindFulfilmentObject(IEntity actor)
        {
            this.GetBits();
            
            IEnumerable<string> tags = actor.Tags.Where(x => x.Contains("sentient"));

            List<IEntity> possibleMates = actor.MyWorld.SearchForEntities(actor, tags).ToList();

            IEntity bestMate = null;
            int bestRelationship = actor.Sexuality.MatingThreshold;
            foreach (IEntity mate in possibleMates)
            {
                List<IJoyObject> participants = new List<IJoyObject>();
                participants.Add(actor);
                participants.Add(mate);
                string[] relationshipTags = new string[] { "sexual" };
                IEnumerable<IRelationship> relationships = this.RelationshipHandler.Get(participants, relationshipTags);

                foreach (IRelationship relationship in relationships)
                {
                    int thisRelationship = relationship.GetRelationshipValue(actor.Guid, mate.Guid);
                    if (thisRelationship >= bestRelationship)
                    {
                        bestRelationship = thisRelationship;
                        bestMate = mate;
                    }
                }
            }

            if (bestMate is null)
            {
                this.m_CachedActions["wanderaction"].Execute(
                    new IJoyObject[] { actor },
                    new [] { "need", "wander", "sex" });
                return false;
            }
            else
            {
                this.m_CachedActions["seekaction"].Execute(
                    new IJoyObject[] { actor, bestMate },
                    new [] { "need", "seek", "sex" },
                    new Dictionary<string, object>
                    {
                        {"need", "sex"}
                    });
                return true;
            }
        }

        public override bool Interact(IEntity actor, IJoyObject obj)
        {
            if (!(obj is IEntity partner))
            {
                return false;
            }
            
            this.GetBits();

            if (actor.Sexuality.WillMateWith(actor, partner, this.RelationshipHandler.Get(
                    new IJoyObject[] { actor, partner },
                    new string[] { "sexual" })))
            {
                int satisfaction = this.CalculateSatisfaction(
                    new IEntity[] { actor, partner },
                    new string[] {
                        EntityStatistic.ENDURANCE,
                        EntityStatistic.CUNNING,
                        EntityStatistic.PERSONALITY });

                int time = this.Roller.Roll(5, 30);

                if (actor.FulfillmentData is null
                    || partner.FulfillmentData is null)
                {
                    return false;
                }
                
                if (actor.FulfillmentData.Name.Equals(this.Name) && 
                    partner.FulfillmentData.Name.Equals(this.Name))
                {
                    HashSet<IJoyObject> userParticipants =
                        new HashSet<IJoyObject>(actor.FulfillmentData.Targets) {actor, partner};
                    this.m_CachedActions["fulfillneedaction"].Execute(
                        userParticipants.ToArray(),
                        new[] { "sex", "need", "fulfill" },
                        new Dictionary<string, object>
                        {
                            {"need", this.Name},
                            {"value", satisfaction},
                            {"counter", time }
                        });

                    HashSet<IJoyObject> partnerParticipants =
                        new HashSet<IJoyObject>(partner.FulfillmentData.Targets) {partner, actor};
                    this.m_CachedActions["fulfillneedaction"].Execute(
                        partnerParticipants.ToArray(),
                        new string[] { "sex", "need", "fulfill" },
                        new Dictionary<string, object>
                        {
                            {"need", this.Name},
                            {"value", satisfaction},
                            {"counter", time }
                        });
                }
            }

            return true;
        }

        protected int CalculateSatisfaction(IEnumerable<IEntity> participants, IEnumerable<string> tags)
        {
            int satisfaction = 0;
            int total = 0;
            foreach (IEntity participant in participants)
            {
                IEnumerable<Tuple<string, object>> data = participant.GetData(tags.ToArray());
                int subTotal = 0;
                foreach (Tuple<string, object> tuple in data)
                {
                    if (tuple.Item2 is int value)
                    {
                        subTotal += value;
                    }
                }
                subTotal /= data.Count();
                total += subTotal;
            }

            satisfaction = total / participants.Count();

            return satisfaction;
        }

        public override INeed Randomise()
        {
            int decay = this.Roller.Roll(DECAY_MIN, DECAY_MAX);
            int decayCounter = this.Roller.Roll(0, DECAY_MAX);
            int maxValue = this.Roller.Roll(MAX_VALUE_MIN, MAX_VALUE_MAX);

            this.GetBits();
            
            return new Sex(
                decay, 
                decayCounter, 
                true, 
                PRIORITY, this.Roller.Roll(HAPPINESS_THRESHOLD_MIN, HAPPINESS_THRESHOLD_MAX), 
                HAPPINESS_THRESHOLD_MAX, 
                maxValue,
                this.FulfillingSprite,
                this.RelationshipHandler);
        }

        protected void GetBits()
        {
            if (this.RelationshipHandler is null)
            {
                this.RelationshipHandler = GlobalConstants.GameManager?.RelationshipHandler;
            }
        }
    }
}
