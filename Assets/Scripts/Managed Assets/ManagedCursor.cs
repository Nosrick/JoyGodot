using System.Collections.Generic;
using Godot;
using JoyGodot.Assets.Scripts.GUI.Managed_Assets;
using JoyLib.Code;
using JoyLib.Code.Graphics;

namespace JoyGodot.Assets.Scripts.Managed_Assets
{
    public class ManagedCursor : Control
    {
        protected ManagedUIElement DragObject { get; set; }
        
        protected ManagedUIElement CursorObject { get; set; }
        
        public int CursorSize { get; protected set; }

        public ISpriteState DragSprite
        {
            get => this.DragObject.CurrentSpriteState;
            set
            {
                this.DragObject.Clear();
                if (value is null)
                {
                    this.DragObject.Visible = false;
                    return;
                }
                
                this.DragObject.AddSpriteState(value);
                this.DragObject.OverrideAllColours(value.SpriteData.GetCurrentPartColours());
                this.DragObject.Visible = true;
            }
        }

        public void AddSpriteState(ISpriteState state)
        {
            this.CursorObject.Clear();
            this.CursorObject.AddSpriteState(state);
            this.CursorSize = GlobalConstants.SPRITE_WORLD_SIZE;
            this.CursorObject.RectSize = new Vector2(this.CursorSize, this.CursorSize);
            this.DragObject.RectSize = new Vector2(this.CursorSize, this.CursorSize);
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
                this.CursorObject.RectPosition = motion.Position;
                this.DragObject.RectPosition = motion.Position;
            }
        }
    }
}