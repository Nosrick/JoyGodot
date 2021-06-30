using System;
using System.Collections.Generic;
using System.Linq;

using Godot.Collections;
using JoyGodot.Assets.Scripts.Entities.Relationships;
using JoyGodot.Assets.Scripts.Entities.Sexuality.Processors;
using JoyGodot.Assets.Scripts.Helpers;
using Array = Godot.Collections.Array;

namespace JoyGodot.Assets.Scripts.Entities.Sexuality
{
    [Serializable]
    public class BaseSexuality : ISexuality
    {
        protected HashSet<string> m_Tags;


        public string Name { get; protected set; }


        public bool DecaysNeed { get; protected set; }


        public int MatingThreshold { get; set; }


        protected ISexualityPreferenceProcessor Processor { get; set; }

        public IEnumerable<string> Tags
        {
            get => this.m_Tags;
            protected set => this.m_Tags = new HashSet<string>(value);
        }

        public BaseSexuality()
        {
            this.Name = "DEFAULT";
            this.DecaysNeed = false;
            this.MatingThreshold = 0;
            this.Processor = new AsexualProcessor();
            this.m_Tags = new HashSet<string>
            {
                "sexual"
            };
        }

        public BaseSexuality(
            string name,
            bool decaysNeed,
            int matingThreshold,
            ISexualityPreferenceProcessor processor,
            IEnumerable<string> tags = null)
        {
            this.Name = name;
            this.DecaysNeed = decaysNeed;
            this.MatingThreshold = matingThreshold;
            this.Processor = processor;
            this.Tags = tags ?? new HashSet<string>();

            this.m_Tags.Add("sexual");
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
            if (!this.HasTag(tag))
            {
                return false;
            }

            this.m_Tags.Remove(tag);
            return true;
        }

        public bool WillMateWith(IEntity me, IEntity them, IEnumerable<IRelationship> relationships)
        {
            return this.Processor.WillMateWith(me, them, relationships);
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
                {"MatingThreshold", this.MatingThreshold},
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
            this.MatingThreshold = valueExtractor.GetValueFromDictionary<int>(data, "MatingThreshold");
            string processorName = valueExtractor.GetValueFromDictionary<string>(data, "Processor");
            this.Processor = processorName.IsNullOrEmpty() == false 
                ? GlobalConstants.GameManager.SexualityHandler.GetProcessor(processorName) 
                : new AsexualProcessor();
        }
    }
}