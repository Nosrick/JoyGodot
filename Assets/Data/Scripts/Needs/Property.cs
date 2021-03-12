using System;
using System.Collections.Generic;
using System.Linq;
using JoyLib.Code.Entities.AI;
using JoyLib.Code.Entities.Items;
using JoyLib.Code.Graphics;

namespace JoyLib.Code.Entities.Needs
{
    public class Property : AbstractNeed
    {
        protected const int DECAY_MIN = 4;
        protected const int DECAY_MAX = 128;

        protected const int PRIORITY_MIN = 0;
        protected const int PRIORITY_MAX = 12;

        protected const int HAPPINESS_THRESHOLD_MIN = 0;
        protected const int HAPPINESS_THRESHOLD_MAX = 24;

        protected const int MAX_VALUE_MIN = HAPPINESS_THRESHOLD_MAX;
        protected const int MAX_VALUE_MAX = MAX_VALUE_MIN * 4;
        
        public override string Name => "property";

        public Property()
        : base(
            1,
            1,
            true,
            1,
            1,
            1,
            1,
            new string[] { "additemaction" })
        {
        }

        public Property(
            int decayRef, 
            int decayCounterRef, 
            bool doesDecayRef, 
            int priorityRef, 
            int happinessThresholdRef, 
            int valueRef, 
            int maxValueRef,
            ISpriteState fulfillingSprite,
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
                new string[] { "additemaction" },
                fulfillingSprite,
                averageForDayRef, 
                averageForWeekRef)
        {
        }

        public override bool FindFulfilmentObject(IEntity actor)
        {
            IEnumerable<IJoyObject> objects = actor.MyWorld.SearchForObjects(actor, new string[0]);

            if (objects.Any() == false)
            {
                return false;
            }

            ItemInstance chosenItem = null;
            int highestValue = int.MinValue;
            foreach (IJoyObject obj in objects)
            {
                if (!(obj is ItemInstance item))
                {
                    continue;
                }

                if (item.Value > highestValue)
                {
                    highestValue = item.Value;
                    chosenItem = item;
                }
            }

            if (chosenItem is null)
            {
                this.m_CachedActions["wanderaction"].Execute(
                    new IJoyObject[] { actor },
                    new[] { "need", "property", "wander" });
                return false;
            }

            this.m_CachedActions["seekaction"].Execute(
                new IJoyObject[] {actor, chosenItem},
                new[] {"need", "property", "seek"},
                new Dictionary<string, object>
                {
                    {"need", "property"}
                });
            return true;
        }

        public override bool Interact(IEntity actor, IJoyObject obj)
        {
            if (!(obj is ItemInstance item))
            {
                return false;
            }

            List<string> tags = new List<string> {"pickup", "property"};
            if (item.OwnerGUID != actor.Guid && item.OwnerGUID != Guid.Empty)
            {
                tags.Add("theft");
            }

            if (this.m_CachedActions["additemaction"].Execute(
                new[] {actor, obj},
                tags.ToArray(),
                new Dictionary<string, object>
                {
                    {"newOwner", true}
                }))
            {
                if (actor.MyWorld.RemoveObject(actor.WorldPosition, item))
                {
                    actor.CurrentTarget = new NeedAIData
                    {
                        idle = true,
                        intent = Intent.Interact,
                        need = null,
                        searching = false,
                        target = null,
                        targetPoint = GlobalConstants.NO_TARGET
                    };
                    return true;
                }
                this.FindFulfilmentObject(actor);
            }

            return false;
        }

        public override INeed Copy()
        {
            return new Property(
                this.m_Decay,
                this.m_DecayCounter,
                this.m_DoesDecay,
                this.m_Priority,
                this.m_HappinessThreshold,
                this.m_Value,
                this.m_MaximumValue,
                this.FulfillingSprite,
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
            
            return new Property(
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

            if (actor.Contents.Any() == false)
            {
                return result;
            }

            this.Fulfill(actor.Contents.Count());
            return true;
        }
    }
}