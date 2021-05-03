using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;
using JoyGodot.Assets.Scripts.GUI.Managed_Assets;
using JoyLib.Code.Graphics;

namespace JoyGodot.Assets.Scripts.Managed_Assets
{
    public class ManagedCursor : Control
    {
        protected ManagedUIElement DragObject { get; set; }
        
        protected ManagedUIElement CursorObject { get; set; }

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

        public void AddSpriteState(ISpriteState state)
        {
            this.CursorObject.Clear();
            this.CursorObject.AddSpriteState(state);
        }

        public void OverrideAllColours(IDictionary<string, Color> colours, bool crossfade = false,
            float duration = 0.1f)
        {
            this.CursorObject.OverrideAllColours(colours, crossfade, duration);
        }

        public override void _Ready()
        {
            base._Ready();

            this.CursorObject = this.GetNode<ManagedUIElement>("Cursor Object");
            this.DragObject = this.GetNode<ManagedUIElement>("Drag Object");
            this.DragObject.Visible = false;
        }

        public override void _Input(InputEvent @event)
        {
            base._Input(@event);

            if (@event is InputEventMouseMotion motion)
            {
                this.CursorObject.SetPosition(motion.Position);
                this.DragObject.SetPosition(motion.Position);
            }
        }
    }
}