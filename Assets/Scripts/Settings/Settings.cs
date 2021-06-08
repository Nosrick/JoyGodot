using System;
using System.Reflection;
using Godot;
using Godot.Collections;
using JoyLib.Code.Helpers;

namespace JoyLib.Code.Settings
{
    public interface ISetting
    {
        public string Name { get; }
        
        public object ObjectValue { get; }
    }

    public static class SettingsFactory
    {
        private static Setting<T> InternalCreate<T>(string name, object value)
        {
            return new Setting<T>(name, (T) value);
        }
        
        public static ISetting Create(string name, object value)
        {
            MethodInfo methodInfo = typeof(SettingsFactory).GetMethod(nameof(InternalCreate))?.MakeGenericMethod(value.GetType());

            return (ISetting) methodInfo?.Invoke(null, new[] { name, value });
        }
    }
    
    public class Setting<T> : ISetting
    {
        public string Name { get; protected set; }
        public object ObjectValue { get; protected set; }

        public T Value
        {
            get => (T) this.ObjectValue;
            set => this.ObjectValue = value;
        }

        public Type MyType => this.ObjectValue.GetType();

        protected static readonly JSONValueExtractor ValueExtractor = new JSONValueExtractor();
        
        public Setting(string name, T value = default)
        {
            this.Name = name;
            this.ObjectValue = value;
        }
        
        public string Save()
        {
            return JSON.Print(this);
        }

        public void Load(string data)
        {
            var result = JSON.Parse(data);
            if (result.Error != Error.Ok)
            {
                GD.PushError("Failed trying to load setting " + this.Name);
                return;
            }
            
            if (!(result.Result is Dictionary dictionary))
            {
                GD.PushError("Could not parse JSON to Dictionary for setting " + this.Name);
                return;
            }

            this.Name =  ValueExtractor.GetValueFromDictionary<string>(dictionary, "Name");
            this.ObjectValue = ValueExtractor.GetValueFromDictionary<object>(dictionary, "ObjectValue");
        }
    }
}