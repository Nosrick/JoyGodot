using System;
using JoyGodot.Assets.Scripts.Items;

namespace JoyGodot.Assets.Scripts.Events
{
    public delegate void ItemRemovedEventHandler(IItemContainer sender, ItemChangedEventArgs args);

    public delegate void ItemAddedEventHandler(IItemContainer sender, ItemChangedEventArgs args);
    
    public class ItemChangedEventArgs : EventArgs
    {
        public IItemInstance Item { get; set; }
    }
}