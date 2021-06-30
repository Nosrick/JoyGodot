using Godot;

namespace JoyGodot.Assets.Scripts.GUI
{
    public class TooltipGrabber : Control
    {
        protected bool MouseInside { get; set; }

        public override void _GuiInput(InputEvent @event)
        {
            base._GuiInput(@event);

            if (@event is InputEventMouseMotion motion)
            {
                bool result = this.GetGlobalRect().HasPoint(motion.GlobalPosition);
                if (result && this.MouseInside == false)
                {
                    this.MouseInside = true;
                    this.EmitSignal("mouse_entered");
                }
                else if (result == false && this.MouseInside)
                {
                    this.MouseInside = false;
                    this.EmitSignal("mouse_exited");
                }
            }
        }
    }
}