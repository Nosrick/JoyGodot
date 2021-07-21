using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace JoyGodot.Assets.Scripts.Items
{
    [Serializable]
    public class ItemMaterial : IItemMaterial
    {
        public ItemMaterial()
        {
            this.Name = "DEFAULT MATERIAL";
            this.Hardness = 1.0f;
            this.Bonus = 0;
            this.Density = 1.0f;
            this.ValueMod = 1.0f;
            this.m_Tags = new List<string>();
            this.Colours = new[] {Colors.White};
        }

        public ItemMaterial(
            string nameRef, 
            float hardnessRef, 
            int bonusRef, 
            float weightRef, 
            float valueMod,
            IEnumerable<string> tags = null,
            IEnumerable<Color> colours = null)
        {
            this.Name = nameRef;
            this.Hardness = hardnessRef;
            this.Bonus = bonusRef;
            this.Density = weightRef;
            this.ValueMod = valueMod;
            this.m_Tags = tags?.ToList() ?? new List<string>();
            this.Colours = colours?.ToArray() ?? new[] {Colors.White};
        }

        public Color[] Colours { get; protected set; } 
        
        public string Name
        {
            get;
            protected set;
        }

         
        //Hardness will be multiplied by the item's size modifier to find its hit points
        public float Hardness
        {
            get;
            protected set;
        }

         
        //The bonus the material applies to any checks made with it
        public int Bonus
        {
            get;
            protected set;

        }

         
        //Density is how many grams per cm^3
        public float Density
        {
            get;
            protected set;
        }

         
        //The multiplier for the value of the item
        public float ValueMod
        {
            get;
            protected set;
        }

        public IEnumerable<string> Tags => this.m_Tags;

        protected List<string> m_Tags;
        public bool HasTag(string tag)
        {
            return this.m_Tags.Any(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase));
        }

        public bool HasTags(IEnumerable<string> tags)
        {
            return tags.All(this.HasTag);
        }

        public bool AddTag(string tag)
        {
            if (this.HasTag(tag))
            {
                return false;
            }

            this.m_Tags.Add(tag.ToLower());
            return true;
        }

        public bool RemoveTag(string tag)
        {
            return !this.HasTag(tag) && this.m_Tags.Remove(tag.ToLower());
        }
    }
}