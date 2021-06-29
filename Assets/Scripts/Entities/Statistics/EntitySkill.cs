using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace JoyGodot.Assets.Scripts.Entities.Statistics
{
    [Serializable]
    public class EntitySkill : IEntitySkill
    {
        public ICollection<string> Tooltip { get; set; }
        
        public EntitySkill()
        {
        }
        
        public EntitySkill(
            string name, 
            int value, 
            int successThreshold,
            ICollection<string> tooltip)
        {
            this.Name = name;
            this.Value = value;
            this.SuccessThreshold = successThreshold;
            this.Tooltip = tooltip;
        }

        public int ModifyValue(int value)
        {
            this.Value = Math.Max(0, this.Value + value);
            return this.Value;
        }

        public int SetValue(int value)
        {
            this.Value = Math.Max(0, value);
            return this.Value;
        }

        public int SetThreshold(int value)
        {
            this.SuccessThreshold = Mathf.Clamp(value, 1, 10);
            return this.SuccessThreshold;
        }

        public string Name
        {
            get;
            protected set;
        }

        public int Value
        {
            get;
            protected set;
        }

        public int SuccessThreshold
        {
            get;
            protected set;
        }

        public Dictionary Save()
        {
            Dictionary saveDict = new Dictionary
            {
                {"Name", this.Name}, 
                {"Value", this.Value}, 
                {"SuccessThreshold", this.SuccessThreshold}
            };
            
            return saveDict;
        }

        public void Load(Dictionary data)
        {
            var valueExtractor = GlobalConstants.GameManager.ItemHandler.ValueExtractor;

            this.Name = valueExtractor.GetValueFromDictionary<string>(data, "Name");
            this.Value = valueExtractor.GetValueFromDictionary<int>(data, "Value");
            this.SuccessThreshold = valueExtractor.GetValueFromDictionary<int>(data, "SuccessThreshold");
        }
    }
}