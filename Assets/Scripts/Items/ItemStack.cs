using System;
using System.Collections.Generic;
using System.Linq;
using JoyGodot.Assets.Scripts.Events;
using JoyGodot.Assets.Scripts.Managed_Assets;

namespace JoyGodot.Assets.Scripts.Items
{
    public class ItemStack : IItemContainer
    {
        public string JoyName => this.Contents.FirstOrDefault()?.JoyName;
        public Guid Guid => Guid.Empty;
        public IEnumerable<IItemInstance> Contents => this.m_Contents;

        protected List<IItemInstance> m_Contents;
        protected Guid ItemTypeGuid { get; set; }

        public ISpriteState DisplayState => this.m_Contents.FirstOrDefault()?.States.FirstOrDefault();

        public bool Empty => this.m_Contents.Any() == false;

        public ItemStack()
        {
            this.m_Contents = new List<IItemInstance>();
            this.ItemTypeGuid = Guid.Empty;
        }

        public ItemStack(IEnumerable<IItemInstance> contents)
            : this()
        {
            if (contents.Any() == false)
            {
                return;
            }
            
            IItemInstance last = null;
            foreach (IItemInstance item in contents)
            {
                if (last is null == false
                    && item.ItemType.Guid != last.ItemType.Guid)
                {
                    return;
                }

                last = item;
            }
            
            this.m_Contents.AddRange(contents);
            this.ItemTypeGuid = contents.First().Guid;
        }
        
        public bool Contains(IItemInstance actor)
        {
            return this.m_Contents.Contains(actor);
        }

        public bool CanAddContents(IItemInstance actor)
        {
            return this.ItemTypeGuid == Guid.Empty 
                || this.ItemTypeGuid == actor.ItemType.Guid;
        }

        public bool CanAddContents(IEnumerable<IItemInstance> actors)
        {
            return actors.Aggregate(true, (agg, item) => this.CanAddContents(item));
        }

        public bool AddContents(IItemInstance actor)
        {
            if (this.CanAddContents(actor) == false)
            {
                return false;
            }

            this.m_Contents.Add(actor);
            this.ItemTypeGuid = actor.ItemType.Guid;
            return true;
        }

        public bool AddContents(IEnumerable<IItemInstance> actors)
        {
            return actors.Aggregate(true, (agg, item) => this.AddContents(item));
        }

        public bool CanRemoveContents(IItemInstance actor)
        {
            return this.Contains(actor);
        }

        public bool CanRemoveContents(IEnumerable<IItemInstance> actors)
        {
            return actors.Aggregate(true, (agg, item) => this.CanRemoveContents(item));
        }

        public bool RemoveContents(IItemInstance actor)
        {
            return this.m_Contents.Remove(actor);
        }

        public bool RemoveContents(IEnumerable<IItemInstance> actors)
        {
            return actors.Aggregate(true, (agg, item) => this.RemoveContents(item));
        }

        public void Clear()
        {
            this.m_Contents.Clear();
            this.ItemTypeGuid = Guid.Empty;
        }

        public string ContentString
        {
            get
            {
                if (this.m_Contents.Any() == false)
                {
                    return "Empty";
                }

                return "Contains " + this.m_Contents.Count + " " + this.m_Contents.First().DisplayName;
            }
        }
        
        public event ItemRemovedEventHandler ItemRemoved;
        public event ItemAddedEventHandler ItemAdded;
    }
}