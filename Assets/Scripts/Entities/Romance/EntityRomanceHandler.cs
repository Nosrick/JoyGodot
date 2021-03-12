using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JoyLib.Code.Entities.Romance.Processors;
using JoyLib.Code.Scripting;

namespace JoyLib.Code.Entities.Romance
{
    public class EntityRomanceHandler : IEntityRomanceHandler
    {
        protected IDictionary<string, IRomance> RomanceTypes { get; set; }
        
        protected IDictionary<string, IRomanceProcessor> Processors { get; set; }
        
        public IEnumerable<IRomance> Values => this.RomanceTypes.Values.ToArray();

        public EntityRomanceHandler()
        {
            this.RomanceTypes = this.Load().ToDictionary(romance => romance.Name, romance => romance);
        }

        public IEnumerable<IRomance> Load()
        {
            List<IRomance> romances = new List<IRomance>();

            this.Processors = ScriptingEngine.Instance.FetchAndInitialiseChildren<IRomanceProcessor>()
                .ToDictionary(processor => processor.Name, processor => processor);
            
            string[] files =
                Directory.GetFiles(Directory.GetCurrentDirectory() + GlobalConstants.DATA_FOLDER + "/Romance",
                    "*.json", SearchOption.AllDirectories);
            
            foreach(string file in files)
            {
                /*
                using (StreamReader reader = new StreamReader(file))
                {
                    using (JsonTextReader jsonReader = new JsonTextReader(reader))
                    {
                        try
                        {
                            JObject jToken = JObject.Load(jsonReader);

                            if (jToken.IsNullOrEmpty())
                            {
                                continue;
                            }

                            foreach (JToken child in jToken["Romance"])
                            {
                                string name = (string) child["Name"];
                                bool decaysNeed = (bool) (child["DecaysNeed"] ?? false);
                                int romanceThreshold = (int) (child["RomanceThreshold"] ?? 0);
                                int bondingThreshold = (int) (child["BondingThreshold"] ?? 0);
                                string processorString = (string) (child["Processor"] ?? "aromantic");
                                IEnumerable<string> tags = child["Tags"] is null
                                    ? new string[0]
                                    : child["Tags"].Select(token => (string) token);

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
                        catch (Exception e)
                        {
                            GlobalConstants.ActionLog.AddText("Could not load sexes in " + file);
                            GlobalConstants.ActionLog.StackTrace(e);
                        }
                        finally
                        {
                            jsonReader.Close();
                            reader.Close();
                        }
                    }
                }
                */
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