using System;
using JoyGodot.Assets.Scripts.Entities.Items;

namespace JoyGodot.Assets.Scripts.Exceptions
{
    public class ItemCreationException : Exception
    {
        public BaseItemType ItemType;

        public ItemCreationException(BaseItemType itemType, string message) :
            base(message)
        {
            this.ItemType = itemType;
        }
    }
}