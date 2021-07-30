using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using Godot.Collections;
using JoyGodot.Assets.Scripts.Collections;
using JoyGodot.Assets.Scripts.Entities.Abilities;
using JoyGodot.Assets.Scripts.Graphics;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.Items.Crafting;
using JoyGodot.Assets.Scripts.Rollers;
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

        protected ICraftingRecipeHandler CraftingRecipeHandler { get; set; }

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
                        if (this.m_ItemWeights.ContainsKey(type.IdentifiedName) == false)
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
            ICraftingRecipeHandler craftingRecipeHandler,
            IRollable roller = null)
        {
            this.ValueExtractor = new JSONValueExtractor();
            this.ObjectIcons = objectIconHandler;
            this.MaterialHandler = materialHandler;
            this.AbilityHandler = abilityHandler;
            this.CraftingRecipeHandler = craftingRecipeHandler;
            this.Roller = roller ?? new RNG();

            this.m_ItemDatabase = new List<BaseItemType>(this.LoadComponents());
            this.m_ItemDatabase.AddRange(this.Load());
        }

        protected IEnumerable<BaseItemType> LoadComponents()
        {
            string[] files = Directory.GetFiles(
                Directory.GetCurrentDirectory() +
                GlobalConstants.ASSETS_FOLDER +
                GlobalConstants.DATA_FOLDER +
                "Items/Components",
                "*.json",
                SearchOption.AllDirectories);

            return this.GetTypesFromJson(files);
        }

        public IEnumerable<BaseItemType> Load()
        {
            string[] files = Directory.GetFiles(
                Directory.GetCurrentDirectory() +
                GlobalConstants.ASSETS_FOLDER +
                GlobalConstants.DATA_FOLDER +
                "Items/Manufactured",
                "*.json",
                SearchOption.AllDirectories);

            return this.GetTypesFromJson(files);
        }

        protected IEnumerable<BaseItemType> GetTypesFromJson(IEnumerable<string> files)
        {
            List<BaseItemType> items = new List<BaseItemType>();

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

                    IEnumerable<IEnumerable<BaseItemType>> components = componentNames.Select(this.GetAllForName);

                    int size = this.ValueExtractor.GetValueFromDictionary<int>(identifiedToken, "Size");
                    int lightLevel = identifiedToken.Contains("LightLevel")
                        ? this.ValueExtractor.GetValueFromDictionary<int>(identifiedToken, "LightLevel")
                        : 0;
                    int spawnWeight = identifiedToken.Contains("SpawnWeight")
                        ? this.ValueExtractor.GetValueFromDictionary<int>(identifiedToken, "SpawnWeight")
                        : 0;
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

                string actionWord = itemData.Contains("ActionWord")
                    ? this.ValueExtractor.GetValueFromDictionary<string>(itemData, "ActionWord")
                    : "strikes";

                this.ObjectIcons.AddSpriteDataFromJson(itemData);

                foreach (IdentifiedItem identifiedItem in identifiedItems)
                {
                    var materialCombinations = this.GetMaterialPermutations(identifiedItem.materials);

                    var componentCombinations = identifiedItem.components.CartesianProduct();

                    NonUniqueDictionary<string, int> craftingMaterials = new NonUniqueDictionary<string, int>(
                        identifiedItem.materials.Select(pair =>
                            new Tuple<string, int>(pair.Key, pair.Value)));

                    Guid itemGuid = GlobalConstants.GameManager.GUIDManager.AssignGUID();
                    
                    foreach (var materials in materialCombinations)
                    {
                        foreach (var components in componentCombinations)
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

                        var materialDict = new NonUniqueDictionary<IItemMaterial, int>(materials);

                        var createdItem = new BaseItemType(
                            itemGuid, 
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
                            components,
                            abilities: identifiedItem.abilities);

                        items.Add(createdItem);

                        this.CraftingRecipeHandler.Add(
                            new CraftingRecipe(
                                craftingMaterials,
                                components,
                                new[] {createdItem}));
                        }
                    }
                }
            }

            return items;
        }

        protected IEnumerable<IEnumerable<Tuple<IItemMaterial, int>>> GetMaterialPermutations(
            IDictionary<string, int> materialNames)
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

        public IEnumerable<BaseItemType> GetMany(IEnumerable<string> keys)
        {
            return keys.Select(this.Get);
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
            this.ValueExtractor = null;
        }
    }
}