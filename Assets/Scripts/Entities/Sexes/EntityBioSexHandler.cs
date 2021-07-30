using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using Godot.Collections;
using JoyGodot.Assets.Scripts.Entities.Sexes.Processors;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.Scripting;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace JoyGodot.Assets.Scripts.Entities.Sexes
{
    public class EntityBioSexHandler : IEntityBioSexHandler
    {
        public IEnumerable<IBioSex> Values => this.Sexes.Values;
        
        public JSONValueExtractor ValueExtractor { get; protected set; }

        protected IDictionary<string, IBioSexProcessor> Processors { get; set; }
        
        protected IDictionary<string, IBioSex> Sexes { get; set; }

        public EntityBioSexHandler()
        {
            this.ValueExtractor = new JSONValueExtractor();
            this.Sexes = this.Load().ToDictionary(sex => sex.Name, sex => sex);
        }

        public IEnumerable<IBioSex> Load()
        {
            List<IBioSex> sexes = new List<IBioSex>();

            this.Processors = GlobalConstants.ScriptingEngine.FetchAndInitialiseChildren<IBioSexProcessor>()
                .ToDictionary(processor => processor.Name, processor => processor);
            
            string[] files =
                Directory.GetFiles(
                    Directory.GetCurrentDirectory() +
                    GlobalConstants.ASSETS_FOLDER + 
                    GlobalConstants.DATA_FOLDER + 
                    "/Sexes",
                    "*.json", 
                    SearchOption.AllDirectories);
            foreach(string file in files)
            {
                JSONParseResult result = JSON.Parse(File.ReadAllText(file));

                if (result.Error != Error.Ok)
                {
                    this.ValueExtractor.PrintFileParsingError(result, file);
                    continue;
                }

                if (!(result.Result is Dictionary dictionary))
                {
                    GlobalConstants.ActionLog.Log("Could not parse JSON in " + file + " into a Dictionary.", LogLevel.Warning);
                    continue;
                }

                ICollection<Dictionary> sexesCollection =
                    this.ValueExtractor.GetArrayValuesCollectionFromDictionary<Dictionary>(dictionary, "Sexes");

                foreach (Dictionary innerDict in sexesCollection)
                {
                    string name = this.ValueExtractor.GetValueFromDictionary<string>(innerDict, "Name");
                    bool canBirth = this.ValueExtractor.GetValueFromDictionary<bool>(innerDict, "CanBirth");
                    bool canFertilise = this.ValueExtractor.GetValueFromDictionary<bool>(innerDict, "CanFertilise");
                    string processorString = innerDict.Contains("Processor")
                        ? this.ValueExtractor.GetValueFromDictionary<string>(innerDict, "Processor")
                        : "Neutral";

                    ICollection<string> tags =
                        this.ValueExtractor.GetArrayValuesCollectionFromDictionary<string>(innerDict, "Tags");

                    IBioSexProcessor processor =
                        this.Processors.Values.FirstOrDefault(preferenceProcessor => 
                            preferenceProcessor.Name.Equals(processorString, StringComparison.OrdinalIgnoreCase)) ??
                        new NeutralProcessor();
                                
                    sexes.Add(
                        new BaseBioSex(
                            name,
                            canBirth,
                            canFertilise,
                            processor,
                            tags));
                }
            }

            sexes.AddRange(GlobalConstants.ScriptingEngine.FetchAndInitialiseChildren<IBioSex>());
            return sexes;
        }

        public IBioSex Get(string name)
        {
            if(this.Sexes.Any(sex => sex.Key.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                return this.Sexes.First(sex => sex.Key.Equals(name, StringComparison.OrdinalIgnoreCase)).Value;
            }
            return null;
        }

        public IEnumerable<IBioSex> GetMany(IEnumerable<string> keys)
        {
            return keys.Select(this.Get);
        }

        public IBioSexProcessor GetProcessor(string name)
        {
            return this.Processors.TryGetValue(name, out IBioSexProcessor processor) 
                ? processor 
                : new NeutralProcessor();
        }

        public bool Add(IBioSex value)
        {
            if (this.Sexes.ContainsKey(value.Name))
            {
                return false;
            }

            this.Sexes.Add(value.Name, value);
            return true;
        }

        public bool Destroy(string key)
        {
            if (!this.Sexes.ContainsKey(key))
            {
                return false;
            }
            this.Sexes[key] = null;
            this.Sexes.Remove(key);
            return true;

        }

        public void Dispose()
        {
            this.Sexes = null;
            this.Processors = null;
            this.ValueExtractor = null;
        }
    }
}
