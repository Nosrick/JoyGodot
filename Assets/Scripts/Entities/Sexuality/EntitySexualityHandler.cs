using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using Godot.Collections;
using JoyGodot.Assets.Scripts.Entities.Sexuality.Processors;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.Scripting;
using Array = Godot.Collections.Array;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace JoyGodot.Assets.Scripts.Entities.Sexuality
{
    public class EntitySexualityHandler : IEntitySexualityHandler
    {
        public IEnumerable<ISexuality> Values => this.Sexualities.Values;

        public JSONValueExtractor ValueExtractor { get; protected set; }

        protected IDictionary<string, ISexuality> Sexualities { get; set; }

        protected IDictionary<string, ISexualityPreferenceProcessor> PreferenceProcessors { get; set; }

        public EntitySexualityHandler()
        {
            this.ValueExtractor = new JSONValueExtractor();
            this.Sexualities = this.Load().ToDictionary(sexuality => sexuality.Name, sexuality => sexuality);
        }

        public IEnumerable<ISexuality> Load()
        {
            this.PreferenceProcessors = ScriptingEngine.Instance
                .FetchAndInitialiseChildren<ISexualityPreferenceProcessor>()
                .ToDictionary(processor => processor.Name, processor => processor);

            List<ISexuality> sexualities = new List<ISexuality>();

            string[] files = Directory.GetFiles(
                Directory.GetCurrentDirectory() +
                GlobalConstants.ASSETS_FOLDER +
                GlobalConstants.DATA_FOLDER +
                "/Sexualities",
                "*.json", 
                SearchOption.AllDirectories);
            foreach (string file in files)
            {
                JSONParseResult result = JSON.Parse(File.ReadAllText(file));

                if (result.Error != Error.Ok)
                {
                    this.ValueExtractor.PrintFileParsingError(result, file);
                    continue;
                }

                if (!(result.Result is Dictionary dict))
                {
                    GlobalConstants.ActionLog.Log("Could not parse JSON into Dictionary, in file " + file, LogLevel.Warning);
                    continue;
                }

                ICollection<Dictionary> sexualityCollection = this.ValueExtractor.GetCollectionFromArray<Dictionary>(
                    this.ValueExtractor.GetValueFromDictionary<Array>(dict, "Sexualities"));

                foreach (Dictionary sexuality in sexualityCollection)
                {
                    string name = this.ValueExtractor.GetValueFromDictionary<string>(sexuality, "Name");
                    bool decaysNeed = !sexuality.Contains("DecaysNeed") 
                                      || this.ValueExtractor.GetValueFromDictionary<bool>(sexuality, "DecaysNeed");
                    int matingThreshold = sexuality.Contains("MatingThreshold")
                        ? this.ValueExtractor.GetValueFromDictionary<int>(sexuality, "MatingThreshold")
                        : 0;
                    string processorName = sexuality.Contains("Processor")
                        ? this.ValueExtractor.GetValueFromDictionary<string>(sexuality, "Processor")
                        : "asexual";
                    ICollection<string> tags =
                        this.ValueExtractor.GetArrayValuesCollectionFromDictionary<string>(sexuality, "Tags");
                    var preferenceProcessor = this.GetProcessor(processorName);
                    
                    sexualities.Add(
                        new BaseSexuality(
                            name,
                            decaysNeed,
                            matingThreshold,
                            preferenceProcessor,
                            tags));
                }
            }

            IEnumerable<ISexuality> extraSexualities =
                ScriptingEngine.Instance.FetchAndInitialiseChildren<ISexuality>();
            sexualities.AddRange(extraSexualities);

            return sexualities;
        }

        public ISexualityPreferenceProcessor GetProcessor(string name)
        {
            return this.PreferenceProcessors
                       .FirstOrDefault(pair => pair.Key.Equals(name, StringComparison.OrdinalIgnoreCase))
                       .Value
                   ?? new AsexualProcessor();
        }

        public ISexuality Get(string sexuality)
        {
            if (this.Sexualities is null)
            {
                throw new InvalidOperationException("Sexuality index was null.");
            }

            if (this.Sexualities.Any(pair => pair.Key.Equals(sexuality, StringComparison.OrdinalIgnoreCase)))
            {
                return this.Sexualities.First(pair => pair.Key.Equals(sexuality, StringComparison.OrdinalIgnoreCase))
                    .Value;
            }

            throw new InvalidOperationException("Sexuality of type " + sexuality + " not found.");
        }

        public bool Add(ISexuality value)
        {
            if (this.Sexualities.ContainsKey(value.Name))
            {
                return false;
            }

            this.Sexualities.Add(value.Name, value);
            return true;
        }

        public bool Destroy(string key)
        {
            if (this.Sexualities.ContainsKey(key) == false)
            {
                return false;
            }

            this.Sexualities[key] = null;
            this.Sexualities.Remove(key);
            return true;
        }

        public void Dispose()
        {
            this.Sexualities = null;
            this.PreferenceProcessors = null;
        }

        ~EntitySexualityHandler()
        {
            this.Dispose();
        }
    }
}