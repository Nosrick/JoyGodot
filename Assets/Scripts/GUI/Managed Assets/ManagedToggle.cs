using Godot;
using JoyGodot.Assets.Scripts.GUI.Managed_Assets;

namespace Code.Unity.GUI.Managed_Assets
{
    public class ManagedToggle : CheckBox, IManagedElement
    {
        public string ElementName { get; protected set; }
        public bool Initialised { get; protected set; }
    }
}