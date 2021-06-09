using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        protected string SettingsDirectory => Directory.GetCurrentDirectory() +
                                              GlobalConstants.ASSETS_FOLDER +
                                              GlobalConstants.DATA_FOLDER +
                                              "Settings";

        public const string DYSLEXIC_MODE = "Dyslexic Mode";
        public const string HAPPINESS_WORLD = "Happiness Affects World";
        public const string HAPPINESS_UI = "Happiness Affects UI";
        public const string HAPPINESS_CURSOR = "Happiness Affects Cursor";
        

        public SettingsManager()
        {
            this.ValueExtractor = new JSONValueExtractor();
            
            this.Settings = this.Load().ToDictionary(setting => setting.Name, setting => setting);
            
            this.MakeDefaults();
        }

        protected void MakeDefaults()
        {
            this.Add(new Setting<bool>(DYSLEXIC_MODE, new List<bool> { false, true }));
            this.Add(new Setting<bool>(HAPPINESS_WORLD, new List<bool> { true, false }));
            this.Add(new Setting<bool>(HAPPINESS_UI, new List<bool> { true, false }));
            this.Add(new Setting<bool>(HAPPINESS_CURSOR, new List<bool> { true, false }));
        }

        public ISetting Get(string name)
        {
            return this.Settings.TryGetValue(name, out ISetting setting) ? setting : null;
        }

        public bool ChangeSetting(string name, int index)
        {
            if (!this.Settings.TryGetValue(name, out ISetting setting))
            {
                return false;
            }
            
            setting.Index = index;
            return true;
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

            string[] files = Directory.GetFiles(
                this.SettingsDirectory,
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
                    ICollection<object> values = this.ValueExtractor.GetArrayValuesCollectionFromDictionary<object>(settingsDict, "ValuesRange");
                    int index = this.ValueExtractor.GetValueFromDictionary<int>(settingsDict, "Index");
                    ISetting setting = SettingsFactory.Create(name, values);
                    setting.Index = index;
                    settings.Add(setting);
                }
            }

            return settings;
        }

        public void Save()
        {
            Dictionary saveDict = new Dictionary();
            Array saveArray = new Array();

            foreach (ISetting setting in this.Values)
            {
                saveArray.Add(setting.Save());
            }
            
            saveDict.Add("Settings", saveArray);
            
            File.WriteAllText(this.SettingsDirectory + "/Settings.json", JSON.Print(saveDict, "\t"));
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