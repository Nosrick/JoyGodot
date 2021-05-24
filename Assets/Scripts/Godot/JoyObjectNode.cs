using System;
using System.Linq;
using Castle.Core.Internal;
using Godot;
using JoyLib.Code.Graphics;
using JoyLib.Code.Unity;
using JoyLib.Code.Unity.GUI;

namespace JoyLib.Code.Godot
{
    public class JoyObjectNode : ManagedSprite
    {
        protected IJoyObject JoyObject { get; set; }

        protected IGUIManager GuiManager { get; set; }

        protected ManagedSprite SpeechBubble { get; set; }
        
        protected Area2D Collider { get; set; }

        public JoyObjectNode()
        {
        }

        public override void _Ready()
        {
            base._Ready();

            this.GuiManager = GlobalConstants.GameManager.GUIManager;
            this.Collider = this.GetNode<Area2D>("Mouse Collision");
        }

        public void AttachJoyObject(IJoyObject joyObject)
        {
            this.JoyObject = joyObject;
            this.JoyObject.MyNode = this;
            this.Name = joyObject.ToString();
            this.SpeechBubble = this.GetNodeOrNull<ManagedSprite>("Speech Bubble");
            this.Clear();
            ISpriteState state = this.JoyObject.States.FirstOrDefault();
            if (state is null)
            {
                return;
            }
            this.Position = joyObject.WorldPosition.ToVec2 * GlobalConstants.SPRITE_WORLD_SIZE;
            this.AddSpriteState(state);
            this.OverrideAllColours(state.SpriteData.GetRandomPartColours());
            float scale = (float) GlobalConstants.SPRITE_WORLD_SIZE / this.CurrentSpriteState.SpriteData.Size;
            this.Scale = new Vector2(scale, scale);
            var shape = this.Collider.GetChild<CollisionShape2D>(0).Shape as RectangleShape2D;
            shape.Extents = new Vector2(GlobalConstants.SPRITE_WORLD_SIZE * 1.8f, GlobalConstants.SPRITE_WORLD_SIZE * 1.8f);
            this.MoveChild(this.Collider, this.GetChildCount() - 1);
        }

        public void SetSpeechBubble(bool on, ISpriteState need = null)
        {
            if (this.SpeechBubble is null)
            {
                return;
            }

            this.SpeechBubble.Visible = true;
            if (on && need is null == false)
            {
                this.SpeechBubble.Clear();
                this.SpeechBubble.AddSpriteState(need);
                Texture needSprite = need.SpriteData.Parts.First(
                        part =>
                            part.m_Data.Any(data => data.Equals("need", StringComparison.OrdinalIgnoreCase)))
                                .m_FrameSprite.FirstOrDefault();
                //this.SetParticleSystem(needSprite, Color.white);

                //this.ParticleSystem.Play();
            }
        }

        public void OnPointerEnter()
        {
            GlobalConstants.ActionLog.Log("MOUSE ON " + this.JoyObject.JoyName);

            var player = GlobalConstants.GameManager.Player;
            
            if (this.JoyObject.Tooltip.IsNullOrEmpty()
                || player is null
                || player.VisionProvider.HasVisibility(player, player.MyWorld, this.JoyObject.WorldPosition) == false
                || this.GuiManager.IsActive(GUINames.TOOLTIP))
            {
                return;
            }

            this.GuiManager.Tooltip?.Show(
                this.JoyObject.JoyName,
                this.JoyObject.States.FirstOrDefault(),
                this.JoyObject.Tooltip);
        }

        public void OnPointerExit()
        {
            GlobalConstants.ActionLog.Log("MOUSE OFF " + this.JoyObject.JoyName);
            this.GuiManager.CloseGUI(GUINames.TOOLTIP);
            this.GuiManager.Tooltip.WipeData();
        }
    }
}