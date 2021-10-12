using System;
using System.Collections;
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

        public int NumberOfSlots => this.ActiveSlots.Count();

        protected IEnumerable<JoyItemSlot> ActiveSlots => this.Slots
            .Where(slot => slot.Visible);

        protected IEnumerable<JoyItemSlot> FilledSlots =>
            this.Slots.Where(slot => slot.Visible && slot.ItemStack.Empty == false);

        protected IEnumerable<JoyItemSlot> EmptySlots => this.Slots
            .Where(slot => slot.Visible && slot.ItemStack.Empty);

        protected IEnumerable<JoyItemSlot> InactiveSlots => this.Slots
            .Where(slot => slot.Visible == false && slot.ItemStack.Empty);

        public Array<MoveContainerPriority> ContainerPriorities => this.m_ContainerNames;

        public virtual IItemContainer ContainerOwner
        {
            get => this.m_ContainerOwner;
            set
            {
                if (this.m_ContainerOwner is null == false)
                {
                    this.m_ContainerOwner.ItemAdded -= this.StackOrAddFromEvent;
                    this.m_ContainerOwner.ItemRemoved -= this.RemoveItemFromEvent;
                }

                this.m_ContainerOwner = value;

                this.m_ContainerOwner.ItemAdded += this.StackOrAddFromEvent;
                this.m_ContainerOwner.ItemRemoved += this.RemoveItemFromEvent;

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

            /*
            for (int i = this.Slots.Count; i < this.ContainerOwner.Contents.Count(); i++)
            {
                var instance = this.AddSlot(true);
                this.GUIManager.SetupManagedComponents(instance);
                instance.Container = this;
            }
            */

            IEnumerable<IItemInstance> contents = this.ContainerOwner.Contents;
            foreach (IItemInstance item in contents)
            {
                if (this.GetSlotsForItem(item).Any() == false)
                {
                    this.StackOrAddItem(item);
                }
            }

            foreach (JoyItemSlot slot in this.ActiveSlots)
            {
                slot.Repaint();
            }
        }

        protected virtual bool StackOrAddItem(
            IItemInstance item,
            IEnumerable<JoyItemSlot> slots = null,
            bool takeFilledSlots = false)
        {
            if (item is null)
            {
                return true;
            }

            if (this.ContainerOwner is null)
            {
                return false;
            }

            if (this.GetSlotsForItem(item).Any())
            {
                return true;
            }

            if (this.FilledSlots.Any(slot => slot.ItemStack.CanAddContents(item)))
            {
                var slot = this.FilledSlots.First(slot => slot.ItemStack.CanAddContents(item));
                slot.ItemStack.AddContents(item);
                slot.Repaint();
                this.OnAddItem?.Invoke(this.ContainerOwner, item);
                return true;
            }

            var requiredSlots = this.GetRequiredSlots(item, takeFilledSlots, slots);
            if (requiredSlots.Any())
            {
                foreach (JoyItemSlot slot in requiredSlots)
                {
                    slot.ItemStack.AddContents(item);
                    slot.Repaint();
                }
            }
            else
            {
                var emptySlot = this.EmptySlots.FirstOrDefault() ?? this.AddSlot(false);

                emptySlot.ItemStack.AddContents(item);
                emptySlot.Repaint();
            }

            this.OnAddItem?.Invoke(this.ContainerOwner, item);
            return true;
        }

        protected virtual bool StackOrAddItemStack(
            ItemStack itemStack,
            IEnumerable<JoyItemSlot> slots = null,
            bool takeFilledSlots = false)
        {
            if (itemStack.Empty)
            {
                return true;
            }

            if (this.ContainerOwner is null)
            {
                return false;
            }

            var firstItem = itemStack.Contents.First();
            
            if (this.FilledSlots.Any(slot => slot.ItemStack.CanAddContents(firstItem)))
            {
                var slot = this.FilledSlots.First(slot => slot.ItemStack.CanAddContents(firstItem));
                slot.ItemStack.AddContents(itemStack.Contents);
                this.OnAddItemStack?.Invoke(this, itemStack);
                return true;
            }

            var availableSlot = this.GetFirstAvailableSlotForStack(itemStack);
            if (availableSlot is null)
            {
                return false;
            }
            
            availableSlot.ItemStack.AddContents(itemStack.Contents);
            availableSlot.Repaint();
            this.OnAddItemStack?.Invoke(this, itemStack);
            return true;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stack">The stack being moved</param>
        /// <returns>The first available JoyItemSlot that matches the input's type, an empty slot, or null if nothing is found</returns>
        protected virtual JoyItemSlot GetFirstAvailableSlotForStack(ItemStack stack)
        {
            return this.FilledSlots.FirstOrDefault(slot => slot.ItemStack.CanAddContents(stack.Contents))
                ?? this.EmptySlots.FirstOrDefault();
        }

        public virtual bool StackOrAddFromEvent(IItemContainer source, IItemInstance item)
        {
            List<JoyItemSlot> slots;

            if (item is null)
            {
                return true;
            }

            if (item.ItemType.Slots.Any() == false)
            {
                if (this.FilledSlots.Any(slot => slot.ItemStack.CanAddContents(item)))
                {
                    slots = new List<JoyItemSlot> { this.FilledSlots.First(slot => slot.ItemStack.CanAddContents(item)) };
                }
                else if (this.EmptySlots.Any())
                {
                    slots = new List<JoyItemSlot> { this.EmptySlots.First() };
                }
                else
                {
                    if (this.DynamicContainer)
                    {
                        slots = new List<JoyItemSlot> { this.AddSlot(true) };
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                slots = this.GetRequiredSlots(item);
            }

            return this.StackOrAddItem(item, slots);
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
                slot.ItemStack.Clear();
                slot.Repaint();
            }

            this.ContainerOwner.Clear();
        }

        public IEnumerable<JoyItemSlot> GetSlotsForItem(IItemInstance item)
        {
            return this.FilledSlots.Where(slot => slot.ItemStack.Contains(item));
        }

        public IEnumerable<JoyItemSlot> GetSlotsForStack(ItemStack itemStack)
        {
            return itemStack.Empty 
                ? new List<JoyItemSlot>() 
                : this.Slots.Where(slot => slot.ItemStack.CanAddContents(itemStack.Contents));
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

            ItemContainer target = this.MoveToContainers.FirstOrDefault(
                container => sorted.Any(
                    sort =>
                        sort.m_ContainerName.Equals(container.Name, StringComparison.OrdinalIgnoreCase)
                        && ((sort.m_RequiresVisibility && container.Visible)
                            || sort.m_RequiresVisibility == false)));

            if (target is null || item is null)
            {
                return false;
            }

            return this.StackOrSwap(
                this.GetSlotsForItem(item).ToArray(),
                target.EmptySlots.ToArray(),
                this,
                target);
        }

        public virtual bool MoveStack(ItemStack itemStack)
        {
            var sorted = (from priority in this.m_ContainerNames
                orderby priority.m_Priority descending
                select priority);

            if (this.MoveToContainers.Any() == false)
            {
                return false;
            }

            ItemContainer target = this.MoveToContainers.FirstOrDefault(
                container => sorted.Any(
                    sort =>
                        sort.m_ContainerName.Equals(container.Name, StringComparison.OrdinalIgnoreCase)
                        && ((sort.m_RequiresVisibility && container.Visible)
                            || sort.m_RequiresVisibility == false)));

            if (target is null || itemStack.Empty)
            {
                return false;
            }

            return this.StackOrSwap(
                new List<JoyItemSlot> { this.FilledSlots.First(slot => slot.ItemStack == itemStack) },
                target.EmptySlots.ToArray(),
                this,
                target);
        }

        public virtual List<JoyItemSlot> GetRequiredSlots(
            IItemInstance item,
            bool takeFilledSlots = false,
            IEnumerable<JoyItemSlot> includedSlots = null)
        {
            List<JoyItemSlot> slots = new List<JoyItemSlot>();
            if (item is null)
            {
                return slots;
            }

            if (item.ItemType.Slots.Any() == false)
            {
                if (includedSlots.IsNullOrEmpty() == false
                    && includedSlots.Any())
                {
                    return includedSlots.ToList();
                }

                if (this.EmptySlots.Any())
                {
                    return new List<JoyItemSlot> { this.EmptySlots.First() };
                }

                return new List<JoyItemSlot>();
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

            JoyItemSlot[] included = includedSlots?.ToArray() ?? System.Array.Empty<JoyItemSlot>();
            if (included.IsNullOrEmpty() == false)
            {
                foreach (JoyItemSlot slot in included)
                {
                    if (slot is JoyConstrainedSlot constrainedSlot
                        && constrainedSlot.Slot.IsNullOrEmpty() == false)
                    {
                        foreach (KeyValuePair<string, int> pair in requiredSlots)
                        {
                            if (pair.Key.Equals(constrainedSlot.Slot, StringComparison.OrdinalIgnoreCase))
                            {
                                if (takeFilledSlots == false
                                    && slot.IsEmpty
                                    && copySlots[pair.Key] > 0
                                    && slot.ItemStack.CanAddContents(item))
                                {
                                    copySlots[pair.Key] -= 1;
                                    slots.Add(slot);
                                }
                                else if (takeFilledSlots
                                         && copySlots[pair.Key] > 0
                                         && slot.ItemStack.CanAddContents(item))
                                {
                                    copySlots[pair.Key] -= 1;
                                    slots.Add(slot);
                                }
                            }
                        }
                    }
                    else
                    {
                        slots.AddRange(included);
                        return slots;
                    }
                }
            }

            if (requiredSlots.Values.Sum() == 0)
            {
                return slots;
            }

            IEnumerable<JoyItemSlot> emptySlots = this.EmptySlots;
            foreach (JoyItemSlot emptySlot in emptySlots)
            {
                if (included.Contains(emptySlot))
                {
                    continue;
                }

                if (emptySlot is JoyConstrainedSlot constrainedSlot
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
                                slots.Add(emptySlot);
                            }
                            else if (takeFilledSlots
                                     && copySlots[pair.Key] > 0)
                            {
                                copySlots[pair.Key] -= 1;
                                slots.Add(emptySlot);
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
        public virtual JoyItemSlot AddSlot(bool pool, ItemStack item = null)
        {
            if (!pool)
            {
                return this.AddSlot();
            }

            JoyItemSlot poolSlot = this.EmptySlots.FirstOrDefault(itemSlot => itemSlot.Visible == false) ??
                                   this.SlotPrefab.Instance() as JoyItemSlot;

            return this.AddSlot(item, poolSlot);
        }

        protected virtual JoyItemSlot AddSlot(ItemStack item = null, JoyItemSlot slot = null)
        {
            JoyItemSlot tempSlot = slot ?? this.SlotPrefab.Instance() as JoyItemSlot;
            if (tempSlot is null == false)
            {
                tempSlot.Visible = true;
                tempSlot.ItemStack = item;
                tempSlot.Container = this;
                this.SlotParent.AddChild(tempSlot);
                this.Slots.Add(tempSlot);
                GlobalConstants.GameManager.GUIManager.SetupManagedComponents(tempSlot);
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
                foreach (JoyItemSlot slot in this.Slots)
                {
                    slot.Visible = false;
                    slot.ItemStack.Clear();
                }

                return true;
            }

            this.Slots.ForEach(slot => slot.QueueFree());
            return true;
        }

        public override bool CanDropData(Vector2 position, object data)
        {
            return data is DragObject;
        }

        public override void DropData(Vector2 position, object data)
        {
            if (data is DragObject dragObject)
            {
                var cursor = this.GUIManager.Cursor;
                cursor.DragSprite = null;

                if (this.ContainerOwner.CanAddContents(dragObject.ItemStack.Contents))
                {
                    this.ContainerOwner.AddContents(dragObject.ItemStack.Contents);
                    dragObject.SourceContainer.ContainerOwner.RemoveContents(dragObject.ItemStack.Contents);
                }
            }
        }

        public virtual bool RemoveItem(ItemStack item)
        {
            bool removed = false;
            foreach (JoyItemSlot slot in this.FilledSlots.Where(slot => slot.ItemStack == item))
            {
                removed = true;
                slot.ItemStack.Clear();
            }

            return removed;
        }

        public virtual bool RemoveItemFromEvent(IItemContainer source, IItemInstance item)
        {
            if (item is null)
            {
                return false;
            }

            if (this.ContainerOwner is null)
            {
                return false;
            }

            if (item.Guid != this.ContainerOwner.Guid)
            {
                if (this.FilledSlots.Any(slot => slot.ItemStack.Contents.Any(instance => instance == item)) == false)
                {
                    return false;
                }

                foreach (JoyItemSlot joyItemSlot in this.FilledSlots.Where(
                    slot => slot.ItemStack.Contains(item)))
                {
                    joyItemSlot.ItemStack.RemoveContents(item);
                    joyItemSlot.Repaint();
                }

                this.OnRemoveItem?.Invoke(this.ContainerOwner, item);
                return true;
            }

            return false;
        }

        public virtual bool Contains(ItemStack item)
        {
            return this.FilledSlots.Any(slot => slot.ItemStack.Equals(item));
        }

        public virtual bool CanAdd(ItemStack item)
        {
            return this.EmptySlots.Any();
        }

        public virtual bool StackOrSwap(
            ICollection<JoyItemSlot> sourceSlots,
            ICollection<JoyItemSlot> destinationSlots,
            ItemContainer sourceContainer,
            ItemContainer destinationContainer)
        {
            if (sourceSlots.Count == destinationSlots.Count)
            {
                for (int i = 0; i < sourceSlots.Count; i++)
                {
                    var leftSlot = sourceSlots.ElementAt(i);
                    var rightSlot = destinationSlots.ElementAt(i);

                    var leftStack = leftSlot.ItemStack;
                    var rightStack = rightSlot.ItemStack;

                    leftSlot.ItemStack = rightStack;
                    rightSlot.ItemStack = leftStack;
                }

                return true;
            }
            else
            {
                int slots = Math.Min(sourceSlots.Count, destinationSlots.Count);
                for (int i = 0; i < slots; i++)
                {
                    var leftSlot = sourceSlots.ElementAt(i);
                    var rightSlot = destinationSlots.ElementAt(i);

                    var leftStack = leftSlot.ItemStack;
                    var rightStack = rightSlot.ItemStack;

                    leftSlot.ItemStack = rightStack;
                    rightSlot.ItemStack = leftStack;
                }
                
                for (int i = slots; i < sourceSlots.Count; i++)
                {
                    JoyItemSlot sourceSlot = sourceSlots.ElementAt(i);
                    if (destinationContainer.CanAdd(sourceSlot.ItemStack))
                    {
                        destinationContainer.StackOrAddItemStack(sourceSlot.ItemStack);
                        sourceContainer.RemoveItem(sourceSlot.ItemStack);
                        sourceSlot.ItemStack.Clear();
                        sourceSlot.Repaint();
                    }
                }

                for (int i = slots; i < destinationSlots.Count; i++)
                {
                    JoyItemSlot destinationSlot = destinationSlots.ElementAt(i);
                    if (sourceContainer.CanAdd(destinationSlot.ItemStack))
                    {
                        sourceContainer.StackOrAddItemStack(destinationSlot.ItemStack);
                        destinationContainer.RemoveItem(destinationSlot.ItemStack);
                        destinationSlot.ItemStack.Clear();
                        destinationSlot.Repaint();
                    }
                }

                return true;
            }

            return false;
        }

        public override string ToString()
        {
            return this.ContainerOwner?.JoyName ?? "None";
        }

        public virtual event ItemAddedEventHandler OnAddItem;
        public virtual event ItemRemovedEventHandler OnRemoveItem;

        public virtual event ItemStackAddedEventHandler OnAddItemStack;
        public virtual event ItemStackRemovedEventHandler OnRemoveItemStack;
    }
}