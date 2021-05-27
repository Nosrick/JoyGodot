using System;
using System.Linq;
using Castle.Core.Internal;
using Godot;
using JoyLib.Code.Graphics;
using JoyLib.Code.Unity;
using JoyLib.Code.Unity.GUI;

namespace JoyLib.Code.Godot
{
    public class JoyObjectNode : 
        ManagedSprite
    {
        public IJoyObject MyJoyObject { get; protected set; }

        protected IGUIManager GuiManager { get; set; }

        protected ManagedSprite SpeechBubble { get; set; }
        
        protected Area2D Collider { get; set; }
        
        public bool MouseOver { get; protected set; }

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
            this.MyJoyObject = joyObject;
            this.MyJoyObject.MyNode = this;
            this.Name = joyObject.ToString();
            this.SpeechBubble = this.GetNodeOrNull<ManagedSprite>("Speech Bubble");
            this.Clear();
            ISpriteState state = this.MyJoyObject.States.FirstOrDefault();
            if (state is null)
            {
                return;
            }
            this.Move(joyObject.WorldPosition);
            this.AddSpriteState(state);
            this.OverrideAllColours(state.SpriteData.GetCurrentPartColours());
            float scale = (float) GlobalConstants.SPRITE_WORLD_SIZE / this.CurrentSpriteState.SpriteData.Size;
            this.Scale = new Vector2(scale, scale);
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
            GlobalConstants.ActionLog.Log("MOUSE ON " + this.MyJoyObject.JoyName);

            var player = GlobalConstants.GameManager.Player;
            
            if (this.MyJoyObject.Tooltip.IsNullOrEmpty()
                || player is null
                || player.VisionProvider.HasVisibility(player, player.MyWorld, this.MyJoyObject.WorldPosition) == false)
            {
                return;
            }

            this.MouseOver = true;

            this.GuiManager.Tooltip?.Show(
                this, 
                this.MyJoyObject.JoyName,
                this.MyJoyObject.States.FirstOrDefault(),
                this.MyJoyObject.Tooltip);
        }

        public void OnPointerExit()
        {
            this.MouseOver = false;
            
            GlobalConstants.ActionLog.Log("MOUSE OFF " + this.MyJoyObject.JoyName);
            this.GuiManager.CloseGUI(this, GUINames.TOOLTIP);
        }
    }
}