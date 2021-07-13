using System;
using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Base_Interfaces;
using JoyGodot.Assets.Scripts.Collections;

namespace JoyGodot.Assets.Scripts.Items.Crafting
{
    public interface IRecipe : IGuidHolder
    {
        NonUniqueDictionary<string, int> RequiredMaterials { get; }
        
        List<BaseItemType> RequiredComponents { get; }

        IEnumerable<BaseItemType> CraftingResults { get; }
        
        IEnumerable<BaseItemType> ReturnResults { get; }

        bool CanCraft(
            NonUniqueDictionary<IItemMaterial, int> materials,
            IEnumerable<BaseItemType> components);

        bool OutputMaterialsMatch(NonUniqueDictionary<IItemMaterial, int> materials);
    }
}