using System;
using System.Collections.Generic;
using Castle.Core.Internal;
using Godot.Collections;
using JoyGodot.Assets.Scripts.Entities.Sexes.Processors;
using Array = Godot.Collections.Array;

namespace JoyGodot.Assets.Scripts.Entities.Sexes
{
    [Serializable]
    public class BaseBioSex : IBioSex
    {
        public bool CanBirth { get; protected set; }

        public bool CanFertilise { get; protected set; }

        public string Name { get; protected set; }


        protected IBioSexProcessor Processor { get; set; }

        public IEnumerable<string> Tags
        {
            get => this.m_Tags;
            protected set => this.m_Tags = new HashSet<string>(value);
        }


        protected HashSet<string> m_Tags;

        public BaseBioSex()
        {
            this.Name = "DEFAULT";
            this.CanBirth = false;
            this.CanFertilise = false;
            this.Processor = new NeutralProcessor();
            this.m_Tags = new HashSet<string>();
        }

        public BaseBioSex(
            string name,
            bool canBirth,
            bool canFertilise,
            IBioSexProcessor processor,
            IEnumerable<string> tags = null)
        {
            this.Name = name;
            this.CanBirth = canBirth;
            this.CanFertilise = canFertilise;
            this.Processor = processor;
            this.Tags = tags ?? new HashSet<string>
            {
                "neutral"
            };
        }

        public IEntity CreateChild(IEnumerable<IEntity> parents)
        {
            throw new System.NotImplementedException();
        }

        public Dictionary Save()
        {
            Dictionary saveDict = new Dictionary
            {
                {"Name", this.Name}, 
                {"CanBirth", this.CanBirth}, 
                {"CanFertilise", this.CanFertilise},
                {"Tags", new Array(this.Tags)},
                {"Processor", this.Processor?.Name}
            };
            
            return saveDict;
        }

        public void Load(Dictionary data)
        {
            var valueExtractor = GlobalConstants.GameManager.ItemHandler.ValueExtractor;

            this.Name = valueExtractor.GetValueFromDictionary<string>(data, "Name");
            this.Tags = valueExtractor.GetArrayValuesCollectionFromDictionary<string>(data, "Tags");
            this.CanBirth = valueExtractor.GetValueFromDictionary<bool>(data, "CanBirth");
            this.CanFertilise = valueExtractor.GetValueFromDictionary<bool>(data, "CanFertilise");
            string processorName = valueExtractor.GetValueFromDictionary<string>(data, "Processor");
            this.Processor = processorName.IsNullOrEmpty() == false 
                ? GlobalConstants.GameManager.BioSexHandler.GetProcessor(processorName) 
                : new NeutralProcessor();
        }
    }
}