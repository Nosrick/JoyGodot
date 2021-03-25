using System.Collections.Generic;
using Godot;
using JoyLib.Code.Graphics;

namespace JoyLib.Code.Unity.GUI
{
    public class Cursor : GUIData
    {
        [Export]
        protected ManagedSprite m_PartPrefab;
        
        protected ManagedSprite CursorObject { get; set; }

        protected bool Initialised { get; set; }
        public ManagedSprite DragObject { get; set; }

        public void Awake()
        {
            if (this.DragObject is null)
            {
                this.DragObject = new ManagedSprite();
                this.DragObject.Awake();
                this.DragObject.Visible = false;
            }

            if (this.CursorObject is null)
            {
                this.CursorObject = (ManagedSprite) this.m_PartPrefab.Duplicate();
                this.CursorObject.Awake();
            }

            this.Initialised = true;
        }

        public override void _Input(InputEvent @event)
        {
            if (!this.Visible)
            {
                return;
            }

            if (!(@event is InputEventMouse mouseEvent))
            {
                return;
            }

            Vector2 mousePosition = mouseEvent.Position;

            this.RectPosition = new Vector2(mousePosition.x, mousePosition.y);
        }

        public void SetCursorSize(int width, int height)
        {
            if (this.Initialised == false)
            {
                this.Awake();
            }

            //this. = new Vector2(width, height);
            //this.DragObject.Scale = new Vector2()
        }

        public void SetCursorSprites(ISpriteState state)
        {
            if (this.Initialised == false)
            {
                this.Awake();
            }

            this.CursorObject.Visible = true;
            this.CursorObject.Clear();
            this.CursorObject.AddSpriteState(state);
        }

        public void SetCursorColours(IDictionary<string, Color> colours)
        {
            if (this.Initialised == false)
            {
                this.Awake();
            }
            foreach (ISpriteState state in this.CursorObject.States)
            {
                for (int j = 0; j < state.SpriteData.m_Parts.Count; j++)
                {
                    SpritePart part = state.SpriteData.m_Parts[j];
                    if (!colours.ContainsKey(part.m_Name))
                    {
                        continue;
                    }

                    part.m_PossibleColours = new List<Color> {colours[part.m_Name]};
                    state.SpriteData.m_Parts[j] = part;
                    this.CursorObject.Clear();
                }
                this.CursorObject.Clear();
                this.CursorObject.AddSpriteState(state);
            }
        }

        public void Show(ISpriteState replacement)
        {
            if (this.Initialised == false)
            {
                this.Awake();
            }
            this.DragObject.Clear();
            this.DragObject.Visible = false;
            if (replacement is null)
            {
                return;
            }
            this.DragObject.Visible = true;
            this.DragObject.AddSpriteState(replacement);
        }

        public void Reset()
        {
            if (this.Initialised == false)
            {
                this.Awake();
            }
            this.DragObject.Clear();
        }
    }
}