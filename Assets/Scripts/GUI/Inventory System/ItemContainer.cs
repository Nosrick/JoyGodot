using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using JoyGodot.Assets.Scripts.Events;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.Items;
using Array = Godot.Collections.Array;

namespace JoyGodot.Assets.Scripts.GUI.Inventory_System
{
    public class ItemContainer : GUIData
    {
        public bool DynamicContainer => this.m_DynamicContainer;

        [Export] protected bool m_DynamicContainer;

        [Export] protected string m_UseAction;
        protected Container SlotParent { get; set; }
        protected PackedScene SlotPrefab { get; set; }

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

        [Export] protected Array<MoveContainerPriority> m_ContainerNames;
        protected List<ItemContainer> MoveToContainers { get; set; }
        public bool MoveUsedItem => this.m_MoveUsedItem;
        protected List<JoyItemSlot> Slots { get; set; }
        protected List<JoyItemSlot> ActiveSlots => this.Slots
            .Where(slot => slot.Visible)
            .ToList();

        protected List<JoyItemSlot> FilledSlots =>
            this.Slots.Where(slot => slot.Visible && slot.Item is null == false)
                .ToList();

        protected List<JoyItemSlot> EmptySlots => this.Slots
            .Where(slot => slot.Visible && slot.Item is null)
            .ToList();
        
        public Array<MoveContainerPriority> ContainerPriorities => this.m_ContainerNames;

        public virtual IItemContainer ContainerOwner
        {
            get => this.m_ContainerOwner;
            set
            {
                this.m_ContainerOwner = value;
                this.TitleText = value.JoyName;
            }
        }

        protected IItemContainer m_ContainerOwner;

        public string UseAction => this.m_UseAction;

        public IEnumerable<IItemInstance> Contents => this.ContainerOwner.Contents;

        public string TitleText
        {
            get => this.Title?.Text;
            set
            {
                if (this.Title is null)
                {
                    return;
                }

                this.Title.Text = value;
            }
        }

        protected Label Title { get; set; }

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

        public override void _Ready()
        {
            base._Ready();

            this.SlotParent = this.FindNode("Slot Grid") as Container;
            this.SlotPrefab =
                GD.Load<PackedScene>(GlobalConstants.GODOT_ASSETS_FOLDER + "Scenes/Parts/JoyItemSlot.tscn");
            this.Title = this.FindNode("Title") as Label;

            this.OnEnable();

            for (int i = 0; i < 10; i++)
            {
                this.AddSlot();
            }
        }

        public virtual void OnEnable()
        {
            if (GlobalConstants.GameManager is null)
            {
                return;
            }

            this.GUIManager = GlobalConstants.GameManager.GUIManager;
            if (this.Slots is null)
            {
                this.Slots = new List<JoyItemSlot>();
                Array children = this.SlotParent.GetChildren();
                foreach (var child in children)
                {
                    if (child is JoyItemSlot slot)
                    {
                        this.Slots.Add(slot);
                    }
                }
            }

            this.MoveToContainers = new List<ItemContainer>();
            this.m_ContainerNames ??= new Array<MoveContainerPriority>();
            this.ContainerOwner ??= new VirtualStorage();

            foreach (JoyItemSlot slot in this.Slots)
            {
                slot.Container = this;
                slot.Item = null;
            }

            for (int i = this.Slots.Count; i < this.ContainerOwner.Contents.Count(); i++)
            {
                var instance = this.AddSlot(true);
                this.GUIManager.SetupManagedComponents(instance);
                instance.Container = this;
            }

            List<IItemInstance> contents = this.ContainerOwner.Contents.ToList();
            foreach (IItemInstance item in contents)
            {
                this.StackOrAdd(item);
            }

            foreach (JoyItemSlot slot in this.ActiveSlots)
            {
                slot.Repaint();
            }
        }

        public virtual bool StackOrAdd(JoyItemSlot slot, IItemInstance item)
        {
            return this.StackOrAdd(new[] {slot}, item);
        }

        protected virtual bool StackOrAdd(IEnumerable<JoyItemSlot> slots, IItemInstance item)
        {
            if (this.ContainerOwner is null)
            {
                return false;
            }

            if (item.Guid != this.ContainerOwner.Guid)
            {
                if (this.ContainerOwner.CanAddContents(item))
                {
                    this.ContainerOwner.AddContents(item);
                    IEnumerable<JoyItemSlot> joyItemSlots = slots.ToList();
                    if (joyItemSlots.Any())
                    {
                        foreach (JoyItemSlot slot in joyItemSlots)
                        {
                            slot.Item = item;
                        }
                    }
                    else
                    {
                        var emptySlot = this.Slots.FirstOrDefault(slot => slot.IsEmpty);
                        if (emptySlot is null)
                        {
                            emptySlot = this.AddSlot(false);
                        }
                        emptySlot.Item = item;
                        emptySlot.Repaint();
                    }

                    this.OnAddItem?.Invoke(this.ContainerOwner, new ItemChangedEventArgs {Item = item});
                    return true;
                }

                if (this.ContainerOwner.Contains(item))
                {
                    IEnumerable<JoyItemSlot> joyItemSlots = slots.ToList();
                    if (joyItemSlots.Any())
                    {
                        foreach (JoyItemSlot slot in joyItemSlots)
                        {
                            slot.Item = item;
                        }
                    }
                    else
                    {
                        var emptySlot = this.Slots.FirstOrDefault(slot => slot.IsEmpty);
                        if (emptySlot is null)
                        {
                            emptySlot = this.AddSlot(false);
                        }
                        emptySlot.Item = item;
                        emptySlot.Repaint();
                    }

                    return false;
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
                if (this.EmptySlots.Any())
                {
                    slots = new List<JoyItemSlot> {this.EmptySlots.First()};
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

        public override void Display()
        {
            this.OnEnable();
            base.Display();
        }

        public void OnDisable()
        {
            this.GUIManager?.CloseGUI(this, GUINames.TOOLTIP);
            this.GUIManager?.CloseGUI(this, GUINames.CONTEXT_MENU);
        }

        public virtual void RemoveAllItems()
        {
            if (this.Slots is null)
            {
                return;
            }

            foreach (JoyItemSlot slot in this.FilledSlots)
            {
                slot.Item = null;
                slot.Repaint();
            }

            this.ContainerOwner.Clear();
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

            System.Collections.Generic.Dictionary<string, int> requiredSlots =
                new System.Collections.Generic.Dictionary<string, int>();

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

            System.Collections.Generic.Dictionary<string, int> copySlots =
                new System.Collections.Generic.Dictionary<string, int>(requiredSlots);

            List<JoyItemSlot> emptySlots = this.EmptySlots;
            for (int i = 0; i < emptySlots.Count; i++)
            {
                if (emptySlots[i] is JoyConstrainedSlot constrainedSlot
                    && constrainedSlot.Slot.IsNullOrEmpty() == false)
                {
                    foreach (KeyValuePair<string, int> pair in requiredSlots)
                    {
                        if (pair.Key.Equals(constrainedSlot.Slot, StringComparison.OrdinalIgnoreCase))
                        {
                            if (takeFilledSlots == false
                                && copySlots[pair.Key] > 0)
                            {
                                copySlots[pair.Key] -= 1;
                                slots.Add(emptySlots[i]);
                            }
                            else if (takeFilledSlots
                                     && copySlots[pair.Key] > 0)
                            {
                                copySlots[pair.Key] -= 1;
                                slots.Add(emptySlots[i]);
                            }
                        }
                    }
                }
            }

            return slots;
        }

        /// <summary>
        /// Create a brand new, empty slot
        /// </summary>
        /// <param name="pool">Reuse an old abandoned slot?</param>
        /// <param name="item">The item to be placed in the slot</param>
        /// <returns>The created or recycled slot</returns>
        public virtual JoyItemSlot AddSlot(bool pool, IItemInstance item = null)
        {
            if (!pool)
            {
                return this.AddSlot();
            }
            
            JoyItemSlot poolSlot = this.Slots.FirstOrDefault(itemSlot => itemSlot.Visible == false) ?? 
                                   this.SlotPrefab.Instance() as JoyItemSlot;

            return this.AddSlot(item, poolSlot);
        }

        protected virtual JoyItemSlot AddSlot(IItemInstance item = null, JoyItemSlot slot = null)
        {
            JoyItemSlot tempSlot = slot ?? this.SlotPrefab.Instance() as JoyItemSlot;
            if (tempSlot is null == false)
            {
                tempSlot.Visible = true;
                tempSlot.Item = item;
                this.SlotParent.AddChild(tempSlot);
                this.Slots.Add(tempSlot);
                return tempSlot;
            }

            GlobalConstants.ActionLog.Log("Could not create slot for container " + this.Name, LogLevel.Error);
            return null;
        }

        public virtual bool RemoveSlot(JoyItemSlot slot, bool pool = true)
        {
            var foundSlot = this.Slots.FirstOrDefault(itemSlot => itemSlot == slot);
            if (foundSlot is null)
            {
                return false;
            }

            if (pool)
            {
                foundSlot.Visible = false;
                return true;
            }

            this.Slots.Remove(slot);
            slot.QueueFree();
            return true;
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

        public virtual bool CanAddItem(IItemInstance item, string sourceContainer)
        {
            if (this.ContainerPriorities.IsNullOrEmpty())
            {
                return this.ContainerOwner.CanAddContents(item);
            }
            
            if (this.ContainerPriorities.Any(priority =>
                priority.m_ContainerName.Equals(sourceContainer, StringComparison.OrdinalIgnoreCase)))
            {
                return this.ContainerOwner.CanAddContents(item);
            }

            return false;
        }

        public virtual bool RemoveItem(IItemInstance item, int amount)
        {
            if (this.ContainerOwner is null)
            {
                return false;
            }

            bool result = false;
            if (this.ContainerOwner.Contents.Any(i =>
                i.IdentifiedName.Equals(item.IdentifiedName, StringComparison.OrdinalIgnoreCase)))
            {
                List<IItemInstance> matches = this.ContainerOwner.Contents.Where(itemInstance =>
                    itemInstance.IdentifiedName.Equals(item.IdentifiedName,
                        StringComparison.OrdinalIgnoreCase)).ToList();

                for (int i = 0; i < amount; i++)
                {
                    result |= this.ContainerOwner.RemoveContents(matches[i]);
                    result |= this.RemoveItem(matches[i]);
                }

                if (result)
                {
                    this.OnRemoveItem?.Invoke(this.ContainerOwner, new ItemChangedEventArgs {Item = item});
                }
            }

            return result;
        }

        public virtual bool RemoveItem(IItemInstance item)
        {
            if (this.ContainerOwner is null)
            {
                return false;
            }

            if (item.Guid != this.ContainerOwner.Guid)
            {
                if (this.ContainerOwner.Contains(item) == false ||
                    this.FilledSlots.Any(slot => slot.Item.Guid == item.Guid) == false)
                {
                    return false;
                }

                this.ContainerOwner.RemoveContents(item);
                foreach (JoyItemSlot joyItemSlot in this.Slots.Where(slot =>
                    !(slot.Item is null) && item.Equals(slot.Item)))
                {
                    joyItemSlot.Item = null;
                    joyItemSlot.Repaint();
                }

                this.OnRemoveItem?.Invoke(this.ContainerOwner, new ItemChangedEventArgs() {Item = item});
                return true;
            }

            return false;
        }

        public virtual bool RemoveItem(int index)
        {
            if (this.ContainerOwner is null)
            {
                return false;
            }

            if (index < this.Slots.Count)
            {
                JoyItemSlot slot = this.Slots[index];
                IItemInstance item = slot.Item;
                if (item is null)
                {
                    return false;
                }

                this.ContainerOwner.RemoveContents(item);
                slot.Item = null;
                this.OnRemoveItem?.Invoke(this.ContainerOwner, new ItemChangedEventArgs {Item = item});
            }

            return true;
        }

        public virtual bool StackOrSwap(ItemContainer destination, IItemInstance item)
        {
            if (this.ContainerOwner.Equals(destination.ContainerOwner))
            {
                return false;
            }

            if (destination.CanAddItem(item, this.Name) == false)
            {
                IEnumerable<JoyItemSlot> filledSlots = destination.GetRequiredSlots(item, true);
                bool result = true;
                foreach (JoyItemSlot slot in filledSlots)
                {
                    if (slot.Item is null == false && this.CanAddItem(slot.Item, destination.Name))
                    {
                        result &= this.StackOrAdd(slot.Item);
                        result &= slot.Container.RemoveItem(slot.Item);
                    }

                    if (!result)
                    {
                        break;
                    }
                }

                if (!result || !destination.CanAddItem(item, this.Name))
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
}