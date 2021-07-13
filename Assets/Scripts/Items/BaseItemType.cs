using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JoyGodot.Assets.Scripts.Base_Interfaces;
using JoyGodot.Assets.Scripts.Collections;
using JoyGodot.Assets.Scripts.Entities.Abilities;
using JoyGodot.Assets.Scripts.Helpers;

namespace JoyGodot.Assets.Scripts.Items
{
    public class BaseItemType : IGuidHolder
    {
        public Guid Guid { get; }
        
        protected List<string> m_Tags;
        
        public IEnumerable<BaseItemType> Components { get; }
        
        public string Description { get; }
        
        public string IdentifiedName { get; }
        
        public string UnidentifiedDescription { get; }
        
        public string UnidentifiedName { get; }
        
        public IEnumerable<IAbility> Abilities { get; }

        public float Weight
        {
            get
            {
                float total = 0;
                if (this.Components.IsNullOrEmpty())
                {
                    foreach (var pair in this.Materials)
                    {
                        total += pair.Item1.Density * pair.Item2;
                    }
                }
                else
                {
                    total += this.Components.Select(type => type.Weight).Aggregate((run, tot) => tot + run);
                }

                return total;
            }
        }
        
        public float Size { get; }

        public NonUniqueDictionary<IItemMaterial, int> Materials
        {
            get
            {
                NonUniqueDictionary<IItemMaterial, int> materials = new NonUniqueDictionary<IItemMaterial, int>(this.m_Materials);
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

        public IEnumerable<string> Slots { get; }

        public int BaseProtection => (int) this.Materials.Average(material => material.Item1.Bonus);

        public int BaseEfficiency => (int) this.Materials.Average(material => material.Item1.Bonus);

        public IEnumerable<string> GoverningSkills { get; }

        public string MaterialDescription
        {
            get
            {
                StringBuilder builder = new StringBuilder("Made of ");
                List<string> materials = this.Materials
                    .Select(material => material.Item1.Name)
                    .Distinct()
                    .ToList();
                for(int i = 0; i < materials.Count; i++)
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

        public string ActionString { get; }

        public int Value
        {
            get
            {
                return this.Materials.Select(pair => (int) (pair.Item1.ValueMod * pair.Item2)).Sum();
            }
        }
        
        public int SpawnWeighting { get; }

         
        public int LightLevel { get; }

        public string[] Tags => this.m_Tags.ToArray();
        
        public string SpriteSheet { get; }
        
         
        public int Range { get; }
        
        public BaseItemType()
        {}

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
            if(this.m_Tags.Contains(tag) == false)
            {
                this.m_Tags.Add(tag);
                return true;
            }
            return false;
        }

        public bool RemoveTag(string tag)
        {
            if(this.m_Tags.Contains(tag))
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
    }
}
