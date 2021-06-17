using System;
using System.Collections.Generic;
using System.Linq;
using JoyGodot.Assets.Scripts;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Needs;
using JoyGodot.Assets.Scripts.Entities.Relationships;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Managed_Assets;

namespace JoyGodot.Assets.Data.Scripts.Needs
{
    public class Respect : AbstractNeed
    {
        public override string Name => "respect";

        protected const int DECAY_MIN = 4;
        protected const int DECAY_MAX = 128;

        protected const int PRIORITY_MIN = 0;
        protected const int PRIORITY_MAX = 12;

        protected const int HAPPINESS_THRESHOLD_MIN = 0;
        protected const int HAPPINESS_THRESHOLD_MAX = 24;

        protected const int MAX_VALUE_MIN = HAPPINESS_THRESHOLD_MAX;
        protected const int MAX_VALUE_MAX = MAX_VALUE_MIN * 4;
        
        protected IEntityRelationshipHandler RelationshipHandler { get; set; }
        
        public Respect()
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
        
        public Respect(
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
        }

        //This is to do with others, so look for something to do
        public override bool FindFulfilmentObject(IEntity actor)
        {
            INeed[] needs = actor.Needs.Where(need => 
                need.Key.Equals("family", StringComparison.OrdinalIgnoreCase)
                || need.Key.Equals("friendship", StringComparison.OrdinalIgnoreCase)
                || need.Key.Equals("purpose", StringComparison.OrdinalIgnoreCase))
                .Select(n => n.Value)
                .ToArray();

            INeed chosenNeed = null;
            int bestMatch = Int32.MaxValue;
            foreach (INeed need in needs)
            {
                if (need.ContributingHappiness == false && bestMatch > need.Value)
                {
                    chosenNeed = need;
                    bestMatch = need.Value;
                }
            }

            //If this is true, then there are no needs that are not contributing happiness
            if (chosenNeed == null)
            {
                return true;
            }

            return chosenNeed.FindFulfilmentObject(actor);
        }

        public override bool Interact(IEntity actor, IJoyObject obj)
        {
            return false;
        }

        public override INeed Copy()
        {
            return new Respect(
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

        public override INeed Randomise()
        {
            int decay = this.Roller.Roll(DECAY_MIN, DECAY_MAX);
            int decayCounter = this.Roller.Roll(0, DECAY_MAX);
            int priority = this.Roller.Roll(PRIORITY_MIN, PRIORITY_MAX);
            int happinessThreshold = this.Roller.Roll(HAPPINESS_THRESHOLD_MIN, HAPPINESS_THRESHOLD_MAX);
            int value = this.Roller.Roll(0, HAPPINESS_THRESHOLD_MAX);
            int maxValue = this.Roller.Roll(MAX_VALUE_MIN, MAX_VALUE_MAX);
            
            return new Respect(
                decay,
                decayCounter,
                true,
                priority,
                happinessThreshold,
                value,
                maxValue,
                this.FulfillingSprite);
        }

        public override bool Tick(Entity actor)
        {
            bool result = base.Tick(actor);
            if (this.m_DecayCounter == 0 && this.m_DoesDecay)
            {
                IEnumerable<IRelationship> relationships = this.RelationshipHandler.GetAllForObject(actor);

                if (relationships.Any() == false)
                {
                    return result;
                }
                
                int average = (int)Math.Ceiling(
                    relationships.Average(relationship => 
                        relationship.GetHighestRelationshipValue(actor.Guid)));

                this.Fulfill(average);
                return true;
            }
            return result;
        }
    }
}