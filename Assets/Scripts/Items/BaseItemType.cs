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
        
        public float Size { get; protected set; }

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

        public string ActionString { get; protected set; }

        public int Value
        {
            get
            {
                return this.Materials.Select(pair => (int) (pair.Item1.ValueMod * pair.Item2)).Sum();
            }
        }
        
        public int SpawnWeighting { get; protected set; }

         
        public int LightLevel { get; protected set; }

        public string[] Tags => this.m_Tags.ToArray();
        
        public string SpriteSheet
        { get; protected set; }
        
         
        public int Range { get; protected set; }
        
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
    }
}
