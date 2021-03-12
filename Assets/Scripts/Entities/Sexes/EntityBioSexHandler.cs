using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JoyLib.Code.Entities.Sexes.Processors;
using JoyLib.Code.Scripting;

namespace JoyLib.Code.Entities.Sexes
{
    public class EntityBioSexHandler : IEntityBioSexHandler
    {
        public IEnumerable<IBioSex> Values => this.Sexes.Values;

        protected IDictionary<string, IBioSexProcessor> Processors { get; set; }
        
        protected IDictionary<string, IBioSex> Sexes { get; set; }

        public EntityBioSexHandler()
        {
            this.Sexes = this.Load().ToDictionary(sex => sex.Name, sex => sex);
        }

        public IEnumerable<IBioSex> Load()
        {
            List<IBioSex> sexes = new List<IBioSex>();

            this.Processors = ScriptingEngine.Instance.FetchAndInitialiseChildren<IBioSexProcessor>()
                .ToDictionary(processor => processor.Name, processor => processor);
            
            string[] files =
                Directory.GetFiles(Directory.GetCurrentDirectory() + GlobalConstants.DATA_FOLDER + "/Sexes",
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

                            foreach (JToken child in jToken["Sexes"])
                            {
                                string name = (string) child["Name"];
                                bool canBirth = (bool) (child["CanBirth"] ?? false);
                                bool canFertilise = (bool) (child["CanFertilise"] ?? false);
                                string processorString = (string) (child["Processor"] ?? "Neutral");
                                IEnumerable<string> tags = child["Tags"] is null
                                    ? new string[0]
                                    : child["Tags"].Select(token => (string) token);

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

            sexes.AddRange(ScriptingEngine.Instance.FetchAndInitialiseChildren<IBioSex>());
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
        }

        ~EntityBioSexHandler()
        {
            this.Dispose();
        }
    }
}
