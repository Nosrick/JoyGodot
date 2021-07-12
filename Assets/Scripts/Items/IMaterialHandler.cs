using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Base_Interfaces;

namespace JoyGodot.Assets.Scripts.Items
{
    public interface IMaterialHandler : IHandler<IItemMaterial, string>
    {
        IEnumerable<IItemMaterial> GetPossibilities(string type);

        IItemMaterial GetRandomType(string type);

        IEnumerable<IItemMaterial> GetMany(IEnumerable<string> names, bool fuzzy = false);
    }
}