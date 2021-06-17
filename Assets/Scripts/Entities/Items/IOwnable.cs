using System;

namespace JoyGodot.Assets.Scripts.Entities.Items
{
    public interface IOwnable
    {
        string OwnerString { get; }
        
        Guid OwnerGUID { get; }

        void SetOwner(Guid newOwner, bool recursive = false);
    }
}