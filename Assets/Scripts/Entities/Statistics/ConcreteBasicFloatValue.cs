﻿using System;
using System.Collections.Generic;
using Godot.Collections;

namespace JoyGodot.Assets.Scripts.Entities.Statistics
{
    [Serializable]
    public class ConcreteBasicFloatValue : IBasicValue<float>
    {
        public ICollection<string> Tooltip { get; set; }

        public string Name { get; protected set; }
        public float Value { get; protected set; }

        public ConcreteBasicFloatValue()
        {
        }

        public ConcreteBasicFloatValue(string name, float value)
        {
            this.Name = name;
            this.Value = value;
        }
        public float ModifyValue(float value)
        {
            this.Value += value;
            return this.Value;
        }

        public float SetValue(float value)
        {
            this.Value = value;
            return this.Value;
        }

        public Dictionary Save()
        {
            Dictionary saveDict = new Dictionary();

            saveDict.Add("Name", this.Name);
            saveDict.Add("Value", this.Value);

            return saveDict;
        }

        public void Load(Dictionary data)
        {
            throw new NotImplementedException();
        }
    }
}