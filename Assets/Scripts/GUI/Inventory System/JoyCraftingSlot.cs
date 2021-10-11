using System;
using System.Linq;

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
                if (this.m_ItemStack.Empty)
                {
                    return 0;
                }

                if (this.IngredientType.Equals("component", StringComparison.OrdinalIgnoreCase))
                {
                    if (this.m_ItemStack.Contents.FirstOrDefault()?.ItemType.UnidentifiedName
                        .Equals(this.Slot, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        return 1f;
                    }
                }
                else if (this.IngredientType.Equals("material", StringComparison.OrdinalIgnoreCase))
                {
                    if (this.m_ItemStack.Contents.FirstOrDefault()?.HasTag(this.Slot) == true
                        || this.m_ItemStack.Contents.FirstOrDefault()?.ItemType.MaterialNames.Any(
                            name => name.Equals(this.Slot, StringComparison.OrdinalIgnoreCase)) == true)
                    {
                        return this.m_ItemStack.Contents.Sum(item => item.ItemType.Size);
                    }
                }

                return 0;
            }
        }

        public bool SufficientMaterial => this.AmountInSlot >= this.AmountRequired;
    }
}