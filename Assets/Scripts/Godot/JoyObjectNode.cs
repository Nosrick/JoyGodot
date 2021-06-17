using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Godot;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.GUI;
using JoyGodot.Assets.Scripts.GUI.WorldState;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Managed_Assets;

namespace JoyGodot.Assets.Scripts.Godot
{
    public class JoyObjectNode :
        ManagedSprite,
        ITooltipComponent
    {
        public IJoyObject MyJoyObject { get; protected set; }

        protected IGUIManager GuiManager { get; set; }

        protected ManagedSprite SpeechBubble { get; set; }

        protected Area2D Collider { get; set; }

        public ICollection<string> Tooltip
        {
            get => this.MyJoyObject?.Tooltip;
            set { }
        }

        public bool MouseOver { get; protected set; }

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
            this.Move(joyObject.WorldPosition);
            if (state is null)
            {
                return;
            }

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

            var tooltip = new List<string>(this.MyJoyObject.Tooltip)
            {
                "World: " + this.MyJoyObject.MyWorld.Name
            };

            this.GuiManager.Tooltip?.Show(
                this,
                this.MyJoyObject.JoyName,
                this.MyJoyObject.States.FirstOrDefault(),
                tooltip);
        }

        public void OnPointerExit()
        {
            this.MouseOver = false;

            GlobalConstants.ActionLog.Log("MOUSE OFF " + this.MyJoyObject.JoyName);
            this.GuiManager.CloseGUI(this, GUINames.TOOLTIP);
        }

        public override void _Input(InputEvent @event)
        {
            base._Input(@event);

            if (this.MouseOver == false)
            {
                return;
            }

            if (!(@event is InputEventMouseButton mouseButton))
            {
                return;
            }

            if (mouseButton.IsActionPressed("open context menu"))
            {
                this.SetUpContextMenu();
            }
        }

        protected void SetUpContextMenu()
        {
            var contextMenu = GlobalConstants.GameManager.GUIManager.ContextMenu;
            contextMenu.Clear();

            if (this.MyJoyObject is IEntity entity)
            {
                if (entity.PlayerControlled)
                {
                    contextMenu.AddItem(
                        "Open Inventory",
                        delegate { GlobalConstants.GameManager.GUIManager.OpenGUI(this, GUINames.INVENTORY); });
                }
                else
                {
                    var player = entity.MyWorld.Player;
                    if (AdjacencyHelper.IsAdjacent(player.WorldPosition, entity.WorldPosition))
                    {
                        contextMenu.AddItem(
                            "Converse", delegate
                            {
                                var conversationWindow =
                                    GlobalConstants.GameManager.GUIManager.Get(GUINames.CONVERSATION)
                                        as ConversationWindow;
                                conversationWindow?.SetActors(player, entity);
                                GlobalConstants.GameManager.GUIManager.OpenGUI(this, conversationWindow?.Name);
                            });
                        contextMenu.AddItem(
                            "Attack", delegate { GD.Print("ATTACK!"); });
                    }
                    else
                    {
                        contextMenu.AddItem(
                            "Call Over", delegate
                            {
                                entity.FetchAction("seekaction")?.Execute(
                                    new IJoyObject[] {entity, player},
                                    new[] {"call over"},
                                    new Dictionary<string, object>
                                    {
                                        {"need", "friendship"}
                                    });
                            });
                    }
                }
            }

            this.GuiManager.OpenGUI(this, GUINames.CONTEXT_MENU);
        }
    }
}