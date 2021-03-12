using System.Collections.Generic;
using JoyLib.Code.Events;

namespace JoyLib.Code.Entities.Items
{
    public interface IItemContainer
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
