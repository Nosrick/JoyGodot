using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Collections;

namespace JoyGodot.Assets.Scripts.Items.Crafting
{
    public interface IRecipe
    {
        NonUniqueDictionary<string, int> RequiredMaterials { get; }

        BaseItemType CraftingResult { get; }

        bool CanCraft(NonUniqueDictionary<string, int> materials);
    }
}