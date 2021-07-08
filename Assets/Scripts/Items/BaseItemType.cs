using System;
using System.Collections.Generic;
using System.Linq;
using JoyGodot.Assets.Scripts.Entities.Abilities;

namespace JoyGodot.Assets.Scripts.Items
{
    [Serializable]
    public class BaseItemType
    {
         
        protected List<string> m_Tags;
        
        public BaseItemType()
        {}

        public BaseItemType(
            IEnumerable<string> tags, 
            string description, 
            string unidentifiedDescriptionRef, 
            string unidentifiedNameRef, 
            string identifiedNameRef, 
            IEnumerable<string> slotsRef, 
            float size, 
            IItemMaterial material, 
            IEnumerable<string> governingSkills, 
            string actionStringRef, 
            int valueRef, 
            int spawnRef, 
            string spriteSheet,
            int range = 1,
            int lightLevel = 0,
            IEnumerable<IAbility> abilities = null)
        {
            this.m_Tags = tags.ToList();
            
            this.SpawnWeighting = spawnRef;
            this.Value = valueRef;

            this.IdentifiedName = identifiedNameRef;
            this.Description = description;
            this.UnidentifiedDescription = unidentifiedDescriptionRef;
            this.UnidentifiedName = unidentifiedNameRef;
            this.Size = size;
            this.Material = material;
            this.Weight = this.Size * this.Material.Density;
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
            if(this.m_Tags.Contains(tag) == true)
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
            return (int)(Math.Max(1, this.Size * this.Material.Hardness));
        }

         
        public string Description
        {
            get;
            protected set;
        }

         
        public string IdentifiedName
        {
            get;
            protected set;
        }

         
        public string UnidentifiedDescription
        {
            get;
            protected set;
        }

         
        public string UnidentifiedName
        {
            get;
            protected set;
        }

         
        public IEnumerable<IAbility> Abilities
        {
            get;
            protected set;
        }

         
        public float Weight
        {
            get;
            protected set;
        }

         
        public float Size
        {
            get;
            protected set;
        }

         
        public IItemMaterial Material
        {
            get;
            protected set;
        }

         
        public IEnumerable<string> Slots
        {
            get;
            protected set;
        }

        public int BaseProtection
        {
            get
            {
                return this.Material.Bonus;
            }
        }

        public int BaseEfficiency
        {
            get
            {
                return this.Material.Bonus;
            }
        }

         
        public IEnumerable<string> GoverningSkills
        {
            get;
            protected set;
        }

        public string MaterialDescription
        {
            get
            {
                return "Made of " + this.Material.Name;
            }
        }

         
        public string ActionString
        {
            get;
            protected set;
        }

         
        public int Value
        {
            get;
            protected set;
        }

         
        public int SpawnWeighting
        {
            get;
            protected set;
        }

         
        public int OwnerGUID
        {
            get;
            set;
        }

         
        public int LightLevel
        {
            get;
            protected set;
        }

        public string[] Tags
        {
            get
            {
                return this.m_Tags.ToArray();
            }
        }

         
        public string SpriteSheet
        {
            get;
            protected set;
        }
        
         
        public int Range { get; protected set; }
    }
}
