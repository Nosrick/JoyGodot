using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.Items;
using JoyGodot.Assets.Scripts.Items.Crafting;
using JoyGodot.Assets.Scripts.Managed_Assets;
using Array = Godot.Collections.Array;

namespace JoyGodot.Assets.Scripts.GUI.Inventory_System
{
    public class CraftingItemContainer : ItemContainer
    {
        public IRecipe CurrentRecipe { get; protected set; }
        
        public override void _Ready()
        {
            this.SlotParent = this.FindNode("CraftingInventory") as Container;
            this.SlotPrefab = GD.Load<PackedScene>(GlobalConstants.GODOT_ASSETS_FOLDER + "Scenes/Parts/JoyCraftingSlot.tscn");
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

            foreach (JoyItemSlot slot in this.Slots)
            {
                slot.Container = this;
                slot.Item = null;
                slot.Visible = false;
            }

            if (this.CurrentRecipe is null == false)
            {
                this.SetSlots();
            }

            foreach (JoyItemSlot slot in this.Slots.Where(slot => slot.Visible))
            {
                slot.Repaint();
            }
        }

        protected void SetSlots()
        {
            if (this.CurrentRecipe is null)
            {
                foreach (JoyItemSlot slot in this.Slots)
                {
                    slot.Visible = false;
                }
                return;
            }
            
            for (int i = this.Slots.Count; i < this.CurrentRecipe.RequiredMaterials.Count(); i++)
            {
                var instance = this.AddSlot(true);
                this.GUIManager.SetupManagedComponents(instance);
                instance.Container = this;
            }

            var materialCollection = this.CurrentRecipe.RequiredMaterials.Collection;
            for (int i = 0; i < materialCollection.Count; i++)
            {
                var material = materialCollection[i];
                var slot = this.Slots[i] as JoyCraftingSlot;
                slot.Visible = true;
                slot.Slot = material.Item1;
                slot.AmountRequired = material.Item2;
                slot.SlotLabel.Text = material.Item1 + ": " + material.Item2;
            }
        }

        public void SetRecipe(IRecipe recipe)
        {
            this.CurrentRecipe = recipe;
            this.SetSlots();
        }
        
        public override List<JoyItemSlot> GetRequiredSlots(IItemInstance item, bool takeFilledSlots = false)
        {
            List<JoyItemSlot> slots = new List<JoyItemSlot>();
            if (item is null)
            {
                return slots;
            }

            IDictionary<string, int> requiredSlots =
                new System.Collections.Generic.Dictionary<string, int>();

            foreach (string slot in item.Tags)
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

            for (int i = 0; i < this.Slots.Count; i++)
            {
                if (this.Slots[i] is JoyCraftingSlot craftingSlot
                    && craftingSlot.Slot.IsNullOrEmpty() == false)
                {
                    foreach (KeyValuePair<string, int> pair in requiredSlots)
                    {
                        if (pair.Key.Equals(craftingSlot.Slot, StringComparison.OrdinalIgnoreCase))
                        {
                            if (takeFilledSlots == false
                                && this.Slots[i].IsEmpty
                                && copySlots[pair.Key] > 0)
                            {
                                copySlots[pair.Key] -= 1;
                                slots.Add(this.Slots[i]);
                            }
                            else if (takeFilledSlots
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

        public bool CanCraft()
        {
            bool canCraft = true;
            
            foreach (JoyItemSlot temp in this.Slots.Where(slot => slot.Visible))
            {
                if (temp is JoyCraftingSlot slot)
                {
                    if (slot.SufficientMaterial == false)
                    {
                        canCraft = false;
                        break;
                    }
                }
            }

            return canCraft;
        }
    }
}