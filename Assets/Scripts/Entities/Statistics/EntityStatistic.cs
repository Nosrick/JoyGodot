﻿using System;
using Godot.Collections;

namespace JoyLib.Code.Entities.Statistics
{
    [Serializable]
    public class EntityStatistic : IEntityStatistic
    {
        public const string STRENGTH    =   "strength";
        public const string AGILITY     =   "agility";
        public const string ENDURANCE   =   "endurance";

        public const string INTELLECT   =   "intellect";
        public const string CUNNING     =   "cunning";
        public const string FOCUS       =   "focus";

        public const string PERSONALITY =   "personality";
        public const string SUAVITY     =   "suavity";
        public const string WIT         =   "wit";

        public static readonly string[] NAMES = new string[] { STRENGTH, AGILITY, ENDURANCE,
                                                    INTELLECT, CUNNING, FOCUS,
                                                    PERSONALITY, SUAVITY, WIT };

        public EntityStatistic()
        {
        }
        
        public EntityStatistic(string name, int value, int successThreshold)
        {
            this.Name = name;
            this.Value = value;
            this.SuccessThreshold = successThreshold;
        }

        public int ModifyValue(int value)
        {
            this.Value = Math.Max(1, this.Value + value);
            return this.Value;
        }

        public int SetValue(int value)
        {
            this.Value = Math.Max(1, value);
            return this.Value;
        }

        public string Name
        {
            get;
            set;
        }

        public int Value
        {
            get;
            set;
        }

        public int SuccessThreshold
        {
            get;
            set;
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
