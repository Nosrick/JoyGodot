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

        public JoyObjectNode()
        {
            this.Awake();
        }

        public JoyObjectNode(IJoyObject joyObject)
            : this()
        {
            this.AttachJoyObject(joyObject);
            this.GuiManager = GlobalConstants.GameManager.GUIManager;
        }

        public void AttachJoyObject(IJoyObject joyObject)
        {
            this.JoyObject = joyObject;
            this.SpeechBubble = this.GetNodeOrNull<ManagedSprite>("Speech Bubble");
            this.Clear();
            this.AddSpriteState(this.JoyObject.States.FirstOrDefault());
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
                Texture needSprite = need.SpriteData.m_Parts.First(
                        part =>
                            part.m_Data.Any(data => data.Equals("need", StringComparison.OrdinalIgnoreCase)))
                                .m_FrameSprite.GetFrame("default", 0);
                //this.SetParticleSystem(needSprite, Color.white);

                //this.ParticleSystem.Play();
            }
        }

        public void OnPointerEnter()
        {
            if (this.JoyObject.Tooltip.IsNullOrEmpty())
            {
                return;
            }

            GlobalConstants.ActionLog.Log("MOUSE ON " + this.JoyObject.JoyName);

            Tooltip tooltip = (Tooltip) this.GuiManager.OpenGUI(GUINames.TOOLTIP);
            tooltip.Show(
                this.JoyObject.JoyName,
                null,
                this.JoyObject.States[0],
                this.JoyObject.Tooltip);
        }

        public void OnPointerExit()
        {
            GlobalConstants.ActionLog.Log("MOUSE OFF " + this.JoyObject.JoyName);
            this.GuiManager.CloseGUI(GUINames.TOOLTIP);
        }
    }
}