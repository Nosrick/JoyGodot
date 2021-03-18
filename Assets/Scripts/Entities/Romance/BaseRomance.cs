using System;
using System.Collections.Generic;
using System.Linq;
using JoyLib.Code.Entities.Relationships;
using JoyLib.Code.Entities.Romance.Processors;

namespace JoyLib.Code.Entities.Romance
{
    [Serializable]
    public class BaseRomance : IRomance
    {
         
        public string Name { get; protected set; }

         
        public bool DecaysNeed { get; protected set; }

         
        public int RomanceThreshold { get; set; }

         
        public int BondingThreshold { get; set; }
        
         
        protected IRomanceProcessor Processor { get; set; }

        public IEnumerable<string> Tags
        {
            get => this.m_Tags;
            protected set => this.m_Tags = new HashSet<string>(value);
        }

         
        protected HashSet<string> m_Tags;

        public BaseRomance()
        {
            this.Name = "DEFAULT";
            this.DecaysNeed = false;
            this.RomanceThreshold = 0;
            this.BondingThreshold = 0;
            this.Processor = new AromanticProcessor();
            this.Tags = new List<string>();
        }

        public BaseRomance(
            string name,
            bool decaysNeed,
            int romanceThreshold,
            int bondingThreshold,
            IRomanceProcessor processor,
            IEnumerable<string> tags)
        {
            this.Name = name;
            this.DecaysNeed = decaysNeed;
            this.RomanceThreshold = romanceThreshold;
            this.BondingThreshold = bondingThreshold;
            this.Processor = processor;
            this.Tags = tags;
            this.m_Tags.Add("romantic");
        }
        
        public bool HasTag(string tag)
        {
            return this.m_Tags.Any(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase));
        }

        public bool AddTag(string tag)
        {
            if (this.HasTag(tag))
            {
                return false;
            }
            this.m_Tags.Add(tag);
            return true;
        }

        public bool RemoveTag(string tag)
        {
            if (this.HasTag(tag))
            {
                this.m_Tags.Remove(tag);
                return true;
            }

            return false;
        }

        public bool WillRomance(IEntity me, IEntity them, IEnumerable<IRelationship> relationships)
        {
            return this.Processor.WillRomance(me, them, relationships);
        }

        public bool Compatible(IEntity me, IEntity them)
        {
            return this.Processor.Compatible(me, them);
        }
    }
}