using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JoyLib.Code.Scripting;

namespace JoyLib.Code.Entities.Sexuality
{
    public class EntitySexualityHandler : IEntitySexualityHandler
    {
        public IEnumerable<ISexuality> Values => this.Sexualities.Values;
        
        protected IDictionary<string, ISexuality> Sexualities { get; set; }
        
        protected IDictionary<string, ISexualityPreferenceProcessor> PreferenceProcessors { get; set; }

        public EntitySexualityHandler()
        {
            this.Sexualities = this.Load().ToDictionary(sexuality => sexuality.Name, sexuality => sexuality);
        }

        public IEnumerable<ISexuality> Load()
        {
            this.PreferenceProcessors = ScriptingEngine.Instance
                .FetchAndInitialiseChildren<ISexualityPreferenceProcessor>()
                .ToDictionary(processor => processor.Name, processor => processor);

            List<ISexuality> sexualities = new List<ISexuality>();

            string[] files =
                Directory.GetFiles(Directory.GetCurrentDirectory() + GlobalConstants.DATA_FOLDER + "/Sexualities",
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

                            foreach (JToken child in jToken["Sexualities"])
                            {
                                string name = (string) child["Name"];
                                bool decaysNeed = (bool) (child["DecaysNeed"] ?? true);
                                int matingThreshold = (int) (child["MatingThreshold"] ?? 0);
                                string processorString = (string) (child["Processor"] ?? "Asexual");
                                IEnumerable<string> tags = child["Tags"] is null
                                    ? new string[0]
                                    : child["Tags"].Select(token => (string) token);

                                ISexualityPreferenceProcessor processor =
                                    this.PreferenceProcessors.Values.FirstOrDefault(preferenceProcessor => 
                                        preferenceProcessor.Name.Equals(processorString, StringComparison.OrdinalIgnoreCase)) ??
                                    new AsexualProcessor();
                                
                                sexualities.Add(
                                    new BaseSexuality(
                                        name,
                                        decaysNeed,
                                        matingThreshold,
                                        processor,
                                        tags));
                            }
                        }
                        catch (Exception e)
                        {
                            GlobalConstants.ActionLog.AddText("Could not load sexualities in " + file);
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
            IEnumerable<ISexuality> extraSexualities = ScriptingEngine.Instance.FetchAndInitialiseChildren<ISexuality>();
            sexualities.AddRange(extraSexualities);

            return sexualities;
        }

        public ISexuality Get(string sexuality)
        {
            if (this.Sexualities is null)
            {
                throw new InvalidOperationException("Sexuality index was null.");
            }

            if (this.Sexualities.Any(pair => pair.Key.Equals(sexuality, StringComparison.OrdinalIgnoreCase)))
            {
                return this.Sexualities.First(pair => pair.Key.Equals(sexuality, StringComparison.OrdinalIgnoreCase)).Value;
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
