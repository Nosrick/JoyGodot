using System.Collections.Generic;
using System.Linq;

using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Needs;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.Items;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Managed_Assets;

namespace JoyGodot.Assets.Data.Scripts.Needs
{
    public class Thirst : AbstractNeed
    {
        public override string Name => "thirst";

        public override string DisplayName => "thirsty";

        protected const int DECAY = 200;
        protected const int PRIORITY = 12;

        protected const int HAPPINESS_THRESHOLD_MIN = 5;
        protected const int HAPPINESS_THRESHOLD_MAX = 24;

        protected const int MAX_VALUE_MIN = HAPPINESS_THRESHOLD_MAX;
        protected const int MAX_VALUE_MAX = MAX_VALUE_MIN * 4;
        
        public Thirst() :
            base(
                    0,
                    1,
                    true,
                    1,
                    1,
                    1,
                    1, 
                    0,
                    new string[0])
        {
        }

        public Thirst(
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
                0,
                new string[0],
                fulfillingSprite,
                averageForDayRef,
                averageForWeekRef)
        {

        }

        public override INeed Copy()
        {
            return new Thirst(
                this.m_Decay,
                this.m_DecayCounter,
                this.m_DoesDecay,
                this.m_Priority,
                this.m_HappinessThreshold,
                this.m_Value,
                this.m_MaximumValue,
                this.FulfillingSprite,
                this.AverageForDay,
                this.m_AverageForWeek);
        }

        public override bool FindFulfilmentObject(IEntity actor)
        {
            string type = "drink";
            IItemInstance[] targets = actor.SearchBackpackForItemType(new string[] { type });
            int bestDrink = 0;
            IItemInstance chosenDrink = null;

            //Look for food in the target list
            foreach (IItemInstance target in targets)
            {
                if (target.ItemType.Value > bestDrink)
                {
                    bestDrink = target.ItemType.Value;
                    chosenDrink = target;
                }
            }

            //If we've found food, eat it
            if (chosenDrink is null == false)
            {
                this.Interact(actor, chosenDrink);
                actor.RemoveContents(chosenDrink);
                return true;
            }

            //Search the floor
            IEnumerable<IJoyObject> objects = actor.MyWorld.SearchForObjects(actor, new string[] { type });
            foreach (IJoyObject obj in objects)
            {
                if (!(obj is IItemInstance item))
                {
                    continue;
                }

                if (item.ItemType.Value > bestDrink)
                {
                    bestDrink = item.ItemType.Value;
                    chosenDrink = item;
                }
            }

            if (chosenDrink is null == false)
            {
                if (chosenDrink.WorldPosition.Equals(actor.WorldPosition))
                {
                    this.Interact(actor, chosenDrink);
                    actor.MyWorld.RemoveObject(chosenDrink.WorldPosition, chosenDrink);
                    return true;
                }
                else
                {
                    this.m_CachedActions["seekaction"].Execute(
                        new IJoyObject[] { actor, chosenDrink },
                        new string[] { "need", "thirst", "seek" },
                        new Dictionary<string, object>
                        {
                            {"need", "thirst"}
                        });
                    return true;
                }
            }

            this.m_CachedActions["wanderaction"].Execute(
                new IJoyObject[] { actor },
                new string[] { "need", "thirst", "wander" });

            return false;
        }

        public override bool Interact(IEntity actor, IJoyObject obj)
        {
            if (!(obj is ItemInstance item))
            {
                return false;
            }
            
            var name = item.AllAbilities.FirstOrDefault(a => 
                a.HasTag("ingestion")
                && a.HasTag("drink")
                && a.HasTag("active")
            )?.Name;
            
            if (name.IsNullOrEmpty())
            {
                return false;
            }

            item.Interact(actor, name);

            return true;
        }

        public override INeed Randomise()
        {
            return new Thirst(
                DECAY,
                DECAY, 
                true, 
                PRIORITY, this.Roller.Roll(HAPPINESS_THRESHOLD_MIN, HAPPINESS_THRESHOLD_MAX), 
                HAPPINESS_THRESHOLD_MAX, this.Roller.Roll(MAX_VALUE_MIN, MAX_VALUE_MAX),
                this.FulfillingSprite);
        }
    }
}
