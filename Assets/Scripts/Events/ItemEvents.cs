using System;
using JoyGodot.Assets.Scripts.GUI.Inventory_System;
using JoyGodot.Assets.Scripts.Items;

namespace JoyGodot.Assets.Scripts.Events
{
    public delegate bool ItemRemovedEventHandler(IItemContainer sender, IItemInstance item);

    public delegate bool ItemAddedEventHandler(IItemContainer sender, IItemInstance item);

    public delegate bool ItemStackAddedEventHandler(ItemContainer sender, ItemStack itemStack);

    public delegate bool ItemStackRemovedEventHandler(ItemContainer sender, ItemStack itemStack);
}