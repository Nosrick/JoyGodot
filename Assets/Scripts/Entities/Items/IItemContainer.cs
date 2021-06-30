using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Base_Interfaces;
using JoyGodot.Assets.Scripts.Events;

namespace JoyGodot.Assets.Scripts.Entities.Items
{
    public interface IItemContainer : IJoyNameHolder, IGuidHolder
    {
        IEnumerable<IItemInstance> Contents { get; }

        bool Contains(IItemInstance actor);

        bool CanAddContents(IItemInstance actor);

        bool AddContents(IItemInstance actor);

        bool AddContents(IEnumerable<IItemInstance> actors);

        bool RemoveContents(IItemInstance actor);
        bool RemoveContents(IEnumerable<IItemInstance> actors);

        void Clear();
        
        string ContentString { get; }

        event ItemRemovedEventHandler ItemRemoved;
        event ItemAddedEventHandler ItemAdded;
    }
}
