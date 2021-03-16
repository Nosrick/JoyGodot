using Godot;

namespace JoyGodot.Assets.Scripts.GUI.Managed_Assets
{
    public interface IManagedElement
    {
        string ElementName { get; }
        bool Initialised { get; }

        void SetTheme(Theme theme);
    }
}