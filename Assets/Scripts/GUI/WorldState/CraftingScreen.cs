using System.Collections.Generic;
using Godot;
using System;
using Godot.Collections;
using JoyGodot.Assets.Scripts.GUI.Inventory_System;
using JoyGodot.Assets.Scripts.Items;
using JoyGodot.Assets.Scripts.Items.Crafting;
using JoyGodot.Assets.Scripts.Managed_Assets;
using Array = Godot.Collections.Array;

namespace JoyGodot.Assets.Scripts.GUI.WorldState
{
    public class CraftingScreen : GUIData
    {
        protected CraftingItemContainer CraftingItemContainer { get; set; }
        
        protected ItemContainer PlayerInventory { get; set; }
        
        protected PackedScene ButtonPrefab { get; set; }
        protected Container ButtonContainer { get; set; }
        protected List<ManagedTextButton> Buttons { get; set; }
        
        protected ICraftingRecipeHandler RecipeHandler { get; set; }
        protected IItemFactory ItemFactory { get; set; }

        public override void _Ready()
        {
            base._Ready();

            this.RecipeHandler = GlobalConstants.GameManager.CraftingRecipeHandler;
            this.ItemFactory = GlobalConstants.GameManager.ItemFactory;

            this.ButtonPrefab = GD.Load<PackedScene>(
                GlobalConstants.GODOT_ASSETS_FOLDER +
                "Scenes/Parts/ManagedTextButton.tscn");
            
            this.ButtonContainer = this.FindNode("RecipeContainer") as Container;
            this.Buttons = new List<ManagedTextButton>();
            this.PlayerInventory = this.FindNode("PlayerScrollContainer") as ItemContainer;
            this.PlayerInventory.ContainerOwner = this.Player;
            this.CraftingItemContainer = this.FindNode("CraftingScrollContainer") as CraftingItemContainer;
            
            this.SetUpRecipeList();
        }

        protected void SetUpRecipeList()
        {
            foreach (IRecipe recipe in this.RecipeHandler.Values)
            {
                var instance = this.ButtonPrefab.Instance() as ManagedTextButton;
                instance.Visible = true;
                instance.Text = recipe.CraftingResult.IdentifiedName;
                instance.RectMinSize = new Vector2(0, 24);
                if (instance.IsConnected(
                    "_Press",
                    this,
                    nameof(this.SetRecipe)))
                {
                    instance.Disconnect(
                        "_Press",
                        this,
                        nameof(this.SetRecipe));
                }
                instance.Connect(
                    "_Press",
                    this,
                    nameof(this.SetRecipe),
                    new Array
                    {
                        recipe.Guid.ToString()
                    });
                
                this.ButtonContainer.AddChild(instance);
            }
        }

        public void SetRecipe(string guid)
        {
            var recipe = this.RecipeHandler.Get(new Guid(guid));
            
            this.CraftingItemContainer.SetRecipe(recipe);
        }

        public void CraftButton()
        {
            if (this.CraftingItemContainer.CanCraft())
            {
                this.Player.AddContents(
                    this.ItemFactory.CreateFromTemplate(
                        this.CraftingItemContainer.CurrentRecipe.CraftingResult, 
                        true));
            }
        }
    }
}