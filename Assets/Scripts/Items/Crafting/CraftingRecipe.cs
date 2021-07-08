using System;
using System.Collections.Generic;
using System.Linq;
using JoyGodot.Assets.Scripts.Collections;

namespace JoyGodot.Assets.Scripts.Items.Crafting
{
    public class CraftingRecipe : IRecipe
    {
        public Guid Guid { get; protected set; }
        
        public NonUniqueDictionary<string, int> RequiredMaterials { get; protected set; }
        
        public BaseItemType CraftingResult { get; protected set; }

        public CraftingRecipe(
            NonUniqueDictionary<string, int> requiredMaterials,
            BaseItemType craftingResult)
        {
            this.RequiredMaterials = requiredMaterials;
            this.CraftingResult = craftingResult;
            this.Guid = GlobalConstants.GameManager.GUIDManager.AssignGUID();
        }

        public bool CanCraft(NonUniqueDictionary<string, int> materials)
        {
            bool result = true;
            
            foreach (Tuple<string, int> tuple in this.RequiredMaterials)
            {
                var recipeMaterials = this.RequiredMaterials.FetchValuesForKey(tuple.Item1);
                List<int> resultsList = materials.FetchValuesForKey(tuple.Item1);
                resultsList = resultsList
                    .OrderByDescending(i => i)
                    .ToList();
                if (resultsList.Count < recipeMaterials.Count)
                {
                    result = false;
                    break;
                }

                for (int i = 0; i < recipeMaterials.Count; i++)
                {
                    int resultsMin = resultsList.Min();
                    int recipeMin = recipeMaterials.Min();
                    
                    if (resultsMin < recipeMin)
                    {
                        result = false;
                        break;
                    }

                    resultsList.Remove(resultsMin);
                }

                if (!result)
                {
                    break;
                }
            }
            
            return result;
        }
    }
}