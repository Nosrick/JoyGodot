using System;
using System.Collections.Generic;
using System.Linq;
using JoyGodot.Assets.Scripts.Collections;
using JoyGodot.Assets.Scripts.Helpers;

namespace JoyGodot.Assets.Scripts.Items.Crafting
{
    public class CraftingRecipeHandler : ICraftingRecipeHandler
    {
        public IEnumerable<IRecipe> Values => this.Recipes.Values;
        public JSONValueExtractor ValueExtractor { get; protected set; }
        
        protected NonUniqueDictionary<Guid, IRecipe> Recipes { get; set; }

        public CraftingRecipeHandler()
        {
            this.ValueExtractor = new JSONValueExtractor();
            this.Recipes = new NonUniqueDictionary<Guid, IRecipe>();
        }

        public IEnumerable<IRecipe> Load()
        {
            return new List<IRecipe>();
        }
        
        public IRecipe Get(Guid key)
        {
            return this.Recipes.FirstOrDefault(tuple => tuple.Item1.Equals(key))?.Item2;
        }

        public IEnumerable<IRecipe> GetAllForName(string name)
        {
            return this.Recipes.Values.Where(recipe => 
                recipe.CraftingResults.Any(result => result.UnidentifiedName.Equals(name, StringComparison.OrdinalIgnoreCase) )||
                recipe.CraftingResults.Any(result => result.IdentifiedName.Equals(name, StringComparison.OrdinalIgnoreCase)));
        }

        public IEnumerable<IRecipe> GetAllForItemTypeGuid(Guid guid)
        {
            return this.Recipes.Values.Where(recipe => 
                recipe.CraftingResults.Any(result => result.Guid.Equals(guid)));
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