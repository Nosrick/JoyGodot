using System;
using System.Collections.Generic;
using System.Linq;

using Godot.Collections;
using JoyGodot.Assets.Scripts.Entities.Relationships;
using JoyGodot.Assets.Scripts.Entities.Romance.Processors;
using JoyGodot.Assets.Scripts.Helpers;
using Array = Godot.Collections.Array;

namespace JoyGodot.Assets.Scripts.Entities.Romance
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

        public bool HasTags(IEnumerable<string> tags)
        {
            return tags.All(this.HasTag);
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

        public Dictionary Save()
        {
            Dictionary saveDict = new Dictionary
            {
                {"Name", this.Name},
                {"Tags", new Array(this.Tags)},
                {"DecaysNeed", this.DecaysNeed},
                {"BondingThreshold", this.BondingThreshold},
                {"RomanceThreshold", this.RomanceThreshold},
                {"Processor", this.Processor?.Name}
            };

            return saveDict;
        }

        public void Load(Dictionary data)
        {
            var valueExtractor = GlobalConstants.GameManager.ItemHandler.ValueExtractor;

            this.Name = valueExtractor.GetValueFromDictionary<string>(data, "Name");
            this.Tags = valueExtractor.GetArrayValuesCollectionFromDictionary<string>(data, "Tags");
            this.DecaysNeed = valueExtractor.GetValueFromDictionary<bool>(data, "DecaysNeed");
            this.BondingThreshold = valueExtractor.GetValueFromDictionary<int>(data, "BondingThreshold");
            this.RomanceThreshold = valueExtractor.GetValueFromDictionary<int>(data, "RomanceThreshold");
            string processorName = valueExtractor.GetValueFromDictionary<string>(data, "Processor");
            this.Processor = processorName.IsNullOrEmpty() == false 
                ? GlobalConstants.GameManager.RomanceHandler.GetProcessor(processorName) 
                : new AromanticProcessor();
        }
    }
}