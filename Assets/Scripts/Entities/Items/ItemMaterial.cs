using System;

namespace JoyLib.Code.Entities.Items
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
        }

        public ItemMaterial(string nameRef, float hardnessRef, int bonusRef, float weightRef, float valueMod)
        {
            this.Name = nameRef;
            this.Hardness = hardnessRef;
            this.Bonus = bonusRef;
            this.Density = weightRef;
            this.ValueMod = valueMod;
        }

         
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
    }
}