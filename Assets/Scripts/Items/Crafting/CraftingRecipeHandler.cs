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
        
        protected NonUniqueDictionary<string, IRecipe> Recipes { get; set; }
        
        protected IItemDatabase ItemDatabase { get; set; }

        public CraftingRecipeHandler(IItemDatabase itemDatabase)
        {
            this.ItemDatabase = itemDatabase;
            this.ValueExtractor = new JSONValueExtractor();
            this.Recipes = new NonUniqueDictionary<string, IRecipe>(
                this.Load()
                    .Select(recipe => 
                        new Tuple<string, IRecipe>(
                            recipe.CraftingResult.UnidentifiedName, 
                            recipe)));
        }

        public IEnumerable<IRecipe> Load()
        {
            List<IRecipe> recipes = new List<IRecipe>();
            
            string[] files =
                Directory.GetFiles(
                    Directory.GetCurrentDirectory() +
                    GlobalConstants.ASSETS_FOLDER + 
                    GlobalConstants.DATA_FOLDER + 
                    "Recipes", 
                    "*.json",
                    SearchOption.AllDirectories);

            foreach (string file in files)
            {
                JSONParseResult result = JSON.Parse(File.ReadAllText(file));

                if (result.Error != Error.Ok)
                {
                    GlobalConstants.ActionLog.Log("Could not load entity templates from " + file, LogLevel.Warning);
                    GlobalConstants.ActionLog.Log(result.ErrorString, LogLevel.Warning);
                    GlobalConstants.ActionLog.Log("On line: " + result.ErrorLine, LogLevel.Warning);
                    continue;
                }

                if (!(result.Result is Dictionary dictionary))
                {
                    GlobalConstants.ActionLog.Log("Could not parse JSON to Dictionary from " + file, LogLevel.Warning);
                    continue;
                }

                ICollection<Dictionary> recipeCollection =
                    this.ValueExtractor.GetArrayValuesCollectionFromDictionary<Dictionary>(dictionary, "Recipes");

                foreach (Dictionary recipeDict in recipeCollection)
                {
                    NonUniqueDictionary<string, int> materials = new NonUniqueDictionary<string, int>();

                    string resultName = this.ValueExtractor.GetValueFromDictionary<string>(recipeDict, "Result");
                    ICollection<Dictionary> materialsDicts =
                        this.ValueExtractor.GetArrayValuesCollectionFromDictionary<Dictionary>(recipeDict,
                            "Materials");

                    foreach (Dictionary materialsDict in materialsDicts)
                    {
                        string name = this.ValueExtractor.GetValueFromDictionary<string>(materialsDict, "Name");
                        int value = this.ValueExtractor.GetValueFromDictionary<int>(materialsDict, "Value");
                        
                        materials.Add(name, value);
                    }

                    var itemTypes = this.ItemDatabase.GetAllForName(resultName);

                    recipes.AddRange(itemTypes.Select(itemType => new CraftingRecipe(materials, itemType)));
                }
            }
            
            return recipes;
        }
        
        public IRecipe Get(string name)
        {
            return this.Recipes.First(tuple => tuple.Item1.Equals(name, StringComparison.OrdinalIgnoreCase)).Item2;
        }

        public IEnumerable<IRecipe> GetAllForName(string name)
        {
            return this.Recipes.FetchValuesForKey(name);
        }

        public bool Add(IRecipe value)
        {
            this.Recipes.Add(value.CraftingResult.UnidentifiedName, value);
            return true;
        }

        public bool Destroy(string key)
        {
            return this.Recipes.RemoveByKey(key) > 0;
        }

        public void Dispose()
        {
        }
    }
}