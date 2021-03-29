using System.Collections;
using System.Collections.Generic;
using Godot;
using JoyGodot.Assets.Scripts.GUI.Managed_Assets;
using JoyLib.Code;
using JoyLib.Code.Graphics;

namespace Code.Unity.GUI.Managed_Assets
{
    public class ManagedButton : 
        Button,
        IManagedElement
    {
        [Export] public string ElementName { get; protected set; }
        public bool Initialised { get; protected set; }
        
        protected ManagedUIElement Element { get; set; }
        
        protected bool HasFocus { get; set; }

        protected SelectionState CurrentSelectionState
        {
            get
            {
                if (this.Disabled)
                {
                    return SelectionState.Disabled;
                }

                if (this.Pressed || (this.ToggleMode && this.Pressed))
                {
                    return SelectionState.Pressed;
                }

                if (this.HasFocus)
                {
                    return SelectionState.Selected;
                }

                if (this.HasFocus)
                {
                    return SelectionState.Highlighted;
                }
                return SelectionState.Normal;
            }
        }

        public override void _Ready()
        {
            this.Element = new ManagedUIElement
            {
                AnchorBottom = 1, 
                AnchorRight = 1, 
                Name = "Background", 
                MouseFilter = MouseFilterEnum.Ignore
            };

            this.AddChild(this.Element);
            this.MoveChild(this.Element, 0);
        }

        public void Clear()
        {
            this.Element.Clear();
        }

        public void AddSpriteState(ISpriteState state)
        {
            this.Element.AddSpriteState(state);
        }

        public void OverwriteColours(IDictionary<string, Color> colours)
        {
            this.Element.OverrideAllColours(colours);
        }

        protected void DoStateTransition(SelectionState state, bool crossFade)
        {
            if (!this.Visible)
            {
                return;
            }

            Color tintColor;
            Texture transitionSprite;
            string triggerName;

            /*
            switch (state)
            {
                case SelectionState.Normal:
                    tintColor = this.m_ColourBlock.normalColor;
                    //transitionSprite = null;
                    triggerName = this.m_AnimationTriggers.normalTrigger;
                    break;
                case SelectionState.Highlighted:
                    tintColor = this.m_ColourBlock.highlightedColor;
                    //transitionSprite = this.m_SpriteState.highlightedSprite;
                    triggerName = this.m_AnimationTriggers.highlightedTrigger;
                    break;
                case SelectionState.Pressed:
                    tintColor = this.m_ColourBlock.pressedColor;
                    //transitionSprite = this.m_SpriteState.pressedSprite;
                    triggerName = this.m_AnimationTriggers.pressedTrigger;
                    break;
                case SelectionState.Selected:
                    tintColor = this.m_ColourBlock.selectedColor;
                    //transitionSprite = this.m_SpriteState.selectedSprite;
                    triggerName = this.m_AnimationTriggers.selectedTrigger;
                    break;
                case SelectionState.Disabled:
                    tintColor = this.m_ColourBlock.disabledColor;
                    //transitionSprite = this.m_SpriteState.disabledSprite;
                    triggerName = this.m_AnimationTriggers.disabledTrigger;
                    break;
                default:
                    tintColor = Color.black;
                    //transitionSprite = null;
                    triggerName = string.Empty;
                    break;
            }

            switch (this.m_Transition)
            {
                case Selectable.Transition.ColorTint:
                    this.TintWithSingleColour(tintColor * this.m_ColourBlock.colorMultiplier, crossFade);
                    break;
                case Selectable.Transition.SpriteSwap:
                    //this.DoSpriteSwap(transitionSprite);
                    break;
                case Selectable.Transition.Animation:
                    this.TriggerAnimation(triggerName);
                    break;
            }
            */
        }

        protected virtual void DoSpriteSwap(Sprite sprite)
        {
            /*
            if (sprite is null)
            {
                return;
            }
            
            this.Clear();
            this.AddSpriteState(
                new JoyLib.Code.Graphics.SpriteState(
                    "Button",
                    new SpriteData
                    {
                        m_Name = "Button",
                        m_Parts = new List<SpritePart>
                        {
                            new SpritePart
                            {
                                m_Frames = 1,
                                m_FrameSprites = new List<Sprite>
                                {
                                    sprite
                                },
                                m_ImageFillType = Image.Type.Tiled,
                                m_Name = "Button",
                                m_PossibleColours = new List<Color>
                                {
                                    Color.white
                                }
                            }
                        }
                    }));
                    */
        }
        
        protected virtual void EvaluateAndTransitionToSelectionState()
        {
            if (!this.Visible || this.Disabled)
            {
                return;
            }

            this.DoStateTransition(this.CurrentSelectionState, true);
        }

        public virtual void OnPointerDown()
        {
            // Selection tracking
            if (this.Disabled == false)
            {
            }

            GD.Print("PRESS");
            
            if (this.ToggleMode)
            {
                this.Pressed = !this.Pressed;
            }

            this.EvaluateAndTransitionToSelectionState();
        }

        public virtual void OnPointerUp()
        {
            this.EvaluateAndTransitionToSelectionState();
        }

        public virtual void OnPointerEnter()
        {
            this.EvaluateAndTransitionToSelectionState();
        }

        public virtual void OnPointerExit()
        {
            this.EvaluateAndTransitionToSelectionState();
        }

        public virtual void OnSelect()
        {
            this.EvaluateAndTransitionToSelectionState();
        }

        public virtual void OnDeselect()
        {
            this.EvaluateAndTransitionToSelectionState();
        }

        public virtual void OnPointerClick()
        {
            this.Press();
        }

        public virtual void OnSubmit()
        {
            this.Press();

            // if we get set disabled during the press
            // don't run the coroutine.
            if (!this.Visible || this.Disabled)
            {
                return;
            }

            this.DoStateTransition(SelectionState.Pressed, true);
            //this.StartCoroutine(this.OnFinishSubmit());
        }
        
        protected virtual void Press()
        {
            if (!this.Visible || this.Disabled)
            {
                return;
            }

            GlobalConstants.ActionLog.Log("Pressed " + this.Name);
        }
        
        protected virtual IEnumerator OnFinishSubmit()
        {/*
            var fadeTime = this.m_ColourBlock.fadeDuration;
            var elapsedTime = 0f;

            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }
            */

            yield return null;
            this.DoStateTransition(this.CurrentSelectionState, true);
        }
    }
    
    public enum SelectionState
    {
        /// <summary>
        /// The UI object can be selected.
        /// </summary>
        Normal,

        /// <summary>
        /// The UI object is highlighted.
        /// </summary>
        Highlighted,

        /// <summary>
        /// The UI object is pressed.
        /// </summary>
        Pressed,

        /// <summary>
        /// The UI object is selected
        /// </summary>
        Selected,

        /// <summary>
        /// The UI object cannot be selected.
        /// </summary>
        Disabled,
    }
}