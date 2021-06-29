using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace JoyGodot.Assets.Scripts.Entities.Statistics
{
    [Serializable]
    public class ConcreteDerivedIntValue : IDerivedValue
    {
        public ICollection<string> Tooltip { get; set; }
        
        public string Name
        {
            get;
            set;
        }

        public int Maximum => this.Base + this.Enhancement;

        public int Value
        {
            get;
            protected set;
        }

        public int Base { get; protected set; }

        public int Enhancement { get; protected set; }

        public ConcreteDerivedIntValue()
        {}

        public ConcreteDerivedIntValue(string name, int value, int maximum)
        {
            this.Name = name;
            this.Base = value;
            this.Value = value;
        }

        public ConcreteDerivedIntValue(
            string name,
            int baseValue,
            int enhancement,
            int value)
        {
            this.Name = name;
            this.Base = baseValue;
            this.Enhancement = enhancement;
            this.Value = Mathf.Clamp(value, -this.Maximum, this.Maximum); 
        }

        public int SetValue(int data)
        {
            this.Value = Mathf.Clamp(data, -this.Maximum, this.Maximum);
            return this.Value;
        }

        public int ModifyValue(int value)
        {
            this.Value = Mathf.Clamp(this.Value + value, -this.Maximum, this.Maximum);
            return this.Value;
        }

        public int SetBase(int data)
        {
            this.Base = Math.Max(1, data);
            return this.Base;
        }

        public int SetEnhancement(int data, bool changeToMatch = true)
        {
            this.Enhancement = Math.Max(0, data);
            if (changeToMatch)
            {
                this.ModifyValue(data);
            }
            return this.Enhancement;
        }

        public Dictionary Save()
        {
            Dictionary saveDict = new Dictionary
            {
                {"Name", this.Name}, 
                {"Base", this.Base}, 
                {"Enhancement", this.Enhancement}, 
                {"Value", this.Value}
            };
            
            return saveDict;
        }

        public void Load(Dictionary data)
        {
            var valueExtractor = GlobalConstants.GameManager.DerivedValueHandler.ValueExtractor;

            this.Name = valueExtractor.GetValueFromDictionary<string>(data, "Name");
            this.Base = valueExtractor.GetValueFromDictionary<int>(data, "Base");
            this.Enhancement = valueExtractor.GetValueFromDictionary<int>(data, "Enhancement");
            this.Value = valueExtractor.GetValueFromDictionary<int>(data, "Value");
        }
    }

    public static class DerivedValueName
    {
        public const string HITPOINTS = "hitpoints";
        public const string CONCENTRATION = "concentration";
        public const string COMPOSURE = "composure";
        public const string MANA = "mana";
        public const string DURABILITY = "durability";
    }
}
