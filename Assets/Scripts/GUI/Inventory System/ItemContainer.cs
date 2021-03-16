using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Godot;
using JoyLib.Code.Entities.Items;
using JoyLib.Code.Events;
using JoyLib.Code.Unity.GUI;
using Array = Godot.Collections.Array;

namespace JoyLib.Code.Unity
{
    public class ItemContainer : GUIData
    {
        [Export] protected string m_UseAction;
        [Export] protected BoxContainer m_SlotParent;
        [Export] protected JoyItemSlot m_SlotPrefab;

        [Export] protected bool m_CanDrag = false;
        public bool CanDrag => this.m_CanDrag;

        [Export] protected bool m_CanDropItems = false;
        public bool CanDropItems => this.m_CanDropItems;

        [Export] protected bool m_CanUseItems = false;
        public bool CanUseItems => this.m_CanUseItems;

        [Export] protected bool m_UseContextMenu = false;
        public bool UseContextMenu => this.m_UseContextMenu;

        [Export] protected bool m_ShowTooltips = false;
        public bool ShowTooltips => this.m_ShowTooltips;

        [Export] protected bool m_MoveUsedItem = false;

        [Export] protected List<MoveContainerPriority> m_ContainerNames;
        protected List<ItemContainer> MoveToContainers { get; set; }
        public bool MoveUsedItem => this.m_MoveUsedItem;

        protected List<JoyItemSlot> Slots { get; set; }

        protected IJoyObject m_Owner;

        public List<MoveContainerPriority> ContainerPriorities => this.m_ContainerNames;
        
        public IGUIManager GUIManager { get; set; }

        public IJoyObject Owner
        {
            get => this.m_Owner;
            set
            {
                this.m_Owner = value;
                this.OnEnable();
            }
        }

        public string UseAction => this.m_UseAction;

        public IEnumerable<IItemInstance> Contents
        {
            get
            {
                if (this.Owner is IItemContainer container)
                {
                    return container.Contents;
                }

                return new List<IItemInstance>();
            }
        }

        protected int ComparePriorities(Tuple<string, int> left, Tuple<string, int> right)
        {
            if (left.Item2 > right.Item2)
            {
                return 1;
            }

            if (left.Item2 < right.Item2)
            {
                return -1;
            }

            return 0;
        }

        public virtual void OnEnable()
        {
            /*
            if (GlobalConstants.GameManager is null)
            {
                return;
            }

            this.GUIManager = GlobalConstants.GameManager.GUIManager;
            */
            if (this.Slots is null)
            {
                this.Slots = new List<JoyItemSlot>();
                Array children = this.GetChildren();
                foreach (var child in children)
                {
                    if (child is JoyItemSlot slot)
                    {
                        this.Slots.Add(slot);
                    }
                }
            }

            this.MoveToContainers = new List<ItemContainer>();
            if (this.m_ContainerNames is null)
            {
                this.m_ContainerNames = new List<MoveContainerPriority>();
            }
            else
            {
                Node root = this.GetTree().Root.FindNode("MainUI");
                foreach (MoveContainerPriority priority in this.m_ContainerNames)
                {
                    this.MoveToContainers.Add((ItemContainer) root.FindNode(priority.m_ContainerName));
                }
            }

            if (this.Owner is null)
            {
                this.Owner = new VirtualStorage();
            }

            if (this.Owner is IItemContainer container)
            {
                foreach (JoyItemSlot slot in this.Slots)
                {
                    slot.OnEnable();
                    slot.Container = this;
                    slot.Item = null;
                }

                if (this.Slots.Count < container.Contents.Count())
                {
                    for (int i = this.Slots.Count; i < container.Contents.Count(); i++)
                    {
                        this.Slots.Add((JoyItemSlot) this.m_SlotPrefab.Duplicate());
                    }
                }

                List<IItemInstance> contents = container.Contents.ToList();
                foreach (IItemInstance item in contents)
                {
                    this.StackOrAdd(item);
                }

                foreach (JoyItemSlot slot in this.Slots.Where(slot => slot.Visible))
                {
                    slot.Repaint();
                }
            }
        }

        public void OnDisable()
        {
            this.GUIManager?.CloseGUI(GUINames.TOOLTIP);
            this.GUIManager?.CloseGUI(GUINames.CONTEXT_MENU);
        }

        public virtual void RemoveAllItems()
        {
            if (this.Slots is null)
            {
                return;
            }

            foreach (JoyItemSlot slot in this.Slots)
            {
                slot.Item = null;
                slot.Repaint();
            }
        }

        public virtual bool MoveItem(IItemInstance item)
        {
            var sorted = (from priority in this.m_ContainerNames
                orderby priority.m_Priority descending
                select priority);

            if (this.MoveToContainers.Count == 0)
            {
                return false;
            }
            
            ItemContainer target = this.MoveToContainers.FirstOrDefault(container => sorted.Any(sort =>
                sort.m_ContainerName.Equals(container.Name, StringComparison.OrdinalIgnoreCase)
                && ((sort.m_RequiresVisibility && container.Visible)
                || sort.m_RequiresVisibility == false)));
                
            if (target is null || item is null)
            {
                return false;
            }

            return this.StackOrSwap(target, item);
        }

        public virtual List<JoyItemSlot> GetRequiredSlots(IItemInstance item, bool takeFilledSlots = false)
        {
            List<JoyItemSlot> slots = new List<JoyItemSlot>();
            if (item == null)
            {
                return slots;
            }

            Dictionary<string, int> requiredSlots = new Dictionary<string, int>();

            foreach (string slot in item.ItemType.Slots)
            {
                if (requiredSlots.ContainsKey(slot))
                {
                    requiredSlots[slot] += 1;
                }
                else
                {
                    requiredSlots.Add(slot, 1);
                }
            }

            Dictionary<string, int> copySlots = new Dictionary<string, int>(requiredSlots);

            for (int i = 0; i < this.Slots.Count; i++)
            {
                if (this.Slots[i].m_Slot.IsNullOrEmpty() == false)
                {
                    foreach (KeyValuePair<string, int> pair in requiredSlots)
                    {
                        if (pair.Key.Equals(this.Slots[i].m_Slot, StringComparison.OrdinalIgnoreCase))
                        {
                            if (takeFilledSlots == false 
                                && this.Slots[i].IsEmpty
                                && copySlots[pair.Key] > 0)
                            {
                                copySlots[pair.Key] -= 1;
                                slots.Add(this.Slots[i]);
                            }
                            else if (takeFilledSlots == true
                                     && copySlots[pair.Key] > 0)
                            {
                                copySlots[pair.Key] -= 1;
                                slots.Add(this.Slots[i]);
                            }
                        }
                    }
                }
            }

            return slots;
        }

        public virtual bool AddSlot(JoyItemSlot slot, bool pool = true)
        {
            if (pool)
            {
                if (this.Slots.Any(itemSlot => itemSlot.Visible == false))
                {
                    JoyItemSlot poolSlot = this.Slots.FirstOrDefault(itemSlot => itemSlot.Visible == false);
                    if (poolSlot is null)
                    {
                        poolSlot = slot;
                    }
                    poolSlot.Visible = true;
                    poolSlot.Item = slot.Item;
                    return true;
                }

                return false;
            }
            else
            {
                this.m_SlotParent.AddChild(slot);
                this.Slots.Add(slot);
                return true;
            }
        }

        public virtual bool RemoveSlot(JoyItemSlot slot, bool pool = true)
        {
            if (this.Slots.Any(itemSlot => itemSlot == slot))
            {
                if (pool)
                {
                    JoyItemSlot foundSlot = this.Slots.First(itemSlot => itemSlot == slot);
                    foundSlot.Visible = false;
                    return true;
                }
                else
                {
                    this.Slots.Remove(slot);
                    slot.QueueFree();
                    return true;
                }
            }

            return false;
        }

        public virtual bool RemoveAllSlots(bool pool = true)
        {
            if (pool)
            {
                this.Slots.ForEach(slot => slot.Visible = false);
                return true;
            }

            this.Slots.ForEach(slot => slot.QueueFree());
            return true;
        }

        public virtual bool CanAddItem(IItemInstance item)
        {
            if (this.Owner is IItemContainer container)
            {
                return container.CanAddContents(item);
            }

            return false;
        }

        public virtual bool StackOrAdd(JoyItemSlot slot, IItemInstance item)
        {
            return this.StackOrAdd(new[] {slot}, item);
        }

        protected virtual bool StackOrAdd(IEnumerable<JoyItemSlot> slots, IItemInstance item)
        {
            if (this.Owner is null)
            {
                return false;
            }

            if (item is ItemInstance instance && instance.Guid != this.Owner.Guid)
            {
                if (this.Owner is IItemContainer container)
                {
                    if (container.CanAddContents(instance) | container.Contains(instance))
                    {
                        container.AddContents(instance);
                        IEnumerable<JoyItemSlot> joyItemSlots = slots.ToList();
                        if (joyItemSlots.Any())
                        {
                            foreach (JoyItemSlot slot in joyItemSlots)
                            {
                                slot.Item = instance;
                            }
                        }
                        else
                        {
                            this.Slots.First(slot => slot.IsEmpty).Item = instance;
                        }
                        this.OnAddItem?.Invoke(container, new ItemChangedEventArgs() {Item = item});
                        return true;
                    }
                }

                return false;
            }

            return false;
        }

        public virtual bool StackOrAdd(IItemInstance item)
        {
            List<JoyItemSlot> slots = null;

            if (item.ItemType.Slots.Any() == false)
            {
                if (this.Slots.Any(slot => slot.IsEmpty && slot.m_Slot.IsNullOrEmpty()))
                {
                    slots = new List<JoyItemSlot> { this.Slots.First(slot => slot.IsEmpty && slot.m_Slot.IsNullOrEmpty()) };
                }
                else
                {
                    return false;
                }
            }
            else
            {
                slots = this.GetRequiredSlots(item);
            }

            return this.StackOrAdd(slots, item);
        }

        public virtual bool RemoveItem(IItemInstance item, int amount)
        {
            if (this.Owner is null)
            {
                return false;
            }

            bool result = false;
            if (this.Owner is IItemContainer container)
            {
                if (container.Contents.Any(i =>
                    i.IdentifiedName.Equals(item.IdentifiedName, StringComparison.OrdinalIgnoreCase)))
                {
                    List<IItemInstance> matches = container.Contents.Where(itemInstance =>
                        itemInstance.IdentifiedName.Equals(item.IdentifiedName,
                            StringComparison.OrdinalIgnoreCase)).ToList();

                    for (int i = 0; i < amount; i++)
                    {
                        result |= container.RemoveContents(matches[i]);
                        result |= this.RemoveItem(matches[i]);
                    }

                    if (result)
                    {
                        this.OnRemoveItem?.Invoke(container, new ItemChangedEventArgs {Item = item});
                    }
                }
            }

            return result;
        }

        public virtual bool RemoveItem(IItemInstance item)
        {
            if (this.Owner is null)
            {
                return false;
            }

            if (item.Guid != this.Owner.Guid)
            {
                if (this.Owner is IItemContainer container)
                {
                    if (container.Contains(item) == false ||
                        this.Slots.Any(slot => !(slot.Item is null) && slot.Item.Guid == item.Guid) == false)
                    {
                        return false;
                    }

                    container.RemoveContents(item);
                    foreach (JoyItemSlot joyItemSlot in this.Slots.Where(slot => !(slot.Item is null) && item.Equals(slot.Item)))
                    {
                        joyItemSlot.Item = null;
                    }
                    this.OnRemoveItem?.Invoke(container, new ItemChangedEventArgs() {Item = item});
                }

                return true;
            }

            return false;
        }

        public virtual bool RemoveItem(int index)
        {
            if (this.Owner is null)
            {
                return false;
            }

            if (index < this.Slots.Count
                && this.Owner is IItemContainer container)
            {
                JoyItemSlot slot = this.Slots[index];
                IItemInstance item = slot.Item;
                if (item is null)
                {
                    return false;
                }

                container.RemoveContents(item);
                slot.Item = null;
                this.OnRemoveItem?.Invoke(container, new ItemChangedEventArgs() {Item = item});
            }

            return true;
        }

        public virtual bool StackOrSwap(ItemContainer destination, IItemInstance item)
        {
            if (this.Owner.Equals(destination.Owner))
            {
                return false;
            }
            
            if (destination.CanAddItem(item) == false)
            {
                IEnumerable<JoyItemSlot> filledSlots = destination.GetRequiredSlots(item, true);
                bool result = true;
                foreach (JoyItemSlot slot in filledSlots)
                {
                    if (slot.Item is null == false && this.CanAddItem(slot.Item))
                    {
                        result &= this.StackOrAdd(slot.Item);
                        result &= slot.Container.RemoveItem(slot.Item);
                    }

                    if (!result)
                    {
                        break;
                    }
                }

                if (!result || !destination.CanAddItem(item))
                {
                    return false;
                }
                if (this.RemoveItem(item) == false)
                {
                    return false;
                }
                destination.StackOrAdd(item);
                return true;
            }

            return this.RemoveItem(item) && destination.StackOrAdd(item);
        }

        public virtual event ItemAddedEventHandler OnAddItem;
        public virtual event ItemRemovedEventHandler OnRemoveItem;
    }

    [Serializable]
    public class MoveContainerPriority
    {
        public string m_ContainerName;
        public int m_Priority;
        public bool m_RequiresVisibility;
    }
}