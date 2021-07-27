using System;
using System.Collections.Generic;
using System.Linq;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Needs;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Managed_Assets;

namespace JoyGodot.Assets.Data.Scripts.Needs
{
    public class Health : AbstractNeed
    {
        protected bool CleanBonusApplied
        {
            get;
            set;
        }
        
        protected const int DECAY_MIN = 64;
        protected const int DECAY_MAX = 128;

        protected const int PRIORITY_MIN = 4;
        protected const int PRIORITY_MAX = 12;

        protected const int HAPPINESS_THRESHOLD_MIN = 0;
        protected const int HAPPINESS_THRESHOLD_MAX = 24;

        protected const int MAX_VALUE_MIN = HAPPINESS_THRESHOLD_MAX;
        protected const int MAX_VALUE_MAX = MAX_VALUE_MIN * 4;

        //Bonus to health for having no diseases
        protected const int CLEAN_BONUS = 50;

        public override string Name => "health";

        public override string DisplayName => "sick";

        protected INeed ActingNeed
        {
            get;
            set;
        }

        public Health()
            : base(
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

        public Health(
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
                new string[0],
                fulfillingSprite,
                averageForDayRef, 
                averageForWeekRef)
        {
        }

        public override bool FindFulfilmentObject(IEntity actor)
        {
            this.ActingNeed = null;
            
            List<INeed> needs = new List<INeed>();
            needs.Add(actor.Needs["hunger"]);
            needs.Add(actor.Needs["thirst"]);
            needs.Add(actor.Needs["sleep"]);

            int max = int.MinValue;
            INeed chosen = null;
            foreach (INeed need in needs)
            {
                if (need.Value > max)
                {
                    max = need.Value;
                    chosen = need;
                }
            }

            if (chosen is null)
            {
                return false;
            }

            this.ActingNeed = chosen;

            return chosen.FindFulfilmentObject(actor);
        }

        public override bool Interact(IEntity actor, IJoyObject obj)
        {
            if (this.ActingNeed is null)
            {
                return false;
            }

            return this.ActingNeed.Interact(actor, obj);
        }

        public override INeed Copy()
        {
            return new Health(
                this.m_Decay,
                this.m_DecayCounter,
                this.m_DoesDecay,
                this.m_Priority,
                this.m_HappinessThreshold,
                this.m_Value,
                this.m_MaximumValue,
                this.FulfillingSprite,
                this.AverageForDay,
                this.AverageForWeek);
        }

        public override INeed Randomise()
        {
            int decay = this.Roller.Roll(DECAY_MIN, DECAY_MAX);
            int decayCounter = this.Roller.Roll(0, DECAY_MAX);
            int priority = this.Roller.Roll(PRIORITY_MIN, PRIORITY_MAX);
            int happinessThreshold = this.Roller.Roll(HAPPINESS_THRESHOLD_MIN, HAPPINESS_THRESHOLD_MAX);
            int value = this.Roller.Roll(0, HAPPINESS_THRESHOLD_MAX);
            int maxValue = this.Roller.Roll(MAX_VALUE_MIN, MAX_VALUE_MAX);
            
            return new Health(
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

            bool diseaseFree = actor.Abilities.Any(
                ability => ability.Tags.Any(
                    tag => tag.Equals("disease", StringComparison.OrdinalIgnoreCase)));
            
            if (diseaseFree == true && this.CleanBonusApplied == false)
            {
                this.CleanBonusApplied = true;
                this.Value += CLEAN_BONUS;
                result = true;
            }
            else if (diseaseFree == false && this.CleanBonusApplied)
            {
                this.CleanBonusApplied = false;
                this.Value -= CLEAN_BONUS;
                result = true;
            }

            return result;
        }
    }
}