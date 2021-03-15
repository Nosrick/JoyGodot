using System.Collections;
using Godot;
using JoyGodot.Assets.Scripts.GUI.Managed_Assets;
using JoyLib.Code;

namespace Code.Unity.GUI.Managed_Assets
{
    public class ManagedButton : 
        Button,
        IManagedElement
    {
        public string ElementName { get; protected set; }
        public bool Initialised { get; protected set; }

        protected bool m_Interactable = true;

        protected bool m_Toggleable = false;

        protected SelectionState CurrentSelectionState
        {
            get
            {
                if (!this.IsInteractable())
                {
                    return SelectionState.Disabled;
                }

                if (this.IsPointerDown || this.Toggled)
                {
                    return SelectionState.Pressed;
                }

                if (this.HasSelection)
                {
                    return SelectionState.Selected;
                }

                if (this.IsPointerInside)
                {
                    return SelectionState.Highlighted;
                }
                return SelectionState.Normal;
            }
        }

        protected bool IsPointerInside { get; set; }
        protected bool IsPointerDown { get; set; }
        protected bool HasSelection { get; set; }
        
        public bool Toggled { get; set; }
        
        protected bool m_GroupsAllowInteraction = true;

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
        
        public virtual bool IsInteractable()
        {
            return m_GroupsAllowInteraction && m_Interactable;
        }
        
        protected virtual void EvaluateAndTransitionToSelectionState()
        {
            if (!this.Visible || !this.IsInteractable())
            {
                return;
            }

            this.DoStateTransition(this.CurrentSelectionState, true);
        }

        /*
        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            // Selection tracking
            if (this.IsInteractable() && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(this.gameObject, eventData);
            }

            if (this.m_Toggleable)
            {
                this.Toggled = !this.Toggled;
            }

            this.IsPointerDown = true;
            this.EvaluateAndTransitionToSelectionState();
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            
            this.IsPointerDown = false;
            this.EvaluateAndTransitionToSelectionState();
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            this.IsPointerInside = true;
            this.EvaluateAndTransitionToSelectionState();
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            this.IsPointerInside = false;
            this.HasSelection = false;
            this.EvaluateAndTransitionToSelectionState();
        }

        public virtual void OnSelect(BaseEventData eventData)
        {
            this.HasSelection = true;
            this.EvaluateAndTransitionToSelectionState();
        }

        public virtual void OnDeselect(BaseEventData eventData)
        {
            this.HasSelection = false;
            this.EvaluateAndTransitionToSelectionState();
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            this.Press();
        }

        public virtual void OnSubmit(BaseEventData eventData)
        {
            this.Press();

            // if we get set disabled during the press
            // don't run the coroutine.
            if (!this.enabled || !this.IsInteractable())
            {
                return;
            }

            this.DoStateTransition(SelectionState.Pressed, true);
            this.StartCoroutine(this.OnFinishSubmit());
        }
        */
        
        protected virtual void Press()
        {
            if (!this.Visible || !this.IsInteractable())
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