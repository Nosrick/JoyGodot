using System;

namespace JoyGodot.Assets.Scripts.GUI.Inventory_System
{
    public class JoyCraftingSlot : JoyConstrainedSlot
    {
        public string IngredientType { get; set; }

        public int AmountRequired { get; set; }

        public float AmountInSlot
        {
            get
            {
                if (this.m_Item is null)
                {
                    return 0;
                }

                if (this.IngredientType.Equals("component", StringComparison.OrdinalIgnoreCase))
                {
                    if (this.m_Item.ItemType.UnidentifiedName.Equals(this.Slot, StringComparison.OrdinalIgnoreCase))
                    {
                        return 1f;
                    }
                }
                else if (this.IngredientType.Equals("material", StringComparison.OrdinalIgnoreCase))
                {
                    if (this.m_Item.HasTag(this.Slot))
                    {
                        return this.m_Item.ItemType.Size;
                    }
                }

                return 0;
            }
        }

        public bool SufficientMaterial => this.AmountInSlot >= this.AmountRequired;
    }
}