namespace JoyGodot.Assets.Scripts.GUI.Inventory_System
{
    public class JoyCraftingSlot : JoyConstrainedSlot
    {
        public int AmountRequired { get; set; }

        public float AmountInSlot
        {
            get
            {
                if (this.m_Item is null)
                {
                    return 0;
                }

                if (this.m_Item.HasTag(this.Slot))
                {
                    return this.m_Item.ItemType.Size;
                }

                return 0;
            }
        }

        public bool SufficientMaterial => this.AmountInSlot >= this.AmountRequired;
    }
}