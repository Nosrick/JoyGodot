using System;

namespace JoyGodot.Assets.Scripts.Items
{
    public interface IOwnable
    {
        string OwnerString { get; }
        
        Guid OwnerGUID { get; }

        void SetOwner(Guid newOwner, bool recursive = false);
    }
}