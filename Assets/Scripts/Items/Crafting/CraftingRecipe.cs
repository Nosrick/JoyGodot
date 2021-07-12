using System;
using System.Collections.Generic;
using System.Linq;
using JoyGodot.Assets.Scripts.Collections;

namespace JoyGodot.Assets.Scripts.Items.Crafting
{
    public class CraftingRecipe : IRecipe
    {
        public Guid Guid { get; protected set; }
        
        public NonUniqueDictionary<IItemMaterial, int> RequiredMaterials { get; protected set; }
        
        public List<BaseItemType> RequiredComponents { get; protected set; }
        
        public BaseItemType CraftingResult { get; protected set; }

        public CraftingRecipe(
            NonUniqueDictionary<IItemMaterial, int> requiredMaterials,
            IEnumerable<BaseItemType> components,
            BaseItemType craftingResult)
        {
            this.RequiredMaterials = requiredMaterials;
            this.RequiredComponents = new List<BaseItemType>(components);
            this.CraftingResult = craftingResult;
            this.Guid = GlobalConstants.GameManager.GUIDManager.AssignGUID();
        }

        public bool CanCraft(
            NonUniqueDictionary<IItemMaterial, int> materials, 
            List<BaseItemType> components)
        {
            bool result = true;
            
            foreach (Tuple<IItemMaterial, int> tuple in this.RequiredMaterials)
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

            var copyComponents = new List<BaseItemType>(components);
            foreach (BaseItemType component in this.RequiredComponents)
            {
                if (copyComponents.Contains(component))
                {
                    copyComponents.Remove(component);
                }
            }

            result &= copyComponents.Count == 0;
            
            return result;
        }
    }
}