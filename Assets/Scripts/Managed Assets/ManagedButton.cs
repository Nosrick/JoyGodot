using System.Collections;
using System.Collections.Generic;
using Godot;
using JoyGodot.Assets.Scripts.GUI.Managed_Assets;
using JoyLib.Code;
using JoyLib.Code.Graphics;

namespace Code.Unity.GUI.Managed_Assets
{
    [Tool]
    public class ManagedButton :
        Control,
        IColourableElement,
        ISpriteStateElement
    {
        [Export] public string ElementName { get; protected set; }
        public bool Initialised { get; protected set; }

        protected ManagedUIElement Element { get; set; }


        public bool MouseInside { get; protected set; }

        /// <summary>
        /// <para>If <c>true</c>, the button is in disabled state and can't be clicked or toggled.</para>
        /// </summary>
        [Export]
        public bool Disabled
        {
            get => this.m_Disabled;
            set
            {
                this.m_Disabled = value;
                this.EvaluateAndTransitionToSelectionState();
            }
        }

        protected bool m_Disabled;

        /// <summary>
        /// <para>If <c>true</c>, the button is in toggle mode. Makes the button flip state between pressed and unpressed each time its area is clicked.</para>
        /// </summary>
        [Export]
        public bool ToggleMode { get; set; }

        /// <summary>
        /// <para>If <c>true</c>, the button will add information about its shortcut in the tooltip.</para>
        /// </summary>
        [Export]
        public bool ShortcutInTooltip { get; set; }

        /// <summary>
        /// <para>If <c>true</c>, the button's state is pressed. Means the button is pressed down or toggled (if <see cref="P:Godot.BaseButton.ToggleMode" /> is active).</para>
        /// </summary>
        [Export]
        public bool Pressed
        {
            get => this.m_Pressed;
            set
            {
                this.m_Pressed = value;
                this.EvaluateAndTransitionToSelectionState();
            }
        }

        protected bool m_Pressed;

        /// <summary>
        /// <para>Determines when the button is considered clicked, one of the <see cref="T:Godot.BaseButton.ActionModeEnum" /> constants.</para>
        /// </summary>
        [Export]
        public BaseButton.ActionModeEnum ActionMode { get; set; }

        /// <summary>
        /// <para>Binary mask to choose which mouse buttons this button will respond to.</para>
        /// <para>To allow both left-click and right-click, use <c>BUTTON_MASK_LEFT | BUTTON_MASK_RIGHT</c>.</para>
        /// </summary>
        [Export]
        public int ButtonMask { get; set; }

        /// <summary>
        /// <para>Focus access mode to use when switching between enabled/disabled (see <see cref="P:Godot.Control.FocusMode" /> and <see cref="P:Godot.BaseButton.Disabled" />).</para>
        /// </summary>
        [Export]
        public Control.FocusModeEnum EnabledFocusMode { get; set; }

        /// <summary>
        /// <para>If <c>true</c>, the button stays pressed when moving the cursor outside the button while pressing it.</para>
        /// <para>Note: This property only affects the button's visual appearance. Signals will be emitted at the same moment regardless of this property's value.</para>
        /// </summary>
        [Export]
        public bool KeepPressedOutside { get; set; }

        /// <summary>
        /// <para><see cref="T:Godot.ShortCut" /> associated to the button.</para>
        /// </summary>
        [Export]
        public ShortCut Shortcut { get; set; }

        /// <summary>
        /// <para><see cref="T:Godot.ButtonGroup" /> associated to the button.</para>
        /// </summary>
        [Export]
        public ButtonGroup Group { get; set; }

        [Export] public ColourBlock StateColours { get; set; }

        public delegate void _Press();

        public delegate void _Toggle();

        protected SelectionState CurrentSelectionState
        {
            get
            {
                if (this.Disabled)
                {
                    return SelectionState.Disabled;
                }

                if (this.Pressed)
                {
                    return SelectionState.Pressed;
                }

                if (this.MouseInside)
                {
                    return SelectionState.Selected;
                }

                if (this.MouseInside)
                {
                    return SelectionState.Highlighted;
                }

                return SelectionState.Normal;
            }
        }

        public override void _Ready()
        { }

        public override void _EnterTree()
        {
            base._EnterTree();
            this.Element = this.GetNode<ManagedUIElement>("Element");
            if (this.Element is null)
            {
                GD.Print("Creating managed UI element");
                this.Element = new ManagedUIElement
                {
                    AnchorBottom = 1,
                    AnchorRight = 1,
                    Name = "Element",
                    MouseFilter = MouseFilterEnum.Ignore
                };

                this.AddChild(this.Element);
                this.MoveChild(this.Element, 0);
            }
        }

        public virtual void AddSpriteState(ISpriteState state, bool changeToNew = true)
        {
            this.Element.AddSpriteState(state, changeToNew);
        }

        public bool RemoveStatesByName(string name)
        {
            return this.Element.RemoveStatesByName(name);
        }

        public ISpriteState GetState(string name)
        {
            return this.Element.GetState(name);
        }

        public bool ChangeState(string name)
        {
            return this.Element.ChangeState(name);
        }

        public void Clear()
        {
            this.Element.Clear();
        }

        public void OverrideAllColours(IDictionary<string, Color> colours, bool crossFade = false,
            float duration = 0.1f)
        {
            this.Element.OverrideAllColours(colours, crossFade, duration);
        }

        public void TintWithSingleColour(Color colour, bool crossFade = false, float duration = 0.1f)
        {
            this.Element.TintWithSingleColour(colour, crossFade, duration);
        }

        protected void DoStateTransition(SelectionState state, bool crossFade)
        {
            if (!this.Visible)
            {
                return;
            }

            Color tintColor;

            switch (state)
            {
                case SelectionState.Normal:
                    tintColor = this.StateColours.Normal;
                    //transitionSprite = null;
                    //triggerName = this.m_AnimationTriggers.normalTrigger;
                    break;
                case SelectionState.Highlighted:
                    tintColor = this.StateColours.Highlighted;
                    //transitionSprite = this.m_SpriteState.highlightedSprite;
                    //triggerName = this.m_AnimationTriggers.highlightedTrigger;
                    break;
                case SelectionState.Pressed:
                    tintColor = this.StateColours.Pressed;
                    //transitionSprite = this.m_SpriteState.pressedSprite;
                    //triggerName = this.m_AnimationTriggers.pressedTrigger;
                    break;
                case SelectionState.Selected:
                    tintColor = this.StateColours.Selected;
                    //transitionSprite = this.m_SpriteState.selectedSprite;
                    //triggerName = this.m_AnimationTriggers.selectedTrigger;
                    break;
                case SelectionState.Disabled:
                    tintColor = this.StateColours.Disabled;
                    //transitionSprite = this.m_SpriteState.disabledSprite;
                    //triggerName = this.m_AnimationTriggers.disabledTrigger;
                    break;
                default:
                    tintColor = Colors.Magenta;
                    //transitionSprite = null;
                    //triggerName = string.Empty;
                    break;
            }

            this.TintWithSingleColour(tintColor * this.StateColours.ColourMultiplier, crossFade);
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

        protected virtual void Press()
        {
            if (!this.Visible || this.Disabled)
            {
                return;
            }

            GD.Print("PRESS");
            this.EmitSignal("_Press");

            if (this.ToggleMode)
            {
                this.Pressed = !this.Pressed;
                this.EmitSignal("_Toggle");
            }
            else
            {
                this.Pressed = false;
            }

            this.EvaluateAndTransitionToSelectionState();

            GlobalConstants.ActionLog.Log("Pressed " + this.Name);
        }

        public override void _GuiInput(InputEvent @event)
        {
            base._GuiInput(@event);
            if (@event is InputEventMouseMotion motion)
            {
                if (this.GetRect().HasPoint(motion.Position) && this.MouseInside == false)
                {
                    this.MouseInside = true;
                }
                else if (this.MouseInside)
                {
                    this.MouseInside = false;
                }
            }

            if (@event is InputEventAction action)
            {
                if (this.MouseInside
                    && (action.IsActionPressed("ui_select")
                        || action.IsActionPressed("ui_accept")))
                {
                    this.Pressed = !this.Pressed;

                    if (this.Pressed)
                    {
                        this.Press();
                    }
                }
            }
        }

        protected virtual IEnumerator OnFinishSubmit()
        {
            /*
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

    public class ColourBlock
    {
        public const int NORMAL = 0;
        public const int HIGHLIGHTED = 1;
        public const int SELECTED = 2;
        public const int PRESSED = 3;
        public const int DISABLED = int.MaxValue;

        [Export] protected IDictionary<int, Color> StateColours { get; set; }

        [Export] public float ColourMultiplier { get; set; }

        public Color Normal => this.StateColours[NORMAL];
        public Color Highlighted => this.StateColours[HIGHLIGHTED];
        public Color Selected => this.StateColours[SELECTED];
        public Color Pressed => this.StateColours[PRESSED];
        public Color Disabled => this.StateColours[DISABLED];

        public ColourBlock()
            : this(
                1f,
                new Dictionary<int, Color>
                {
                    {NORMAL, Colors.White},
                    {HIGHLIGHTED, Colors.LightGray},
                    {SELECTED, Colors.LightGray},
                    {PRESSED, Colors.DarkGray},
                    {DISABLED, Colors.DarkSlateGray}
                })
        { }

        public ColourBlock(float colourMultiplier)
            : this(
                colourMultiplier,
                new Dictionary<int, Color>
                {
                    {NORMAL, Colors.White},
                    {HIGHLIGHTED, Colors.LightGray},
                    {SELECTED, Colors.LightGray},
                    {PRESSED, Colors.DarkGray},
                    {DISABLED, Colors.DarkSlateGray}
                })
        { }

        public ColourBlock(
            float colourMultiplier,
            IDictionary<int, Color> stateColours)
        {
            this.ColourMultiplier = colourMultiplier;
            this.StateColours = stateColours;
        }

        public bool Add(int code, Color colour)
        {
            if (this.StateColours.ContainsKey(code))
            {
                return false;
            }

            this.StateColours.Add(code, colour);
            return true;
        }

        public bool Remove(int code)
        {
            return this.StateColours.Remove(code);
        }

        public Color Get(int code)
        {
            if (!this.StateColours.TryGetValue(code, out Color colour))
            {
                GD.PushWarning("No colour found for code: " + code);
                return Colors.Magenta;
            }

            return colour;
        }
    }
}