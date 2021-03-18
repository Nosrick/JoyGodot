using System;
using System.Collections;
using System.Collections.Generic;
using Castle.Core.Internal;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace JoyLib.Code.Helpers
{
    public class JSONValueExtractor
    {
        public ICollection<T> GetArrayValuesCollectionFromDictionary<T>(IDictionary dict, string search)
        {
            if (dict.Contains(search) == false)
            {
                return new List<T>();
            }

            if (!(dict[search] is Array array))
            {
                return new List<T>();
            }

            ICollection<T> data = this.GetCollectionFromArray<T>(array);

            return data;
        }

        public ICollection<T> GetCollectionFromArray<T>(Array array)
        {
            List<T> data = new List<T>();
            foreach (var d in array)
            {
                data.Add(this.GetValueFromProperty<T>(d));
            }

            return data;
        }

        public T GetValueFromDictionary<T>(IDictionary dict, string search)
        {
            if (dict.Contains(search) == false)
            {
                return default;
            }

            return this.GetValueFromProperty<T>(dict[search]);
        }

        public T GetValueFromProperty<T>(object obj)
        {
            return (T) Convert.ChangeType(obj, typeof(T));
        }

        public IDictionary<TKey, TValue> CastToDictionary<TKey, TValue>(IDictionary dictionary)
        {
            IDictionary<TKey, TValue> dict = new System.Collections.Generic.Dictionary<TKey, TValue>();

            foreach (var key in dictionary.Keys)
            {
                dict.Add(
                    this.GetValueFromProperty<TKey>(key),
                    this.GetValueFromProperty<TValue>(dictionary[key]));
            }
            
            return dict;
        }

        public void PrintFileParsingError(JSONParseResult result, string file)
        {
            GlobalConstants.ActionLog.Log("Could not parse JSON in " + file, LogLevel.Warning);
            GlobalConstants.ActionLog.Log(result.ErrorString, LogLevel.Warning);
            GlobalConstants.ActionLog.Log("On line: " + result.ErrorLine, LogLevel.Warning);
        }
    }
}