using System;
using System.Collections.Generic;
using JoyLib.Code.Entities.Sexes.Processors;

namespace JoyLib.Code.Entities.Sexes
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
    }
}