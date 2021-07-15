using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Items;
using JoyGodot.Assets.Scripts.Managed_Assets;
using Array = Godot.Collections.Array;

namespace JoyGodot.Assets.Scripts.GUI.Inventory_System
{
    public class ConstrainedItemContainer : ItemContainer
    {
        public override void _Ready()
        {
            this.SlotParent = this.FindNode("Slot Container") as Container;
            this.SlotPrefab = GD.Load<PackedScene>(GlobalConstants.GODOT_ASSETS_FOLDER + "Scenes/Parts/JoyConstrainedSlot.tscn");
            this.Title = this.FindNode("Title") as ManagedLabel;
            
            this.OnEnable();
        }
        
        public override void OnEnable()
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
            if (this.m_ContainerNames is null)
            {
                this.m_ContainerNames = new Array<MoveContainerPriority>();
            }

            if (this.ContainerOwner is null)
            {
                this.ContainerOwner = new VirtualStorage();
            }

            if (this.ContainerOwner is EquipmentStorage equipment)
            {
                var contents = equipment.GetSlotsAndContents().ToList();
                
                if (this.Slots.Count < contents.Count)
                {
                    for (int i = this.Slots.Count; i < contents.Count; i++)
                    {
                        var instance = this.AddSlot(true);
                        this.GUIManager.SetupManagedComponents(instance);
                        instance.Container = this;
                    }
                }

                for (int i = 0; i < contents.Count; i++)
                {
                    var slot = this.Slots[i];
                    slot.Container = this;
                    if (slot is JoyConstrainedSlot equipmentSlot)
                    {
                        equipmentSlot.Slot = contents[i].Item1;
                    }
                }
                foreach (var tuple in contents)
                {
                    if (tuple.Item2 is null == false
                        && this.CanAddItem(tuple.Item2, this.Name))
                    {
                        this.StackOrAdd(tuple.Item2);
                    }
                }

                foreach (JoyItemSlot slot in this.Slots.Where(slot => slot.Visible))
                {
                    slot.Repaint();
                }
            }
            else if (this.ContainerOwner is IEntity entity)
            {
                this.ContainerOwner = entity.Equipment;
                this.OnEnable();
            }
        }
    }
}