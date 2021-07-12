using System.Collections.Generic;
using Godot;
using System;
using System.Collections;
using System.Linq;
using System.Text;
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
        
        protected IItemDatabase ItemDatabase { get; set; }

        public override void _Ready()
        {
            base._Ready();

            this.RecipeHandler = GlobalConstants.GameManager.CraftingRecipeHandler;
            this.ItemFactory = GlobalConstants.GameManager.ItemFactory;
            this.ItemDatabase = GlobalConstants.GameManager.ItemDatabase;

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

        public override bool Close(object sender)
        {
            this.Player.AddContents(this.CraftingItemContainer.Contents);
            this.CraftingItemContainer.RemoveAllItems();
            
            return base.Close(sender);
        }

        protected void SetUpRecipeList()
        {
            foreach (var guid in this.ItemDatabase.Values.Select(type => type.Guid).Distinct())
            {
                var recipes = this.RecipeHandler.GetAllForItemTypeGuid(guid);
                var results = recipes.First().CraftingResults.Select(type => type.IdentifiedName).ToArray();
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < results.Length; i++)
                {
                    builder.Append(results[i]);
                    if (i < results.Length - 1)
                    {
                        builder.Append(", ");
                    }
                }
                
                var instance = this.ButtonPrefab.Instance() as ManagedTextButton;
                instance.Visible = true;
                instance.Text = builder.ToString();
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
                        guid.ToString()
                    });
                
                this.ButtonContainer.AddChild(instance);
            }
        }

        public void SetRecipe(string guid)
        {
            var recipes = this.RecipeHandler.GetAllForItemTypeGuid(new Guid(guid));
            
            this.CraftingItemContainer.SetRecipe(recipes);
        }

        public void CraftButton()
        {
            if (this.CraftingItemContainer.CanCraft())
            {
                this.Player.RemoveContents(this.CraftingItemContainer.Contents);

                List<IItemInstance> newItems = new List<IItemInstance>();
                foreach (BaseItemType itemType in this.CraftingItemContainer.InferRecipeFromIngredients().CraftingResults)
                {
                    IItemInstance item = this.ItemFactory.CreateFromTemplate(
                        itemType,
                        true);
                    newItems.Add(item);
                    GlobalConstants.GameManager.ItemHandler.Add(item);
                }
                
                this.Player.AddContents(newItems);
                
                this.PlayerInventory.Display();
                
                this.CraftingItemContainer.RemoveAllItems();
            }
        }
    }
}