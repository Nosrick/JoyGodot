using System.Collections.Generic;
using JoyLib.Code.Entities.Items;
using JoyLib.Code.Graphics;

namespace JoyLib.Code.Entities.Needs
{
    public class Hunger : AbstractNeed
    {
        public override string Name => "hunger";
        
        protected const int DECAY = 200;
        protected const int PRIORITY = 12;

        protected const int HAPPINESS_THRESHOLD_MIN = 5;
        protected const int HAPPINESS_THRESHOLD_MAX = 24;

        protected const int MAX_VALUE_MIN = HAPPINESS_THRESHOLD_MAX;
        protected const int MAX_VALUE_MAX = MAX_VALUE_MIN * 4;
        
        public Hunger() : 
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

        public Hunger(
            int decayRef, 
            int decayCounterRef, 
            bool doesDecayRef, 
            int priorityRef, 
            int happinessThresholdRef, 
            int valueRef, 
            int maxValueRef, 
            ISpriteState fulfillingSprite,
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
        }

        public override INeed Copy()
        {
            return new Hunger(
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

        public override bool FindFulfilmentObject(IEntity actor)
        {
            IItemInstance[] targets = actor.SearchBackpackForItemType(new string[] { "food" });
            int bestFood = 0;
            IItemInstance chosenFood = null;

            //Look for food in the target list
            foreach(IItemInstance target in targets)
            {
                if(target.ItemType.Value > bestFood)
                {
                    bestFood = target.ItemType.Value;
                    chosenFood = target;
                }
            }

            //If we've found food, eat it
            if(chosenFood != null)
            {
                this.Interact(actor, chosenFood);
                actor.RemoveContents(chosenFood);
                return true;
            }

            //Search the floor
            IEnumerable<IJoyObject> objects = actor.MyWorld.SearchForObjects(actor, new string[] { "food" });
            foreach(IJoyObject obj in objects)
            {
                if(!(obj is IItemInstance item))
                {
                    continue;
                }

                if(item.ItemType.Value > bestFood)
                {
                    bestFood = item.ItemType.Value;
                    chosenFood = item;
                }
            }

            if(chosenFood != null)
            {
                if(chosenFood.WorldPosition.Equals(actor.WorldPosition))
                {
                    this.Interact(actor, chosenFood);
                    actor.MyWorld.RemoveObject(chosenFood.WorldPosition, chosenFood);
                    return true;
                }
                else
                {
                    this.m_CachedActions["seekaction"].Execute(
                        new IJoyObject[]{ actor, chosenFood },
                        new string[] { "seek", "need", "hunger" },
                        new Dictionary<string, object>
                        {
                            {"need", "hunger"}
                        });
                    return true;
                }
            }

            this.m_CachedActions["wanderaction"].Execute(
                new IJoyObject[]{ actor },
                new[] { "wander", "need", "hunger" });

            return false;
        }

        public override bool Interact(IEntity actor, IJoyObject obj)
        {
            if (!(obj is ItemInstance item))
            {
                return false;
            }

            actor.AddContents(item);
            actor.MyWorld.RemoveObject(item.WorldPosition, item);
            item.Interact(actor);

            return true;
        }

        public override INeed Randomise()
        {
            return new Hunger(
                DECAY, 
                DECAY, 
                true, 
                PRIORITY, this.Roller.Roll(HAPPINESS_THRESHOLD_MIN, HAPPINESS_THRESHOLD_MAX), 
                HAPPINESS_THRESHOLD_MAX, this.Roller.Roll(MAX_VALUE_MIN, MAX_VALUE_MAX),
                this.FulfillingSprite);
        }
    }
}
