using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using JoyLib.Code.Helpers;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace JoyLib.Code.Settings
{
    public class SettingsManager : IHandler<ISetting, string>
    {
        public IEnumerable<ISetting> Values => this.Settings.Values;
        
        public JSONValueExtractor ValueExtractor { get; protected set; }
        
        protected IDictionary<string, ISetting> Settings { get; set; }

        public SettingsManager()
        {
            this.Settings = this.Load().ToDictionary(setting => setting.Name, setting => setting);
            this.ValueExtractor = new JSONValueExtractor();
        }
        
        public ISetting Get(string name)
        {
            return this.Settings.TryGetValue(name, out ISetting setting) ? setting : null;
        }

        public bool Add(ISetting value)
        {
            if (this.Settings.ContainsKey(value.Name))
            {
                return false;
            }
            
            this.Settings.Add(value.Name, value);
            return true;
        }

        public bool Destroy(string key)
        {
            return this.Settings.Remove(key);
        }

        public IEnumerable<ISetting> Load()
        {
            List<ISetting> settings = new List<ISetting>();

            string[] files = System.IO.Directory.GetFiles(
                Directory.GetCurrentDirectory() +
                GlobalConstants.ASSETS_FOLDER +
                GlobalConstants.DATA_FOLDER +
                "Settings",
                "*.json");

            foreach (string file in files)
            {
                JSONParseResult result = JSON.Parse(File.ReadAllText(file));

                if (result.Error != Error.Ok)
                {
                    GD.PushWarning("Could not parse settings file " + file);
                    continue;
                }

                if (!(result.Result is Dictionary dictionary))
                {
                    GD.PushWarning("Could not parse JSON to Dictionary from " + file);
                    continue;
                }

                ICollection<Dictionary> settingsArray =
                    this.ValueExtractor
                        .GetArrayValuesCollectionFromDictionary<Dictionary>(
                            dictionary, 
                            "Settings");

                foreach (Dictionary settingsDict in settingsArray)
                {
                    string name = this.ValueExtractor.GetValueFromDictionary<string>(settingsDict, "Name");
                    object value = this.ValueExtractor.GetValueFromDictionary<object>(settingsDict, "ObjectValue");
                    
                    settings.Add(SettingsFactory.Create(name, value));
                }
            }

            return settings;
        }
        
        public void Dispose()
        {
            this.Settings = null;
            this.ValueExtractor = null;
        }

        ~SettingsManager()
        {
            this.Dispose();
        }
    }
}