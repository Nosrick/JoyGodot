using Godot;
using JoyGodot.Assets.Scripts.GUI.Managed_Assets;
using JoyLib.Code.Graphics;

namespace JoyGodot.Assets.Scripts.Managed_Assets
{
    public class ManagedCursor : ManagedUIElement
    {
        protected ManagedUIElement DragObject { get; set; }

        public ISpriteState DragSprite
        {
            get => this.DragObject.CurrentSpriteState;
            set
            {
                this.DragObject.Clear();
                if (value is null)
                {
                    return;
                }
                
                this.DragObject.AddSpriteState(value);
            }
        }

        public override void _Ready()
        {
            base._Ready();

            this.DragObject = this.GetNode<ManagedUIElement>("Drag Object");
        }

        public override void _GuiInput(InputEvent @event)
        {
            base._GuiInput(@event);

            if (@event is InputEventMouseMotion motion)
            {
                this.SetGlobalPosition(motion.GlobalPosition);
            }
        }
    }
}