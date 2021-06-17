using System.Collections.Generic;
using Godot;

namespace JoyGodot.Assets.Scripts.Managed_Assets
{
    public interface IColourableElement : IManagedElement
    {
        void OverrideAllColours(
            IDictionary<string,
                Color> colours,
            bool crossFade = false,
            float duration = 0.1f,
            bool modulateChildren = false);

        void TintWithSingleColour(
            Color colour,
            bool crossFade = false,
            float duration = 0.1f,
            bool modulateChildren = false);
    }
}