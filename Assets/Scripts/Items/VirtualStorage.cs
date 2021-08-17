using System;
using System.Collections.Generic;
using System.Linq;
using JoyGodot.Assets.Scripts.Events;

namespace JoyGodot.Assets.Scripts.Items
{
    [Serializable]
    public class VirtualStorage : JoyObject.JoyObject, IItemContainer
    {
        protected List<IItemInstance> m_Contents;
        public IEnumerable<IItemInstance> Contents => this.m_Contents;

        public VirtualStorage()
        {
            this.m_Contents = new List<IItemInstance>();
        }
        
        public bool Contains(IItemInstance actor)
        {
            return this.Contents.Contains(actor);
        }

        public bool CanAddContents(IItemInstance actor)
        {
            if (actor is null)
            {
                return true;
            }
            
            return this.Guid != actor.Guid && !this.Contains(actor);
        }

        public bool AddContents(IItemInstance actor)
        {
            if (!this.CanAddContents(actor))
            {
                return false;
            }
            
            this.m_Contents.Add(actor);
                
            this.ItemAdded?.Invoke(this, actor);
            return true;

        }

        public bool AddContents(IEnumerable<IItemInstance> actors)
        {
            this.m_Contents.AddRange(actors.Where(actor => this.Contents.Any(item => item.Guid == actor.Guid) == false));
            
            foreach (IItemInstance actor in actors)
            {
                this.ItemAdded?.Invoke(this, actor);
            }

            return true;
        }

        public bool CanRemoveContents(IItemInstance actor)
        {
            return actor is null || this.Contains(actor);
        }

        public bool CanRemoveContents(IEnumerable<IItemInstance> actors)
        {
            return actors.Aggregate(true, (current, actor) => current & this.CanRemoveContents(actor));
        }

        public bool RemoveContents(IItemInstance actor)
        {
            if (actor is null)
            {
                return true;
            }
            
            if (!this.m_Contents.Remove(actor))
            {
                return false;
            }
            this.ItemRemoved?.Invoke(this, actor);

            return true;
        }

        public virtual bool RemoveContents(IEnumerable<IItemInstance> actors)
        {
            return actors.Aggregate(true, (current, actor) => current & this.RemoveContents(actor));
        }

        public void Clear()
        {
            List<IItemInstance> copy = new List<IItemInstance>(this.Contents);
            foreach (IItemInstance item in copy)
            {
                this.RemoveContents(item);
            }
        }

        public string ContentString { get; }
        public event ItemRemovedEventHandler ItemRemoved;
        public event ItemAddedEventHandler ItemAdded;
    }
}