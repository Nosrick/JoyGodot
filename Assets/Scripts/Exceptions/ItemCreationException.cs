using System;
using JoyLib.Code.Entities.Items;

public class ItemCreationException : Exception
{
    public BaseItemType ItemType;

    public ItemCreationException(BaseItemType itemType, string message) :
        base(message)
    {
        this.ItemType = itemType;
    }
}