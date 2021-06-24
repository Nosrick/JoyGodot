using System.Collections.Generic;
using Godot;
using JoyGodot.Assets.Scripts.Base_Interfaces;

namespace JoyGodot.Assets.Scripts.Entities.Statistics
{
    public interface IDerivedValueHandler : IHandler<IDerivedValue, string>
    {
        IDerivedValue Calculate<T>(string name, IEnumerable<IBasicValue<T>> components)
            where T : struct;

        int Calculate<T>(IEnumerable<IBasicValue<T>> components, string formula)
            where T : struct;

        Dictionary<string, IDerivedValue> GetEntityStandardBlock(IEnumerable<IBasicValue<int>> components);

        Dictionary<string, IDerivedValue> GetItemStandardBlock(IEnumerable<IBasicValue<float>> components);

        bool AddFormula(string name, string formula);

        Color GetBarColour(string name);
        Color GetTextColour(string name);
        Color GetOutlineColour(string name);
    }
}