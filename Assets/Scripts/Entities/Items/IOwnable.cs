using System;

namespace JoyLib.Code.Entities.Items
{
    public interface IOwnable
    {
        string OwnerString { get; }
        
        Guid OwnerGUID { get; }

        void SetOwner(Guid newOwner, bool recursive = false);
    }
}