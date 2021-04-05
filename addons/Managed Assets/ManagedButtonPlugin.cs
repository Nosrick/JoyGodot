#if TOOLS
using Godot;

namespace JoyLib.addons.ManagedAssets
{
    [Tool]
    public class ManagedButtonPlugin : EditorPlugin
    {
        public override void _EnterTree()
        {
            // Initialization of the plugin goes here.
            // Add the new type with a name, a parent type, a script and an icon.
            var script = GD.Load<Script>("addons/Managed Assets/ManagedButton.cs");
            var texture = GD.Load<Texture>("icon.png");
            this.AddCustomType("ManagedButton", "Control", script, texture);

            script = GD.Load<Script>("addons/Managed Assets/ManagedTextButton.cs");
            this.AddCustomType("ManagedTextButton", "Control", script, texture);
        }

        public override void _ExitTree()
        {
            // Clean-up of the plugin goes here.
            // Always remember to remove it from the engine when deactivated.
            this.RemoveCustomType("ManagedButton");
            this.RemoveCustomType("ManagedTextButton");
        }
    }
}
#endif