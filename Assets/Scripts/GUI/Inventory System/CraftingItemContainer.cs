using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using JoyGodot.Assets.Scripts.Collections;
using JoyGodot.Assets.Scripts.Events;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.Items;
using JoyGodot.Assets.Scripts.Items.Crafting;
using JoyGodot.Assets.Scripts.Managed_Assets;
using Array = Godot.Collections.Array;

namespace JoyGodot.Assets.Scripts.GUI.Inventory_System
{
    public class CraftingItemContainer : ItemContainer
    {
        public IEnumerable<IRecipe> PossibleRecipes { get; protected set; }

        public override void _Ready()
        {
            this.SlotParent = this.FindNode("CraftingInventory") as Container;
            this.SlotPrefab =
                GD.Load<PackedScene>(GlobalConstants.GODOT_ASSETS_FOLDER + "Scenes/Parts/JoyCraftingSlot.tscn");
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

            if (this.PossibleRecipes.IsNullOrEmpty() == false)
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
            foreach (JoyItemSlot slot in this.Slots)
            {
                slot.Visible = false;
            }

            if (this.PossibleRecipes.IsNullOrEmpty())
            {
                return;
            }

            var exampleRecipe = this.PossibleRecipes.First();

            for (int i = this.Slots.Count;
                i < exampleRecipe.RequiredMaterials.Count() + exampleRecipe.RequiredComponents.Count;
                i++)
            {
                var instance = this.AddSlot(false);
                this.GUIManager.SetupManagedComponents(instance);
                instance.Container = this;
            }

            var materialCollection = exampleRecipe.RequiredMaterials.Collection;
            for (int i = 0; i < materialCollection.Count; i++)
            {
                var material = materialCollection[i];
                var slot = this.Slots[i] as JoyCraftingSlot;
                slot.Visible = true;
                slot.IngredientType = "material";
                slot.Slot = material.Item1;
                slot.AmountRequired = material.Item2;
                slot.SlotLabel.Text = material.Item1 + ": " + material.Item2;
            }

            var componentCollection = exampleRecipe.RequiredComponents;
            for (int i = 0; i < componentCollection.Count; i++)
            {
                var component = componentCollection[i];
                var slot = this.Slots[i + materialCollection.Count] as JoyCraftingSlot;
                slot.Visible = true;
                slot.IngredientType = "component";
                slot.Slot = component.UnidentifiedName;
                slot.AmountRequired = 1;
                slot.SlotLabel.Text = component.UnidentifiedName;
            }
        }

        public void SetRecipe(IEnumerable<IRecipe> possibilities)
        {
            this.PossibleRecipes = possibilities;
            this.SetSlots();
        }

        public override List<JoyItemSlot> GetRequiredSlots(
            IItemInstance item, 
            bool takeFilledSlots = false,
            IEnumerable<JoyItemSlot> includedSlots = null)
        {
            List<JoyItemSlot> slots = new List<JoyItemSlot>();
            if (item is null)
            {
                return slots;
            }

            IDictionary<string, int> requiredSlots =
                new System.Collections.Generic.Dictionary<string, int>();

            List<string> itemInfo = new List<string>(item.Tags);
            itemInfo.AddRange(item.ItemType.Materials.Keys.Select(material => material.Name).Distinct());
            itemInfo.Add(item.ItemType.UnidentifiedName);

            foreach (string slot in itemInfo)
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
            
            if (includedSlots.IsNullOrEmpty() == false)
            {
                JoyItemSlot[] included = includedSlots.ToArray();
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
                                    && copySlots[pair.Key] > 0)
                                {
                                    copySlots[pair.Key] -= 1;
                                    slots.Add(slot);
                                }
                                else if (takeFilledSlots
                                         && copySlots[pair.Key] > 0)
                                {
                                    copySlots[pair.Key] -= 1;
                                    slots.Add(slot);
                                }
                            }
                        }
                    }
                }
            }

            if (requiredSlots.Values.Sum() == 0)
            {
                return slots;
            }

            List<JoyItemSlot> emptySlots = this.EmptySlots;
            for (int i = 0; i < emptySlots.Count; i++)
            {
                if (emptySlots[i] is JoyCraftingSlot craftingSlot
                    && craftingSlot.Slot.IsNullOrEmpty() == false)
                {
                    foreach (KeyValuePair<string, int> pair in requiredSlots)
                    {
                        if (pair.Key.Equals(craftingSlot.Slot, StringComparison.OrdinalIgnoreCase))
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

        public override bool StackOrAdd(IItemInstance item)
        {
            List<JoyItemSlot> slots = this.GetRequiredSlots(item);

            return this.StackOrAdd(slots, item);
        }

        public override bool StackOrAdd(IEnumerable<JoyItemSlot> slots, IItemInstance item)
        {
            if (this.ContainerOwner is null)
            {
                return false;
            }

            if (item.Guid != this.ContainerOwner.Guid)
            {
                if (this.ContainerOwner.CanAddContents(item))
                {
                    IEnumerable<JoyItemSlot> joyItemSlots = slots.ToList();
                    if (joyItemSlots.Any())
                    {
                        this.ContainerOwner.AddContents(item);
                        foreach (JoyItemSlot slot in joyItemSlots)
                        {
                            slot.Item = item;
                        }
                    }
                    else
                    {
                        return false;
                    }

                    return true;
                }

                return false;
            }

            return false;
        }

        public bool CanCraft()
        {
            bool canCraft = true;

            foreach (JoyItemSlot temp in this.FilledSlots)
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

        protected NonUniqueDictionary<IItemMaterial, int> GetMaterialsFromSlots()
        {
            NonUniqueDictionary<IItemMaterial, int> returnMaterials = new NonUniqueDictionary<IItemMaterial, int>();
            foreach (JoyItemSlot temp in this.FilledSlots)
            {
                if (temp is JoyCraftingSlot slot)
                {
                    returnMaterials.AddRange(slot.Item.ItemType.Materials);
                }
            }

            return returnMaterials;
        }

        public IRecipe InferRecipeFromIngredients()
        {
            NonUniqueDictionary<IItemMaterial, int> materials = this.GetMaterialsFromSlots();
            IEnumerable<BaseItemType> components = this.FilledSlots.Select(slot => slot.Item.ItemType);
            IEnumerable<IRecipe> recipes = this.PossibleRecipes.Where(recipe => 
                recipe.CanCraft(materials, components)
                && recipe.OutputMaterialsMatch(materials));
            return recipes.FirstOrDefault();
        }
    }
}