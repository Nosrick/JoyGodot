using System;
using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Base_Interfaces;

namespace JoyGodot.Assets.Scripts.Items.Crafting
{
    public interface ICraftingRecipeHandler : IHandler<IRecipe, Guid>
    {
        IEnumerable<IRecipe> GetAllForName(string name);
    }
}