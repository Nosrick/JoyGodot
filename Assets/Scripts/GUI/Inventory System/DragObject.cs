using Godot;
using JoyGodot.Assets.Scripts.Items;

namespace JoyGodot.Assets.Scripts.GUI.Inventory_System
{
    public class DragObject : Resource
    {
        public ItemStack ItemStack { get; set; }
        public JoyItemSlot SourceSlot { get; set; }
        public ItemContainer SourceContainer { get; set; }
    }
}