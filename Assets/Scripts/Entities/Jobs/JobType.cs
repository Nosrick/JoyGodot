using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using JoyGodot.Assets.Scripts.Entities.Abilities;
using JoyGodot.Assets.Scripts.Helpers;

namespace JoyGodot.Assets.Scripts.Entities.Jobs
{
    [Serializable]
    public class JobType : IJob
    {
         
        protected IDictionary<IAbility, int> m_Abilities;
         
        protected IDictionary<string, int> m_StatisticDiscounts;
         
        protected IDictionary<string, int> m_SkillDiscounts;

        public JobType()
        {
        }

        public JobType(
            string name, 
            string description, 
            IDictionary<string, int> statDiscounts, 
            IDictionary<string, int> skillDiscounts,
            IDictionary<IAbility, int> abilities,
            Color abilityIconColour,
            Color abilityBackgroundColour)
        {
            this.Name = name;
            this.Description = description;
            this.Abilities = abilities;
            this.m_StatisticDiscounts = statDiscounts;
            this.m_SkillDiscounts = skillDiscounts;
            this.AbilityIconColour = abilityIconColour;
            this.AbilityBackgroundColour = abilityBackgroundColour;
        }

        public int GetSkillDiscount(string skillName)
        {
            if (this.m_SkillDiscounts.Any(p =>
                p.Key.Equals(skillName, StringComparison.OrdinalIgnoreCase)))
            {
                return this.m_SkillDiscounts.First(
                        pair => pair.Key.Equals(skillName, StringComparison.OrdinalIgnoreCase))
                    .Value;
            }

            return 0;
        }

        public int GetStatisticDiscount(string statisticName)
        {
            if (this.m_StatisticDiscounts.Any(p =>
                p.Key.Equals(statisticName, StringComparison.OrdinalIgnoreCase)))
            {
                return this.m_StatisticDiscounts.First(
                        pair => pair.Key.Equals(statisticName, StringComparison.OrdinalIgnoreCase))
                    .Value;
            }

            return 0;
        }

        public int AddExperience(int value)
        {
            this.Experience += value;
            return this.Experience;
        }

        public bool SpendExperience(int value)
        {
            if (this.Experience < value)
            {
                return false;
            }

            this.Experience -= value;
            return true;
        }

        public IJob Copy(IJob original)
        {
            IJob job = new JobType(
                original.Name,
                original.Description,
                original.StatisticDiscounts,
                original.SkillDiscounts,
                original.Abilities, 
                this.AbilityIconColour, 
                this.AbilityBackgroundColour);

            return job;
        }

         
        public string Name
        {
            get;
            protected set;
        }

         
        public string Description
        {
            get;
            protected set;
        }

         
        public int Experience { get; protected set; }

        public IDictionary<string, int> StatisticDiscounts
        {
            get
            {
                return this.m_StatisticDiscounts;
            }
        }

        public IDictionary<string, int> SkillDiscounts
        {
            get
            {
                return this.m_SkillDiscounts.Copy();
            }
        }

        public Color AbilityIconColour { get; protected set; }
        public Color AbilityBackgroundColour { get; protected set; }

        public IDictionary<IAbility, int> Abilities
        {
            get => this.m_Abilities;
            protected set => this.m_Abilities = value;
        }
    }
}
