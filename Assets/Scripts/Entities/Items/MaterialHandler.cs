using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using Godot.Collections;
using JoyGodot.Assets.Scripts.Helpers;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace JoyGodot.Assets.Scripts.Entities.Items
{
    public class MaterialHandler : IMaterialHandler
    {
        protected System.Collections.Generic.Dictionary<string, IItemMaterial> m_Materials;

        public JSONValueExtractor ValueExtractor { get; protected set; }

        public IEnumerable<IItemMaterial> Values => this.m_Materials.Values;

        public MaterialHandler()
        {
            this.ValueExtractor = new JSONValueExtractor();
            this.Initialise();
        }

        public void Initialise()
        {
            this.m_Materials = this.Load().ToDictionary(material => material.Name, material => material);

            this.m_Materials.Add("DEFAULT MATERIAL", 
                new ItemMaterial(
                    "DEFAULT MATERIAL", 
                    0.1f, 
                    0, 
                    1.0f, 
                    0.0f,
                    new []{"DEFAULT"}));
        }

        public IItemMaterial Get(string name)
        {
            if (this.m_Materials is null)
            {
                this.Initialise();
            }

            if (this.m_Materials.Any(pair => pair.Value.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                return this.m_Materials.First(pair => pair.Value.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    .Value;
            }

            return this.m_Materials["DEFAULT MATERIAL"];
        }

        public bool Add(IItemMaterial value)
        {
            if (this.m_Materials.ContainsKey(value.Name))
            {
                return false;
            }

            this.m_Materials.Add(value.Name, value);
            return true;
        }

        public bool Destroy(string key)
        {
            if (!this.m_Materials.ContainsKey(key))
            {
                return false;
            }

            this.m_Materials[key] = null;
            this.m_Materials.Remove(key);
            return true;
        }

        public IEnumerable<IItemMaterial> Load()
        {
            List<IItemMaterial> materials = new List<IItemMaterial>();

            string[] files = Directory.GetFiles(
                Directory.GetCurrentDirectory() +
                GlobalConstants.ASSETS_FOLDER +
                GlobalConstants.DATA_FOLDER +
                "Materials",
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

                if (!(result.Result is Dictionary dictionary))
                {
                    GlobalConstants.ActionLog.Log("Failed to parse JSON from " + file + " into a Dictionary.",
                        LogLevel.Warning);
                    continue;
                }

                ICollection<Dictionary> materialCollection =
                    this.ValueExtractor.GetArrayValuesCollectionFromDictionary<Dictionary>(dictionary, "Materials");

                foreach (Dictionary material in materialCollection)
                {
                    string name = this.ValueExtractor.GetValueFromDictionary<string>(material, "Name");
                    float hardness = material.Contains("Hardness")
                        ? this.ValueExtractor.GetValueFromDictionary<float>(material, "Hardness")
                        : 1f;
                    int bonus = material.Contains("Bonus")
                        ? this.ValueExtractor.GetValueFromDictionary<int>(material, "Bonus")
                        : 1;
                    float weight = material.Contains("Weight")
                        ? this.ValueExtractor.GetValueFromDictionary<float>(material, "Weight")
                        : 1f;
                    float value = material.Contains("Value")
                        ? this.ValueExtractor.GetValueFromDictionary<float>(material, "Value")
                        : 1f;

                    ICollection<string> tags = material.Contains("Tags")
                        ? this.ValueExtractor.GetArrayValuesCollectionFromDictionary<string>(material, "Tags")
                        : new List<string>();

                    materials.Add(
                        new ItemMaterial(
                            name,
                            hardness,
                            bonus,
                            weight,
                            value,
                            tags));
                }
            }

            return materials;
        }

        public void Dispose()
        {
            string[] keys = this.m_Materials.Keys.ToArray();
            foreach (string key in keys)
            {
                this.m_Materials[key] = null;
            }

            this.m_Materials = null;
        }
    }
}