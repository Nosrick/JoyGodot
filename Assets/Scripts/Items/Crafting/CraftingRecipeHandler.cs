using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using Godot.Collections;
using JoyGodot.Assets.Scripts.Collections;
using JoyGodot.Assets.Scripts.Helpers;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace JoyGodot.Assets.Scripts.Items.Crafting
{
    public class CraftingRecipeHandler : ICraftingRecipeHandler
    {
        public IEnumerable<IRecipe> Values => this.Recipes.Values;
        public JSONValueExtractor ValueExtractor { get; protected set; }
        
        protected NonUniqueDictionary<Guid, IRecipe> Recipes { get; set; }
        
        protected IItemDatabase ItemDatabase { get; set; }

        public CraftingRecipeHandler(IItemDatabase itemDatabase)
        {
            this.ItemDatabase = itemDatabase;
            this.ValueExtractor = new JSONValueExtractor();
            this.Recipes = new NonUniqueDictionary<Guid, IRecipe>(
                this.Load()
                    .Select(recipe => 
                        new Tuple<Guid, IRecipe>(
                            recipe.Guid, 
                            recipe)));
        }

        public IEnumerable<IRecipe> Load()
        {
            return this.ItemDatabase.Values
                .Select(itemType => 
                    new CraftingRecipe(
                        itemType.Materials, 
                        itemType.Components, 
                        itemType))
                .ToList();
        }
        
        public IRecipe Get(Guid guid)
        {
            return this.Recipes.First(tuple => tuple.Item1.Equals(guid)).Item2;
        }

        public IEnumerable<IRecipe> GetAllForName(string name)
        {
            return this.Recipes.Values.Where(recipe => 
                recipe.CraftingResult.UnidentifiedName.Equals(name, StringComparison.OrdinalIgnoreCase) ||
                recipe.CraftingResult.IdentifiedName.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public bool Add(IRecipe value)
        {
            this.Recipes.Add(value.Guid, value);
            return true;
        }

        public bool Destroy(Guid key)
        {
            return this.Recipes.RemoveByKey(key) > 0;
        }

        public void Dispose()
        {
        }
    }
}