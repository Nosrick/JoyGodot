using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using Godot.Collections;
using JoyGodot.Assets.Scripts.Entities.Abilities;
using JoyGodot.Assets.Scripts.Graphics;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.Rollers;
using NUnit.Framework;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace JoyGodot.Assets.Scripts.Items
{
    public class ItemDatabase : IItemDatabase
    {
        protected List<BaseItemType> m_ItemDatabase;

        protected IObjectIconHandler ObjectIcons { get; set; }

        protected IMaterialHandler MaterialHandler { get; set; }

        protected IAbilityHandler AbilityHandler { get; set; }

        public IEnumerable<BaseItemType> Values => this.m_ItemDatabase;

        public JSONValueExtractor ValueExtractor { get; protected set; }

        public IDictionary<string, int> ItemWeights
        {
            get
            {
                if (this.m_ItemWeights.IsNullOrEmpty())
                {
                    this.m_ItemWeights = new System.Collections.Generic.Dictionary<string, int>();
                    foreach (BaseItemType type in this.m_ItemDatabase)
                    {
                        if (this.m_ItemWeights.ContainsKey(type.IdentifiedName))
                        {
                            this.m_ItemWeights[type.IdentifiedName] += type.SpawnWeighting;
                        }
                        else
                        {
                            this.m_ItemWeights.Add(type.IdentifiedName, type.SpawnWeighting);
                        }
                    }
                }

                return this.m_ItemWeights;
            }
        }

        protected IDictionary<string, int> m_ItemWeights;

        protected IRollable Roller { get; set; }

        public ItemDatabase(
            IObjectIconHandler objectIconHandler,
            IMaterialHandler materialHandler,
            IAbilityHandler abilityHandler,
            IRollable roller = null)
        {
            this.ValueExtractor = new JSONValueExtractor();
            this.ObjectIcons = objectIconHandler;
            this.MaterialHandler = materialHandler;
            this.AbilityHandler = abilityHandler;
            this.Roller = roller ?? new RNG();

            this.m_ItemDatabase = this.LoadComponents();
            this.m_ItemDatabase.AddRange(this.Load());
        }

        protected List<BaseItemType> LoadComponents()
        {
            List<BaseItemType> items = new List<BaseItemType>();

            string[] files = Directory.GetFiles(
                Directory.GetCurrentDirectory() +
                GlobalConstants.ASSETS_FOLDER +
                GlobalConstants.DATA_FOLDER +
                "Items/Components",
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

                Dictionary itemData =
                    this.ValueExtractor.GetValueFromDictionary<Dictionary>(dictionary, "Items");

                List<IdentifiedItem> identifiedItems = new List<IdentifiedItem>();
                ICollection<Dictionary> identifiedItemsDicts =
                    this.ValueExtractor.GetArrayValuesCollectionFromDictionary<Dictionary>(itemData, "IdentifiedItems");

                foreach (Dictionary identifiedToken in identifiedItemsDicts)
                {
                    string name = this.ValueExtractor.GetValueFromDictionary<string>(identifiedToken, "Name");
                    string description = identifiedToken.Contains("Description")
                        ? this.ValueExtractor.GetValueFromDictionary<string>(identifiedToken, "Description")
                        : "";
                    int value = this.ValueExtractor.GetValueFromDictionary<int>(identifiedToken, "Value");

                    IEnumerable<Dictionary> materialsDict =
                        this.ValueExtractor.GetArrayValuesCollectionFromDictionary<Dictionary>(
                            identifiedToken,
                            "Materials");

                    IDictionary<string, int> materials = new System.Collections.Generic.Dictionary<string, int>();
                    foreach (Dictionary material in materialsDict)
                    {
                        materials.Add(
                            this.ValueExtractor.GetValueFromDictionary<string>(material, "Name"),
                            this.ValueExtractor.GetValueFromDictionary<int>(material, "Value"));
                    }

                    int size = this.ValueExtractor.GetValueFromDictionary<int>(identifiedToken, "Size");
                    int lightLevel = identifiedToken.Contains("LightLevel")
                        ? this.ValueExtractor.GetValueFromDictionary<int>(identifiedToken, "LightLevel")
                        : 0;
                    int spawnWeight = identifiedToken.Contains("SpawnWeight")
                        ? this.ValueExtractor.GetValueFromDictionary<int>(identifiedToken, "SpawnWeight")
                        : 1;
                    int range = identifiedToken.Contains("Range")
                        ? this.ValueExtractor.GetValueFromDictionary<int>(identifiedToken, "Range")
                        : 1;
                    IEnumerable<string> tags =
                        this.ValueExtractor.GetArrayValuesCollectionFromDictionary<string>(identifiedToken, "Tags");
                    string tileSet = this.ValueExtractor.GetValueFromDictionary<string>(identifiedToken, "TileSet");
                    IEnumerable<string> abilityNames = identifiedToken.Contains("Effects")
                        ? this.ValueExtractor.GetArrayValuesCollectionFromDictionary<string>(identifiedToken, "Effects")
                        : new List<string>();
                    IEnumerable<IAbility> abilities =
                        abilityNames.Select(abilityName => this.AbilityHandler.Get(abilityName));
                    IEnumerable<string> slots = identifiedToken.Contains("Slots")
                        ? this.ValueExtractor.GetArrayValuesCollectionFromDictionary<string>(identifiedToken, "Slots")
                        : new List<string>();
                    IEnumerable<string> skills = identifiedToken.Contains("Skill")
                        ? this.ValueExtractor.GetArrayValuesCollectionFromDictionary<string>(identifiedToken,
                            "Skill")
                        : new[] {"none"};

                    identifiedItems.Add(new IdentifiedItem(
                        name,
                        tags.ToArray(),
                        description,
                        value,
                        abilities.ToArray(),
                        spawnWeight,
                        skills,
                        materials,
                        size,
                        slots.ToArray(),
                        tileSet,
                        range,
                        new BaseItemType[0],
                        lightLevel));
                }

                List<UnidentifiedItem> unidentifiedItems = new List<UnidentifiedItem>();

                if (itemData.Contains("UnidentifiedItems"))
                {
                    ICollection<Dictionary> unidentifiedItemDicts =
                        this.ValueExtractor.GetArrayValuesCollectionFromDictionary<Dictionary>(
                            itemData,
                            "UnidentifiedItems");
                    foreach (Dictionary unidentifiedToken in unidentifiedItemDicts)
                    {
                        string name = this.ValueExtractor.GetValueFromDictionary<string>(unidentifiedToken, "Name");
                        string description = unidentifiedToken.Contains("Description")
                            ? this.ValueExtractor.GetValueFromDictionary<string>(unidentifiedToken, "Description")
                            : "";
                        string identifiedName =
                            this.ValueExtractor.GetValueFromDictionary<string>(unidentifiedToken, "IdentifiedName");

                        unidentifiedItems.Add(new UnidentifiedItem(name, description, identifiedName));
                    }
                }

                Dictionary tileSetDict = this.ValueExtractor.GetValueFromDictionary<Dictionary>(itemData, "TileSet");

                string tileSetName = tileSetDict.IsNullOrEmpty()
                    ? "DEFAULT"
                    : this.ValueExtractor.GetValueFromDictionary<string>(tileSetDict, "Name");

                string actionWord = itemData.Contains("ActionWord")
                    ? this.ValueExtractor.GetValueFromDictionary<string>(itemData, "ActionWord")
                    : "strikes";

                this.ObjectIcons.AddSpriteDataFromJson(itemData);

                foreach (IdentifiedItem identifiedItem in identifiedItems)
                {
                    var materialCombinations = this.GetPermutations(identifiedItem.materials);

                    foreach (var enumerable in materialCombinations)
                    {
                        UnidentifiedItem[] possibilities = unidentifiedItems.Where(unidentifiedItem =>
                                unidentifiedItem.identifiedName.Equals(identifiedItem.name,
                                    StringComparison.OrdinalIgnoreCase))
                            .ToArray();
                        UnidentifiedItem selectedItem;

                        var materialDict = enumerable.ToDictionary(
                            tuple => tuple.Item1,
                            tuple => tuple.Item2);
                        
                        if (possibilities.IsNullOrEmpty())
                        {
                            selectedItem = new UnidentifiedItem(
                                identifiedItem.name,
                                identifiedItem.description,
                                identifiedItem.name);
                        }
                        else
                        {
                            selectedItem = this.Roller.SelectFromCollection(possibilities);
                        }

                        items.Add(new BaseItemType(
                            identifiedItem.tags,
                            identifiedItem.description,
                            selectedItem.description,
                            selectedItem.name,
                            identifiedItem.name,
                            identifiedItem.slots,
                            identifiedItem.size,
                            materialDict,
                            identifiedItem.skills,
                            actionWord,
                            identifiedItem.weighting,
                            identifiedItem.spriteSheet,
                            identifiedItem.range,
                            identifiedItem.lightLevel,
                            identifiedItem.components,
                            identifiedItem.abilities));
                    }
                }
            }

            return items;
        }

        public IEnumerable<BaseItemType> Load()
        {
            List<BaseItemType> items = new List<BaseItemType>();

            string[] files = Directory.GetFiles(
                Directory.GetCurrentDirectory() +
                GlobalConstants.ASSETS_FOLDER +
                GlobalConstants.DATA_FOLDER +
                "Items/Manufactured",
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

                Dictionary itemData =
                    this.ValueExtractor.GetValueFromDictionary<Dictionary>(dictionary, "Items");

                List<IdentifiedItem> identifiedItems = new List<IdentifiedItem>();
                ICollection<Dictionary> identifiedItemsDicts =
                    this.ValueExtractor.GetArrayValuesCollectionFromDictionary<Dictionary>(itemData, "IdentifiedItems");

                foreach (Dictionary identifiedToken in identifiedItemsDicts)
                {
                    string name = this.ValueExtractor.GetValueFromDictionary<string>(identifiedToken, "Name");
                    string description = identifiedToken.Contains("Description")
                        ? this.ValueExtractor.GetValueFromDictionary<string>(identifiedToken, "Description")
                        : "";
                    int value = this.ValueExtractor.GetValueFromDictionary<int>(identifiedToken, "Value");
                    IEnumerable<Dictionary> materialsDict =
                        this.ValueExtractor.GetArrayValuesCollectionFromDictionary<Dictionary>(
                            identifiedToken,
                            "Materials");

                    IDictionary<string, int> materials = new System.Collections.Generic.Dictionary<string, int>();
                    foreach (Dictionary material in materialsDict)
                    {
                        materials.Add(
                            this.ValueExtractor.GetValueFromDictionary<string>(material, "Name"),
                            this.ValueExtractor.GetValueFromDictionary<int>(material, "Value"));
                    }

                    IEnumerable<string> componentNames =
                        this.ValueExtractor.GetArrayValuesCollectionFromDictionary<string>(
                            identifiedToken,
                            "Components");

                    IEnumerable<BaseItemType> components = componentNames.Select(this.Get);

                    int size = this.ValueExtractor.GetValueFromDictionary<int>(identifiedToken, "Size");
                    int lightLevel = identifiedToken.Contains("LightLevel")
                        ? this.ValueExtractor.GetValueFromDictionary<int>(identifiedToken, "LightLevel")
                        : 0;
                    int spawnWeight = identifiedToken.Contains("SpawnWeight")
                        ? this.ValueExtractor.GetValueFromDictionary<int>(identifiedToken, "SpawnWeight")
                        : 1;
                    int range = identifiedToken.Contains("Range")
                        ? this.ValueExtractor.GetValueFromDictionary<int>(identifiedToken, "Range")
                        : 1;
                    IEnumerable<string> tags =
                        this.ValueExtractor.GetArrayValuesCollectionFromDictionary<string>(identifiedToken, "Tags");
                    string tileSet = this.ValueExtractor.GetValueFromDictionary<string>(identifiedToken, "TileSet");
                    IEnumerable<string> abilityNames = identifiedToken.Contains("Effects")
                        ? this.ValueExtractor.GetArrayValuesCollectionFromDictionary<string>(identifiedToken, "Effects")
                        : new List<string>();
                    IEnumerable<IAbility> abilities =
                        abilityNames.Select(abilityName => this.AbilityHandler.Get(abilityName));
                    IEnumerable<string> slots = identifiedToken.Contains("Slots")
                        ? this.ValueExtractor.GetArrayValuesCollectionFromDictionary<string>(identifiedToken, "Slots")
                        : new List<string>();
                    IEnumerable<string> skills = identifiedToken.Contains("Skill")
                        ? this.ValueExtractor.GetArrayValuesCollectionFromDictionary<string>(identifiedToken,
                            "Skill")
                        : new[] {"none"};

                    identifiedItems.Add(new IdentifiedItem(
                        name,
                        tags.ToArray(),
                        description,
                        value,
                        abilities.ToArray(),
                        spawnWeight,
                        skills,
                        materials,
                        size,
                        slots.ToArray(),
                        tileSet,
                        range,
                        components,
                        lightLevel));
                }

                List<UnidentifiedItem> unidentifiedItems = new List<UnidentifiedItem>();

                if (itemData.Contains("UnidentifiedItems"))
                {
                    ICollection<Dictionary> unidentifiedItemDicts =
                        this.ValueExtractor.GetArrayValuesCollectionFromDictionary<Dictionary>(
                            itemData,
                            "UnidentifiedItems");
                    foreach (Dictionary unidentifiedToken in unidentifiedItemDicts)
                    {
                        string name = this.ValueExtractor.GetValueFromDictionary<string>(unidentifiedToken, "Name");
                        string description = unidentifiedToken.Contains("Description")
                            ? this.ValueExtractor.GetValueFromDictionary<string>(unidentifiedToken, "Description")
                            : "";
                        string identifiedName =
                            this.ValueExtractor.GetValueFromDictionary<string>(unidentifiedToken, "IdentifiedName");

                        unidentifiedItems.Add(new UnidentifiedItem(name, description, identifiedName));
                    }
                }

                string tileSetName = this.ValueExtractor.GetValueFromDictionary<string>(
                    this.ValueExtractor.GetValueFromDictionary<Dictionary>(itemData, "TileSet"),
                    "Name");

                string actionWord = itemData.Contains("ActionWord")
                    ? this.ValueExtractor.GetValueFromDictionary<string>(itemData, "ActionWord")
                    : "strikes";

                this.ObjectIcons.AddSpriteDataFromJson(itemData);
                
                foreach (IdentifiedItem identifiedItem in identifiedItems)
                {
                    var materialCombinations = this.GetPermutations(identifiedItem.materials);

                    foreach (var enumerable in materialCombinations)
                    {
                        UnidentifiedItem[] possibilities = unidentifiedItems
                            .Where(unidentifiedItem =>
                                unidentifiedItem.identifiedName.Equals(
                                    identifiedItem.name,
                                    StringComparison.OrdinalIgnoreCase))
                            .ToArray();

                        UnidentifiedItem selectedItem;
                        if (possibilities.IsNullOrEmpty())
                        {
                            selectedItem = new UnidentifiedItem(
                                identifiedItem.name,
                                identifiedItem.description,
                                identifiedItem.name);
                        }
                        else
                        {
                            selectedItem = possibilities.GetRandom();
                        }
                        
                        var materialDict = enumerable.ToDictionary(
                            tuple => tuple.Item1,
                            tuple => tuple.Item2);

                        items.Add(new BaseItemType(
                            identifiedItem.tags,
                            identifiedItem.description,
                            selectedItem.description,
                            selectedItem.name,
                            identifiedItem.name,
                            identifiedItem.slots,
                            identifiedItem.size,
                            materialDict,
                            identifiedItem.skills,
                            actionWord,
                            identifiedItem.weighting,
                            identifiedItem.spriteSheet,
                            range: identifiedItem.range,
                            lightLevel: identifiedItem.lightLevel,
                            components: identifiedItem.components,
                            abilities: identifiedItem.abilities));
                    }
                }
            }

            return items;
        }

        protected IEnumerable<IEnumerable<Tuple<IItemMaterial, int>>> GetPermutations(IDictionary<string, int> materialNames)
        {
            IDictionary<string, List<Tuple<IItemMaterial, int>>> replacements =
                new System.Collections.Generic.Dictionary<string, List<Tuple<IItemMaterial, int>>>();

            foreach (string name in materialNames.Keys)
            {
                replacements.Add(name, new List<Tuple<IItemMaterial, int>>());

                List<Tuple<IItemMaterial, int>> materials = GlobalConstants.GameManager.MaterialHandler
                    .GetPossibilities(name)
                    .Select(material => 
                        new Tuple<IItemMaterial, int>(
                            material,
                            materialNames[name]))
                    .ToList();

                replacements[name] = materials;
            }
            
            return replacements.Values.CartesianProduct();
        }

        public BaseItemType Get(string name)
        {
            return this.m_ItemDatabase.FirstOrDefault(type =>
                type.IdentifiedName.Equals(name, StringComparison.OrdinalIgnoreCase)
                || type.UnidentifiedName.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<BaseItemType> GetAllForName(string name)
        {
            return this.m_ItemDatabase.Where(type =>
                type.IdentifiedName.Equals(name, StringComparison.OrdinalIgnoreCase)
                || type.UnidentifiedName.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public bool Add(BaseItemType value)
        {
            if (this.m_ItemDatabase.Contains(value))
            {
                return false;
            }

            this.m_ItemDatabase.Add(value);
            return true;
        }

        public bool Destroy(string key)
        {
            var itemType = this.m_ItemDatabase.FirstOrDefault(type =>
                type.IdentifiedName.Equals(key, StringComparison.OrdinalIgnoreCase) ||
                type.UnidentifiedName.Equals(key, StringComparison.OrdinalIgnoreCase));

            return !(itemType is null) && this.m_ItemDatabase.Remove(itemType);
        }

        public IEnumerable<BaseItemType> FindItemsOfType(string[] tags, int tolerance = 1)
        {
            List<BaseItemType> matchingTypes = new List<BaseItemType>();
            foreach (BaseItemType itemType in this.m_ItemDatabase)
            {
                if (itemType.Tags.Intersect(tags).Count() >= tolerance)
                {
                    matchingTypes.Add(itemType);
                }
            }

            return matchingTypes.ToArray();
        }

        public void Dispose()
        {
            for (int i = 0; i < this.m_ItemDatabase.Count; i++)
            {
                this.m_ItemDatabase[i] = null;
            }

            this.m_ItemDatabase = null;
        }
    }
}