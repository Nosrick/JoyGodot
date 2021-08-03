using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Godot.Collections;
using JoyGodot.Assets.Scripts.Base_Interfaces;
using JoyGodot.Assets.Scripts.Collections;
using JoyGodot.Assets.Scripts.Entities.Abilities;
using Array = Godot.Collections.Array;

namespace JoyGodot.Assets.Scripts.Items
{
    public class BaseItemType : IGuidHolder, ISerialisationHandler
    {
        public Guid Guid { get; protected set; }

        protected List<string> m_Tags;

        public IEnumerable<BaseItemType> Components { get; protected set; }

        public string Description { get; protected set; }

        public string IdentifiedName { get; protected set; }

        public string UnidentifiedDescription { get; protected set; }

        public string UnidentifiedName { get; protected set; }

        public IEnumerable<IAbility> Abilities { get; protected set; }

        public float Weight
        {
            get
            {
                float total = this.Materials.Sum(pair => pair.Item1.Density * pair.Item2);
                total += this.Components.Select(type => type.Weight).Sum();

                return total;
            }
        }

        public float Size { get; protected set; }

        public NonUniqueDictionary<IItemMaterial, int> Materials
        {
            get
            {
                NonUniqueDictionary<IItemMaterial, int> materials =
                    new NonUniqueDictionary<IItemMaterial, int>(this.m_Materials);
                foreach (var pair in this.Components.SelectMany(type => type.Materials))
                {
                    materials.Add(pair.Item1, pair.Item2);
                }

                return materials;
            }
            protected set => this.m_Materials = new NonUniqueDictionary<IItemMaterial, int>(value);
        }

        public NonUniqueDictionary<IItemMaterial, int> MyMaterials => this.m_Materials;

        protected NonUniqueDictionary<IItemMaterial, int> m_Materials;

        public IEnumerable<string> MaterialNames => this.Materials.Keys.Select(material => material.Name).Distinct();

        public IEnumerable<string> Slots { get; protected set; }

        public int BaseProtection => (int) this.Materials.Average(material => material.Item1.Bonus);

        public int BaseEfficiency => (int) this.Materials.Average(material => material.Item1.Bonus);

        public IEnumerable<string> GoverningSkills { get; protected set; }

        public string MaterialDescription
        {
            get
            {
                StringBuilder builder = new StringBuilder("Made of ");
                List<string> materials = this.Materials
                    .Select(material => material.Item1.Name)
                    .Distinct()
                    .ToList();
                for (int i = 0; i < materials.Count; i++)
                {
                    builder.Append(materials[i]);
                    if (i != materials.Count - 1)
                    {
                        builder.Append(", ");
                    }
                }

                return builder.ToString();
            }
        }

        public string ActionString { get; protected set; }

        public int Value
        {
            get { return this.Materials.Select(pair => (int) (pair.Item1.ValueMod * pair.Item2)).Sum(); }
        }

        public int SpawnWeighting { get; protected set; }
        public int LightLevel { get; protected set; }
        public IEnumerable<string> Tags => this.m_Tags;
        public string SpriteSheet { get; protected set; }
        public int Range { get; protected set; }

        public BaseItemType()
        { }

        public BaseItemType(
            Guid guid,
            IEnumerable<string> tags,
            string description,
            string unidentifiedDescriptionRef,
            string unidentifiedNameRef,
            string identifiedNameRef,
            IEnumerable<string> slotsRef,
            float size,
            NonUniqueDictionary<IItemMaterial, int> materials,
            IEnumerable<string> governingSkills,
            string actionStringRef,
            int spawnRef,
            string spriteSheet,
            int range = 1,
            int lightLevel = 0,
            IEnumerable<BaseItemType> components = null,
            IEnumerable<IAbility> abilities = null)
        {
            this.Guid = guid;
            this.m_Tags = tags.ToList();

            this.SpawnWeighting = spawnRef;

            this.Components = components;
            this.IdentifiedName = identifiedNameRef;
            this.Description = description;
            this.UnidentifiedDescription = unidentifiedDescriptionRef;
            this.UnidentifiedName = unidentifiedNameRef;
            this.Size = size;
            this.Materials = new NonUniqueDictionary<IItemMaterial, int>(materials);
            this.Slots = slotsRef;
            this.GoverningSkills = governingSkills;
            this.ActionString = actionStringRef;
            this.LightLevel = lightLevel;
            this.SpriteSheet = spriteSheet;
            this.Abilities = abilities;
            this.Range = range;
        }

        public bool AddTag(string tag)
        {
            if (this.m_Tags.Contains(tag) == false)
            {
                this.m_Tags.Add(tag);
                return true;
            }

            return false;
        }

        public bool RemoveTag(string tag)
        {
            if (this.m_Tags.Contains(tag))
            {
                this.m_Tags.Remove(tag);
                return true;
            }

            return false;
        }

        public bool HasTag(string tag)
        {
            return this.m_Tags.Contains(tag);
        }

        public bool HasSlot(string slot)
        {
            return this.Slots.Contains(slot);
        }

        public int GetHitPoints()
        {
            int total = 0;
            if (this.Components.Any())
            {
                foreach (BaseItemType itemType in this.Components)
                {
                    total += itemType.GetHitPoints();
                }
            }
            else
            {
                foreach (var pair in this.Materials)
                {
                    total += (int) Math.Max(1, pair.Item1.Hardness * pair.Item2);
                }
            }

            return total;
        }

        public override bool Equals(object obj)
        {
            if (obj is BaseItemType other)
            {
                if (this.Guid != other.Guid)
                {
                    return false;
                }

                if (this.UnidentifiedName.Equals(other.UnidentifiedName) == false)
                {
                    return false;
                }

                if (this.Components.Equals(other.Components) == false)
                {
                    return false;
                }

                if (this.MyMaterials.Equals(other.MyMaterials) == false)
                {
                    return false;
                }

                if (this.Abilities.Equals(other.Abilities) == false)
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            int hashCode = 0;
            foreach (var material in this.MyMaterials)
            {
                hashCode += material.Item1.GetHashCode() * material.Item2;
            }

            foreach (var component in this.Components)
            {
                hashCode += component.GetHashCode() * 397;
            }

            return hashCode;
        }

        public Dictionary Save()
        {
            Dictionary saveDict = new Dictionary
            {
                {"Guid", this.Guid.ToString()},
                {"Tags", new Array(this.Tags)},
                {"Description", this.Description},
                {"IdentifiedName", this.IdentifiedName},
                {"UnidentifiedName", this.UnidentifiedName},
                {"UnidentifiedDescription", this.UnidentifiedDescription},
                {"Abilities", this.Abilities.Select(ability => ability.InternalName).ToArray()},
                {"Size", this.Size},
                {"Slots", new Array(this.Slots)},
                {"Skills", new Array(this.GoverningSkills)},
                {"ActionString", this.ActionString},
                {"SpawnWeight", this.SpawnWeighting},
                {"LightLevel", this.LightLevel},
                {"SpriteSheet", this.SpriteSheet},
                {"Range", this.Range}
            };

            Array materialArray = new Array();
            foreach (var tuple in this.MyMaterials.Collection)
            {
                materialArray.Add(new Dictionary
                {
                    {"Name", tuple.Item1.Name},
                    {"Value", tuple.Item2}
                });
            }

            saveDict.Add("Materials", materialArray);

            Array componentArray = new Array();
            foreach (BaseItemType itemType in this.Components)
            {
                componentArray.Add(itemType.Save());
            }
            
            saveDict.Add("Components", componentArray);

            return saveDict;
        }

        public void Load(Dictionary data)
        {
            var valueExtractor = GlobalConstants.GameManager.ItemHandler.ValueExtractor;

            this.Guid = new Guid(valueExtractor.GetValueFromDictionary<string>(data, "Guid"));
            this.m_Tags = valueExtractor.GetArrayValuesCollectionFromDictionary<string>(data, "Tags").ToList();

            List<BaseItemType> components = new List<BaseItemType>();
            foreach (Dictionary componentDict in
                valueExtractor.GetArrayValuesCollectionFromDictionary<Dictionary>(data, "Components"))
            {
                BaseItemType component = new BaseItemType();
                component.Load(componentDict);
                components.Add(component);
            }

            this.Components = components;

            this.m_Materials = new NonUniqueDictionary<IItemMaterial, int>();
            foreach (Dictionary materialDict in valueExtractor.GetArrayValuesCollectionFromDictionary<Dictionary>(
                data, "Materials"))
            {
                this.m_Materials.Add(
                    GlobalConstants.GameManager.MaterialHandler.Get(
                        valueExtractor.GetValueFromDictionary<string>(materialDict, "Name")),
                    valueExtractor.GetValueFromDictionary<int>(materialDict, "Value"));
            }

            this.Description = valueExtractor.GetValueFromDictionary<string>(data, "Description");
            this.IdentifiedName = valueExtractor.GetValueFromDictionary<string>(data, "IdentifiedName");
            this.UnidentifiedName = valueExtractor.GetValueFromDictionary<string>(data, "UnidentifiedName");
            this.UnidentifiedDescription =
                valueExtractor.GetValueFromDictionary<string>(data, "UnidentifiedDescription");
            this.Abilities = GlobalConstants.GameManager.AbilityHandler.GetMany(
                valueExtractor.GetArrayValuesCollectionFromDictionary<string>(data, "Abilities"));
            this.Size = valueExtractor.GetValueFromDictionary<int>(data, "Size");
            this.Slots = valueExtractor.GetArrayValuesCollectionFromDictionary<string>(data, "Slots");
            this.GoverningSkills = valueExtractor.GetArrayValuesCollectionFromDictionary<string>(data, "Skills");
            this.ActionString = valueExtractor.GetValueFromDictionary<string>(data, "ActionString");
            this.SpawnWeighting = valueExtractor.GetValueFromDictionary<int>(data, "SpawnWeight");
            this.LightLevel = valueExtractor.GetValueFromDictionary<int>(data, "LightLevel");
            this.SpriteSheet = valueExtractor.GetValueFromDictionary<string>(data, "SpriteSheet");
            this.Range = valueExtractor.GetValueFromDictionary<int>(data, "Range");
        }
    }
}