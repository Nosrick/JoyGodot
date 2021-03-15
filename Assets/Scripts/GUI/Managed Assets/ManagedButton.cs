using System.Collections;
using System.Collections.Generic;
using JoyLib.Code.Graphics;
using JoyLib.Code.Unity.GUI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Code.Unity.GUI.Managed_Assets
{
    public class ManagedButton : 
        ManagedBackground,
        IMoveHandler,
        IPointerDownHandler, 
        IPointerUpHandler,
        IPointerEnterHandler, 
        IPointerExitHandler,
        ISelectHandler, 
        IDeselectHandler, 
        IPointerClickHandler, 
        ISubmitHandler
    {
        [SerializeField] protected ColorBlock m_ColourBlock = new ColorBlock
        {
            colorMultiplier = 1f,
            disabledColor = new Color(0.2f, 0.2f, 0.2f, 0.5f),
            fadeDuration = 0.1f,
            highlightedColor = new Color(0.8f, 0.8f, 0.8f, 1.0f),
            normalColor = Color.white,
            pressedColor = Color.gray,
            selectedColor = new Color(0.8f, 0.8f, 0.8f, 1f)
        };

        [SerializeField]
        protected AnimationTriggers m_AnimationTriggers = new AnimationTriggers();

        [SerializeField]
        protected Selectable.Transition m_Transition = Selectable.Transition.ColorTint;
        
        [SerializeField]
        protected bool m_Interactable = true;

        [SerializeField] protected bool m_Toggleable = false;

        [SerializeField]
        protected Button.ButtonClickedEvent m_OnClick = new Button.ButtonClickedEvent();

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

        protected readonly List<CanvasGroup> m_CanvasGroupCache = new List<CanvasGroup>();

        protected void OnEnable()
        {
            if (this.CurrentSpriteState is null == false)
            {
                this.DoStateTransition(this.CurrentSelectionState, false);
            }
        }

        protected void OnTransformParentChanged()
        {
            // If our parenting changes figure out if we are under a new CanvasGroup.
            this.OnCanvasGroupChanged();
        }
        
        protected void OnCanvasGroupChanged()
        {
            // Figure out if parent groups allow interaction
            // If no interaction is alowed... then we need
            // to not do that :)
            var groupAllowInteraction = true;
            Transform t = transform;
            while (t != null)
            {
                t.GetComponents(this.m_CanvasGroupCache);
                bool shouldBreak = false;
                for (var i = 0; i < this.m_CanvasGroupCache.Count; i++)
                {
                    // if the parent group does not allow interaction
                    // we need to break
                    if (!this.m_CanvasGroupCache[i].interactable)
                    {
                        groupAllowInteraction = false;
                        shouldBreak = true;
                    }
                    // if this is a 'fresh' group, then break
                    // as we should not consider parents
                    if (this.m_CanvasGroupCache[i].ignoreParentGroups)
                    {
                        shouldBreak = true;
                    }
                }
                if (shouldBreak)
                    break;

                t = t.parent;
            }

            this.m_GroupsAllowInteraction = groupAllowInteraction;
        }

        protected void DoStateTransition(SelectionState state, bool crossFade)
        {
            if (!this.gameObject.activeInHierarchy)
            {
                return;
            }

            Color tintColor;
            Sprite transitionSprite;
            string triggerName;

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
        }

        public void AddListener(UnityAction action)
        {
            this.m_OnClick.AddListener(action);
        }

        public void RemoveListener(UnityAction action)
        {
            this.m_OnClick.RemoveListener(action);
        }

        public void RemoveAllListeners()
        {
            this.m_OnClick.RemoveAllListeners();
        }

        protected virtual void DoSpriteSwap(Sprite sprite)
        {
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
        }
        
        protected virtual void TriggerAnimation(string triggername)
        {
#if PACKAGE_ANIMATION
            if (transition != Transition.Animation || animator == null || !animator.isActiveAndEnabled || !animator.hasBoundPlayables || string.IsNullOrEmpty(triggername))
                return;

            animator.ResetTrigger(m_AnimationTriggers.normalTrigger);
            animator.ResetTrigger(m_AnimationTriggers.highlightedTrigger);
            animator.ResetTrigger(m_AnimationTriggers.pressedTrigger);
            animator.ResetTrigger(m_AnimationTriggers.selectedTrigger);
            animator.ResetTrigger(m_AnimationTriggers.disabledTrigger);

            animator.SetTrigger(triggername);
#endif
        }
        
        public virtual bool IsInteractable()
        {
            return m_GroupsAllowInteraction && m_Interactable;
        }
        
        protected virtual void EvaluateAndTransitionToSelectionState()
        {
            if (!this.enabled || !this.IsInteractable())
            {
                return;
            }

            this.DoStateTransition(this.CurrentSelectionState, true);
        }

        public virtual void OnMove(AxisEventData eventData)
        {
        }

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
        
        protected virtual void Press()
        {
            if (!this.enabled || !this.IsInteractable())
            {
                return;
            }

            UISystemProfilerApi.AddMarker("Button.onClick", this);
            this.m_OnClick.Invoke();
        }
        
        protected virtual IEnumerator OnFinishSubmit()
        {
            var fadeTime = this.m_ColourBlock.fadeDuration;
            var elapsedTime = 0f;

            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

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