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
        
        public List<BaseItemType> RequiredComponents { get; protected set; }
        
        public IEnumerable<BaseItemType> CraftingResults { get; protected set; }
        
        public IEnumerable<BaseItemType> ReturnResults { get; protected set; }

        public CraftingRecipe(
            NonUniqueDictionary<string, int> requiredMaterials,
            IEnumerable<BaseItemType> components,
            IEnumerable<BaseItemType> craftingResults,
            IEnumerable<BaseItemType> returnResults = null)
        {
            this.RequiredMaterials = requiredMaterials;
            this.RequiredComponents = new List<BaseItemType>(components);
            this.CraftingResults = craftingResults;
            this.ReturnResults = returnResults ?? new List<BaseItemType>();
            this.Guid = GlobalConstants.GameManager.GUIDManager.AssignGUID();
        }

        public bool CanCraft(NonUniqueDictionary<IItemMaterial, int> materials,
            IEnumerable<BaseItemType> components)
        {
            bool result = true;
            
            foreach (Tuple<string, int> tuple in this.RequiredMaterials)
            {
                var recipeMaterials = this.RequiredMaterials
                    .Where(t => 
                        t.Item1.Equals(
                            tuple.Item1, 
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();
                var resultsList = materials
                    .Where(t =>
                        t.Item1.Name.Equals(
                            tuple.Item1,
                            StringComparison.OrdinalIgnoreCase)
                        || t.Item1.HasTag(tuple.Item1))
                    .ToList();
                resultsList = resultsList
                    .OrderByDescending(i => i.Item2)
                    .ToList();
                if (resultsList.Count < recipeMaterials.Count)
                {
                    result = false;
                    break;
                }
                
                int resultsSum = resultsList.Select(t => t.Item2).Sum();
                int recipeSum = recipeMaterials.Select(t => t.Item2).Sum();
                    
                if (resultsSum < recipeSum)
                {
                    result = false;
                    break;
                }
            }

            if (!result)
            {
                return result;
            }
            
            var copyComponents = new List<BaseItemType>(components);
            int leftToFill = this.RequiredComponents.Count;
            foreach (BaseItemType component in this.RequiredComponents)
            {
                if (copyComponents.Any(c => c.Equals(component)))
                {
                    copyComponents.Remove(component);
                    leftToFill--;
                }
            }

            if (leftToFill != 0)
            {
                result = false;
            }
            
            return result;
        }

        public bool OutputMaterialsMatch(NonUniqueDictionary<IItemMaterial, int> materials)
        {
            IEnumerable<string> materialNames = materials.Keys.Select(material => material.Name).Distinct();
            foreach (var result in this.CraftingResults)
            {
                var intersect = result.MaterialNames.Intersect(materialNames).Count();
                if (intersect < result.MaterialNames.Count())
                {
                    return false;
                }
            }
            
            return true;
        }
    }
}