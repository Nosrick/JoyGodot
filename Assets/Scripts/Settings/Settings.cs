using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Godot;
using Godot.Collections;
using JoyGodot.Assets.Scripts.Base_Interfaces;
using JoyGodot.Assets.Scripts.Helpers;
using Array = Godot.Collections.Array;

namespace JoyGodot.Assets.Scripts.Settings
{
    public interface ISetting : ISerialisationHandler
    {
        public string Name { get; }
        
        public object ObjectValue { get; }
        
        public ICollection<string> ValueNames { get; }
        
        public int Index { get; set; }
    }

    public static class SettingsFactory
    {
        public static Setting<T> CastToSetting<T>(string name, ICollection<object> values)
        {
            if (values.All(v => v is T))
            {
                return new Setting<T>(name, values.Cast<T>().ToList());
            }

            throw new InvalidOperationException("Values do not constrain to one type.");
        }
        
        public static ISetting Create(string name, ICollection<object> values)
        {
            if (values.Count == 0)
            {
                return new Setting<bool>(
                    "EMPTY VALUES",
                    new List<bool> {false});
            }

            try
            { 
                MethodInfo methodInfo = typeof(SettingsFactory).GetMethod(nameof(CastToSetting))?.MakeGenericMethod(values.FirstOrDefault()?.GetType());

                if (methodInfo is null)
                {
                    throw new InvalidOperationException();
                }
                
                return (ISetting) methodInfo.Invoke(null, new object[] { name, values });
            }
            catch (Exception e)
            {
                GD.PushError("Could not create setting " + name);
            }

            return new Setting<bool>(
                "EXCEPTION THROWN",
                new List<bool> {false});
        }
    }
    
    public class Setting<T> : ISetting
    {
        public string Name { get; protected set; }
        public object ObjectValue => this.ValuesRange.ElementAt(this.Index);

        public ICollection<string> ValueNames
        {
            get => this.ValuesRange.Select(v => v.ToString()).ToList();
        }

        public ICollection<T> ValuesRange { get; set; }

        public int Index
        {
            get => this.m_Index;
            set
            {
                if (value >= 0 && value < this.ValuesRange.Count)
                {
                    this.m_Index = value;
                }
            }
        }

        protected int m_Index;

        public T Value => (T) this.ObjectValue;

        protected static readonly JSONValueExtractor ValueExtractor = new JSONValueExtractor();
        
        public Setting(
            string name, 
            ICollection<T> values,
            int index = 0)
        {
            this.Name = name;
            this.ValuesRange = values;
            this.Index = index;
        }
        
        public Dictionary Save()
        {
            Dictionary saveDict = new Dictionary
            {
                {"Name", this.Name},
                {"ValuesRange", this.ValuesToArray()},
                {"Index", this.Index}
            };

            return saveDict;
        }

        protected Array ValuesToArray()
        {
            Array array = new Array();

            foreach (var value in this.ValuesRange)
            {
                array.Add(value);
            }

            return array;
        }

        protected string ValuesToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("[");

            for(int i = 0; i < this.ValuesRange.Count; i++)
            {
                builder.Append(this.ValuesRange.ElementAt(i));
                if (i != this.ValuesRange.Count - 1)
                {
                    builder.Append(",");
                }
                builder.AppendLine();
            }

            builder.AppendLine("]");

            return builder.ToString();
        }

        public void Load(Dictionary data)
        {
            this.Name =  ValueExtractor.GetValueFromDictionary<string>(data, "Name");
            this.ValuesRange = ValueExtractor.GetArrayValuesCollectionFromDictionary<T>(data, "ValuesRange");
            this.Index = ValueExtractor.GetValueFromDictionary<int>(data, "Index");
        }
    }
}