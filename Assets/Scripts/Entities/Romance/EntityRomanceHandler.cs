using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using Godot.Collections;
using JoyLib.Code.Entities.Romance.Processors;
using JoyLib.Code.Helpers;
using JoyLib.Code.Scripting;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace JoyLib.Code.Entities.Romance
{
    public class EntityRomanceHandler : IEntityRomanceHandler
    {
        protected IDictionary<string, IRomance> RomanceTypes { get; set; }
        
        public JSONValueExtractor ValueExtractor { get; protected set; }
        
        protected IDictionary<string, IRomanceProcessor> Processors { get; set; }
        
        public IEnumerable<IRomance> Values => this.RomanceTypes.Values.ToArray();

        public EntityRomanceHandler()
        {
            this.ValueExtractor = new JSONValueExtractor();
            this.RomanceTypes = this.Load().ToDictionary(romance => romance.Name, romance => romance);
        }

        public IEnumerable<IRomance> Load()
        {
            List<IRomance> romances = new List<IRomance>();

            this.Processors = ScriptingEngine.Instance.FetchAndInitialiseChildren<IRomanceProcessor>()
                .ToDictionary(processor => processor.Name, processor => processor);
            
            string[] files =
                Directory.GetFiles(
                    Directory.GetCurrentDirectory() + 
                    GlobalConstants.ASSETS_FOLDER +
                    GlobalConstants.DATA_FOLDER + 
                    "/Romance",
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
                    GlobalConstants.ActionLog.Log("Failed to parse JSON in " + file + " into a Dictionary.", LogLevel.Warning);
                    continue;
                }

                ICollection<Dictionary> romanceCollection =
                    this.ValueExtractor.GetArrayValuesCollectionFromDictionary<Dictionary>(dictionary, "Romance");

                foreach (Dictionary romance in romanceCollection)
                {
                    string name = this.ValueExtractor.GetValueFromDictionary<string>(romance, "Name");
                    bool decaysNeed = romance.Contains("DecaysNeed") 
                                      && this.ValueExtractor.GetValueFromDictionary<bool>(romance, "DecaysNeed");
                    
                    int romanceThreshold = romance.Contains("RomanceThreshold")
                        ? this.ValueExtractor.GetValueFromDictionary<int>(romance, "RomanceThreshold")
                        : 0;

                    int bondingThreshold = romance.Contains("BondingThreshold")
                        ? this.ValueExtractor.GetValueFromDictionary<int>(romance, "BondingThreshold")
                        : 0;
                    string processorString = romance.Contains("Processor")
                        ? this.ValueExtractor.GetValueFromDictionary<string>(romance, "Processor")
                        : "aromantic";
                    ICollection<string> tags =
                        this.ValueExtractor.GetArrayValuesCollectionFromDictionary<string>(romance, "Tags");

                    IRomanceProcessor processor =
                        this.Processors.Values.FirstOrDefault(preferenceProcessor => 
                            preferenceProcessor.Name.Equals(processorString, StringComparison.OrdinalIgnoreCase)) ??
                        new AromanticProcessor();
                                
                    romances.Add(
                        new BaseRomance(
                            name,
                            decaysNeed,
                            romanceThreshold,
                            bondingThreshold,
                            processor,
                            tags));
                }
            }

            romances.AddRange(ScriptingEngine.Instance.FetchAndInitialiseChildren<IRomance>());
            return romances;
        }
        
        public IRomance Get(string romance)
        {
            if (this.RomanceTypes is null)
            {
                throw new InvalidOperationException("Sexuality search was null.");
            }

            if (this.RomanceTypes.Any(r => r.Key.Equals(romance, StringComparison.OrdinalIgnoreCase)))
            {
                return this.RomanceTypes.First(r => r.Key.Equals(romance, StringComparison.OrdinalIgnoreCase)).Value;
            }
            throw new InvalidOperationException("Sexuality of type " + romance + " not found.");
        }

        public IRomanceProcessor GetProcessor(string name)
        {
            return this.Processors.TryGetValue(name, out IRomanceProcessor processor)
                ? processor
                : new AromanticProcessor();
        }

        public bool Add(IRomance value)
        {
            if (this.RomanceTypes.ContainsKey(value.Name))
            {
                return false;
            }

            this.RomanceTypes.Add(value.Name, value);
            return true;
        }

        public bool Destroy(string key)
        {
            return this.RomanceTypes.ContainsKey(key) != false && this.RomanceTypes.Remove(key);
        }

        public void Dispose()
        {
            this.RomanceTypes = null;
            this.Processors = null;
        }
    }
}