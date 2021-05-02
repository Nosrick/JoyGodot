using Godot;
using JoyGodot.addons.Managed_Assets;

namespace JoyGodot.Assets.Scripts.Managed_Assets
{
    #if TOOLS
    [Tool]
    #endif
    public class ConstrainedManagedTextButton : ManagedTextButton
    {
        public int PointRestriction { get; set; }
        
        public int Value { get; set; }

        [Signal]
        public delegate void ValuePress(string myName, int delta, bool newValue);

        [Signal]
        public delegate void ValueToggle(string myName, int delta, bool newValue);

        protected override void Press()
        {
            if (this.PointRestriction < this.Value)
            {
                return;
            }
            
            base.Press();
            this.EmitSignal(
                "ValuePress",
                this.Name,
                this.Pressed ? 1 : -1,
                this.Pressed);

            if (this.ToggleMode)
            {
                this.EmitSignal(
                    "ValueToggle", 
                    this.Name,
                    this.Pressed ? 1 : -1,
                    this.Pressed);
            }
        }
    }
}