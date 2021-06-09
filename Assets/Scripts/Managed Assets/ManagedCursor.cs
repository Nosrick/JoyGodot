using System;
using System.Collections.Generic;
using Godot;
using JoyGodot.Assets.Scripts.GUI.Managed_Assets;
using JoyLib.Code;
using JoyLib.Code.Entities;
using JoyLib.Code.Events;
using JoyLib.Code.Graphics;
using JoyLib.Code.Settings;

namespace JoyGodot.Assets.Scripts.Managed_Assets
{
    public class ManagedCursor : Control
    {
        protected ManagedUIElement DragObject { get; set; }
        
        protected ManagedUIElement CursorObject { get; set; }
        
        public int CursorSize { get; protected set; }

        protected IEntity Player { get; set; }

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
            
            this.GrabPlayer();
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

        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);
            
            this.GrabPlayer();
        }

        protected void GrabPlayer()
        {
            if (this.Player is null)
            {
                this.Player = GlobalConstants.GameManager?.Player;

                if (this.Player is null)
                {
                    return;
                }

                this.Player.HappinessChange -= this.SetHappiness;
                this.Player.HappinessChange += this.SetHappiness;

                this.SetHappiness(this, new ValueChangedEventArgs<float>
                {
                    NewValue = this.Player.OverallHappiness
                });
            }
        }

        protected void SetHappiness(object sender, ValueChangedEventArgs<float> args)
        {
            bool useHappiness = (bool) GlobalConstants.GameManager.SettingsManager
                .Get(SettingsManager.HAPPINESS_CURSOR)
                .ObjectValue;
            
            float happiness = useHappiness ? args.NewValue : 1f;

            try
            {
                if (this.Material is ShaderMaterial shaderMaterial)
                {
                    shaderMaterial.SetShaderParam("happiness", happiness);
                }
            }
            catch (Exception e)
            {
                GD.PushError("Object has been disposed!");
            }
        }

        public override void _ExitTree()
        {
            if (this.Player is null == false)
            {
                this.Player.HappinessChange -= this.SetHappiness;
            }

            base._ExitTree();
        }
    }
}