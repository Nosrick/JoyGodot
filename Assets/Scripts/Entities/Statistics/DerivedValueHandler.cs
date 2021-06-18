﻿using System;
using System.Collections.Generic;
using System.Linq;

using Godot;
using Godot.Collections;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.Scripting;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace JoyGodot.Assets.Scripts.Entities.Statistics
{
    public class DerivedValueHandler : IDerivedValueHandler
    {
        protected IDictionary<string, string> Formulas { get; set; }
        protected IDictionary<string, string> EntityStandardFormulas { get; set; }
        protected IDictionary<string, string> ItemStandardFormulas { get; set; }
        protected IDictionary<string, IDerivedValue> DerivedValues { get; set; }
        
        protected IDictionary<string, Color> DerivedValueBackgroundColours { get; set; }
        protected IDictionary<string, Color> DerivedValueTextColours { get; set; }
        protected IDictionary<string, Color> DerivedValueOutlineColours { get; set; }

        public IEnumerable<IDerivedValue> Values => this.DerivedValues.Values;

        public JSONValueExtractor ValueExtractor { get; protected set; }

        protected IEntityStatisticHandler StatisticHandler { get; set; }
        protected IEntitySkillHandler SkillHandler { get; set; }

        protected readonly string ENTITY_FILE;
        protected readonly string ITEM_FILE;

        public DerivedValueHandler(
            IEntityStatisticHandler statisticHandler,
            IEntitySkillHandler skillHandler)
        {
            this.ValueExtractor = new JSONValueExtractor();
            this.StatisticHandler = statisticHandler;
            this.SkillHandler = skillHandler;

            this.ENTITY_FILE = Directory.GetCurrentDirectory() +
                               GlobalConstants.ASSETS_FOLDER + 
                               GlobalConstants.DATA_FOLDER +
                               "EntityDerivedValues.json";

            this.ITEM_FILE = Directory.GetCurrentDirectory() + 
                             GlobalConstants.ASSETS_FOLDER + 
                             GlobalConstants.DATA_FOLDER + 
                             "ItemDerivedValues.json";

            this.DerivedValueOutlineColours = new System.Collections.Generic.Dictionary<string, Color>();
            this.DerivedValueBackgroundColours = new System.Collections.Generic.Dictionary<string, Color>();
            this.DerivedValueTextColours = new System.Collections.Generic.Dictionary<string, Color>();
            this.Load();
            this.Formulas = new System.Collections.Generic.Dictionary<string, string>(this.EntityStandardFormulas);
            foreach (KeyValuePair<string, string> pair in this.ItemStandardFormulas)
            {
                this.Formulas.Add(pair.Key, pair.Value);
            }
        }
        
        public IDerivedValue Get(string name)
        {
            return this.DerivedValues.TryGetValue(name, out IDerivedValue value) ? value.Copy() : null;
        }

        public bool Add(IDerivedValue value)
        {
            if (this.DerivedValues.ContainsKey(value.Name))
            {
                return false;
            }

            this.DerivedValues.Add(value.Name, value);
            return true;
        }

        public bool Destroy(string key)
        {
            if (this.DerivedValues.ContainsKey(key) == false)
            {
                return false;
            }

            this.DerivedValues[key] = null;
            this.DerivedValues.Remove(key);
            return true;
        }

        public IEnumerable<IDerivedValue> Load()
        {
            this.EntityStandardFormulas = this.LoadFormulasFromFile(this.ENTITY_FILE);
            this.ItemStandardFormulas = this.LoadFormulasFromFile(this.ITEM_FILE);

            this.DerivedValues = new System.Collections.Generic.Dictionary<string, IDerivedValue>();
            foreach (var pair in this.EntityStandardFormulas)
            {
                this.DerivedValues.Add(
                    pair.Key,
                    new ConcreteDerivedIntValue());
            }

            foreach (var pair in this.ItemStandardFormulas)
            {
                this.DerivedValues.Add(
                    pair.Key,
                    new ConcreteDerivedIntValue());
            }

            return this.DerivedValues.Values;
        }

        public System.Collections.Generic.Dictionary<string, string> LoadFormulasFromFile(string file)
        {
            System.Collections.Generic.Dictionary<string, string> formulas = new System.Collections.Generic.Dictionary<string, string>();

            JSONParseResult result = JSON.Parse(File.ReadAllText(file));
            
            if (result.Error != Error.Ok)
            {
                this.ValueExtractor.PrintFileParsingError(result, file);
                return formulas;
            }

            if (!(result.Result is Dictionary dictionary))
            {
                GlobalConstants.ActionLog.Log("Failed to parse JSON from " + file + " into a Dictionary.", LogLevel.Warning);
                return formulas;
            }

            ICollection<Dictionary> dvCollection =
                this.ValueExtractor.GetArrayValuesCollectionFromDictionary<Dictionary>(dictionary, "DerivedValues");

            foreach (Dictionary dv in dvCollection)
            {
                string name = this.ValueExtractor.GetValueFromDictionary<string>(dv, "Name");

                if (name.IsNullOrEmpty())
                {
                    continue;
                }
                                
                formulas.Add(
                    name,
                    this.ValueExtractor.GetValueFromDictionary<string>(dv, "Formula"));

                string colourCode = dv.Contains("BackgroundColour")
                    ? this.ValueExtractor.GetValueFromDictionary<string>(dv, "BackgroundColour")
                    : "#888888";
                this.DerivedValueBackgroundColours.Add(name, new Color(colourCode));

                colourCode = dv.Contains("TextColour")
                    ? this.ValueExtractor.GetValueFromDictionary<string>(dv, "BackgroundColour")
                    : "#ffffff";
                this.DerivedValueTextColours.Add(name, new Color(colourCode));

                colourCode = dv.Contains("OutlineColour")
                    ? this.ValueExtractor.GetValueFromDictionary<string>(dv, "BackgroundColour")
                    : "#000000";
                this.DerivedValueOutlineColours.Add(name, new Color(colourCode));
            }

            return formulas;
        }
        
        public IDerivedValue Calculate<T>(string name, IEnumerable<IBasicValue<T>> components) 
            where T : struct
        {
            if (this.Formulas.Any(pair => pair.Key.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                string formula = this.Formulas.First(pair => pair.Key.Equals(name, StringComparison.OrdinalIgnoreCase))
                    .Value;

                int value = this.Calculate(components, formula);
                return new ConcreteDerivedIntValue(name, value, value);
            }
            
            throw new InvalidOperationException("Could not find formula for name of value " + name);
        }

        public System.Collections.Generic.Dictionary<string, IDerivedValue> GetEntityStandardBlock(IEnumerable<IBasicValue<int>> components)
        {
            System.Collections.Generic.Dictionary<string, IDerivedValue> values = new System.Collections.Generic.Dictionary<string, IDerivedValue>();
            foreach (KeyValuePair<string, string> pair in this.EntityStandardFormulas)
            {
                int result = Math.Max(1, this.Calculate<int>(components, pair.Value)); 
                values.Add(
                    pair.Key,
                    new ConcreteDerivedIntValue(
                        pair.Key,
                        result,
                        result));
            }

            return values;
        }

        public System.Collections.Generic.Dictionary<string, IDerivedValue> GetItemStandardBlock(IEnumerable<IBasicValue<float>> components)
        {
            System.Collections.Generic.Dictionary<string, IDerivedValue> values = new System.Collections.Generic.Dictionary<string, IDerivedValue>();
            foreach (KeyValuePair<string, string> pair in this.ItemStandardFormulas)
            {
                int result = Math.Max(1, this.Calculate(components, pair.Value)); 
                values.Add(
                    pair.Key,
                    new ConcreteDerivedIntValue(
                        pair.Key,
                        result,
                        result));
            }

            return values;
        }

        public bool AddFormula(string name, string formula)
        {
            if (this.Formulas.Any(pair => pair.Key.Equals(name, StringComparison.OrdinalIgnoreCase)) == false)
            {
                this.Formulas.Add(name, formula);
            }

            return false;
        }

        public Color GetBackgroundColour(string name)
        {
            if (this.DerivedValueBackgroundColours.ContainsKey(name))
            {
                return this.DerivedValueBackgroundColours[name];
            }
            return Colors.Gray;
        }
        
        public Color GetTextColour(string name)
        {
            if (this.DerivedValueTextColours.ContainsKey(name))
            {
                return this.DerivedValueTextColours[name];
            }
            return Colors.Gray;
        }
        
        public Color GetOutlineColour(string name)
        {
            if (this.DerivedValueOutlineColours.ContainsKey(name))
            {
                return this.DerivedValueOutlineColours[name];
            }
            return Colors.Gray;
        }

        public int Calculate<T>(IEnumerable<IBasicValue<T>> components, string formula)
            where T : struct
        {
            string eval = formula;
            foreach (IBasicValue<T> value in components)
            {
                eval = eval.Replace(value.Name, value.Value.ToString());
            }

            return GlobalConstants.ScriptingEngine.Evaluate<int>(eval);
        }

        public void Dispose()
        {
            this.DerivedValues = null;
            this.DerivedValueBackgroundColours = null;
            this.DerivedValueOutlineColours = null;
            this.DerivedValueTextColours = null;
            this.EntityStandardFormulas = null;
            this.ItemStandardFormulas = null;
        }

        ~DerivedValueHandler()
        {
            this.Dispose();
        }
    }
}