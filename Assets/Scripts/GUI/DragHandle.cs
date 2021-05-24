using Godot;

namespace JoyLib.Code.Unity.GUI
{
    public class DragHandle : Control
    {
        protected Control Parent { get; set; }
        
        protected bool Dragging { get; set; }
        protected bool FirstMove { get; set; }

        public override void _Ready()
        {
            base._Ready();

            this.Parent = this.GetParent<Control>();
        }

        public override void _GuiInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseButton)
            {
                if (mouseButton.ButtonIndex != (int) ButtonList.Left)
                {
                    return;
                }
                if (mouseButton.Pressed
                && this.GetGlobalRect().HasPoint(mouseButton.GlobalPosition))
                {
                    this.Dragging = true;
                    this.FirstMove = true;
                }
                else if (mouseButton.Pressed == false)
                {
                    this.Dragging = false;
                }
            }
            else if (@event is InputEventMouseMotion mouseMotion)
            {
                if (!this.Dragging)
                {
                    return;
                }
                
                if (this.FirstMove)
                {
                    this.FirstMove = false;
                }
                else
                {
                    this.Parent.RectGlobalPosition += mouseMotion.Relative;
                }
            }
        }
    }
}