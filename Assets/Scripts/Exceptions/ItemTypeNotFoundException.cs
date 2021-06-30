using System;

namespace JoyGodot.Assets.Scripts.Exceptions
{
    public class ItemTypeNotFoundException : Exception
    {
        public string m_ItemType;

        public ItemTypeNotFoundException(string itemType, string message) :
            base(message)
        {
            this.m_ItemType = itemType;
        }
    }
}