using System.Collections.Generic;
using Godot;

namespace JoyGodot.Assets.Scripts.GUI.Managed_Assets
{
    public interface IColourableElement : IManagedElement
    {
        void OverrideAllColours(
            IDictionary<string,
                Color> colours,
            bool crossFade = false,
            float duration = 0.1f);

        void TintWithSingleColour(
            Color colour,
            bool crossFade = false,
            float duration = 0.1f);
    }
}