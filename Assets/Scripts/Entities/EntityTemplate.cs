using System;
using System.Collections.Generic;
using System.Linq;
using JoyGodot.Assets.Scripts.Entities.Abilities;
using JoyGodot.Assets.Scripts.Entities.AI.LOS.Providers;
using JoyGodot.Assets.Scripts.Entities.Statistics;
using JoyGodot.Assets.Scripts.Helpers;

namespace JoyGodot.Assets.Scripts.Entities
{
    [Serializable]
    public class EntityTemplate : IEntityTemplate
    {
        protected string m_CreatureType;
        protected string m_Type;

        protected IDictionary<string, IEntityStatistic> m_Statistics;
        protected IDictionary<string, IEntitySkill> m_Skills;
        protected string[] m_Needs;
        protected IAbility[] m_Abilities;
        protected string[] m_Slots;
        protected HashSet<string> m_Tags;
        
        protected int m_Size;

        protected bool m_Sentient;

        public IEnumerable<string> Slots => this.m_Slots;

        public IDictionary<string, IEntityStatistic> Statistics
        {
            get
            {
                IDictionary<string, IEntityStatistic> stats = new Dictionary<string, IEntityStatistic>();
                foreach (KeyValuePair<string, IEntityStatistic> stat in this.m_Statistics)
                {
                    stats.Add(new KeyValuePair<string, IEntityStatistic>(
                        ObjectExtensions.Copy(stat.Key), 
                        ObjectExtensions.Copy(stat.Value)));
                }

                return stats;
            }
        }

        public IDictionary<string, IEntitySkill> Skills
        {
            get
            {
                IDictionary<string, IEntitySkill> skills = new Dictionary<string, IEntitySkill>();
                foreach (KeyValuePair<string, IEntitySkill> skill in this.m_Skills)
                {
                    skills.Add(new KeyValuePair<string, IEntitySkill>(
                        ObjectExtensions.Copy(skill.Key),
                        ObjectExtensions.Copy(skill.Value)));
                }
                return skills;
            }
        }

        public IEnumerable<string> Needs => this.m_Needs;

        public IEnumerable<IAbility> Abilities => this.m_Abilities;

        public int Size => this.m_Size;

        public bool Sentient => this.m_Sentient;

        public IVision VisionType
        {
            get;
            protected set;
        }

        public string CreatureType => this.m_CreatureType;

        public string JoyType => this.m_Type;
        
        public string Description { get; protected set; }

        public EntityTemplate(
            IDictionary<string, IEntityStatistic> statistics, 
            IDictionary<string, IEntitySkill> skills, 
            string[] needs,
            IAbility[] abilities,
            string[] slots, 
            int size, 
            IVision visionType, 
            string creatureType, 
            string type,
            string description,
            string[] tags)
        {
            this.m_Statistics = statistics;
            this.m_Skills = skills;
            this.m_Abilities = abilities;
            this.m_Slots = slots;
            this.m_Needs = needs;
            this.Description = description;

            this.m_Size = size;

            this.m_Sentient = tags.Any(tag => tag.Equals("sentient", StringComparison.OrdinalIgnoreCase));

            this.VisionType = visionType;

            this.m_CreatureType = creatureType;
            this.m_Type = type;

            this.m_Tags = new HashSet<string>();
            for(int i = 0; i < tags.Length; i++)
            {
                this.m_Tags.Add(tags[i]);
            }
        }

        public IEnumerable<string> Tags
        {
            get
            {
                return new List<string>(this.m_Tags);
            }
        }

        public bool HasTag(string tag)
        {
            return this.m_Tags.Any(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase));
        }

        public bool HasTags(IEnumerable<string> tags)
        {
            return tags.All(this.HasTag);
        }

        public bool AddTag(string tag)
        {
            return this.m_Tags.Add(tag);
        }

        public bool RemoveTag(string tag)
        {
            return this.m_Tags.Remove(tag);
        }
    }
}
