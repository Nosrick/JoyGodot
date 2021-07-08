using System.Collections.Generic;
using Godot;
using JoyGodot.Assets.Scripts.Helpers;

namespace JoyGodot.Assets.Scripts.Managed_Assets
{
#if TOOLS
    [Tool]
#endif
    public class ManagedButton :
        ManagedUIElement
    {
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

        public const int NORMAL = 0;
        public const int HIGHLIGHTED = 1;
        public const int SELECTED = 2;
        public const int PRESSED = 3;
        public const int DISABLED = -1;

        [Export]
        protected IDictionary<int, Color> StateColours
        {
            get
            {
                if (this.m_StateColours is null)
                {
                    this.DefaultStateColours();
                }

                return this.m_StateColours;
            }
            set => this.m_StateColours = value;
        }

        protected IDictionary<int, Color> m_StateColours;

        [Export] public float ColourMultiplier { get; set; }

        public Color NormalColour => this.StateColours[NORMAL];
        public Color HighlightedColour => this.StateColours[HIGHLIGHTED];
        public Color SelectedColour => this.StateColours[SELECTED];
        public Color PressedColour => this.StateColours[PRESSED];
        public Color DisabledColour => this.StateColours[DISABLED];

        [Signal]
        public delegate void _Press();

        [Signal]
        public delegate void _Toggle(bool newValue);

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

                return SelectionState.Normal;
            }
        }
        
        [Export]
        public string MouseOverSoundName
        {
            get;
            protected set;
        }
        
        protected AudioStream MouseOverAudioStream { get; set; }

        [Export]
        public string MouseOffSoundName
        {
            get;
            protected set;
        }
        
        protected AudioStream MouseOffAudioStream { get; set; }

        [Export]
        public string ClickSoundName
        {
            get;
            protected set;
        }
        
        protected AudioStream ClickAudioStream { get; set; }
        
        protected AudioStreamPlayer AudioPlayer { get; set; }

        public override void _Ready()
        {
            this.Initialise();
        }

        public override void Initialise()
        {
            base.Initialise();

            if (this.ClickSoundName.IsNullOrEmpty() == false)
            {
                this.ClickAudioStream = GlobalConstants.GameManager.AudioHandler.Get(this.ClickSoundName);
            }

            if (this.MouseOverSoundName.IsNullOrEmpty() == false)
            {
                this.MouseOverAudioStream = GlobalConstants.GameManager.AudioHandler.Get(this.MouseOverSoundName);
            }

            if (this.MouseOffSoundName.IsNullOrEmpty() == false)
            {
                this.MouseOffAudioStream = GlobalConstants.GameManager.AudioHandler.Get(this.MouseOffSoundName);
            }

            this.AudioPlayer = this.FindNode("AudioPlayer") as AudioStreamPlayer;
            if (this.AudioPlayer is null)
            {
                var player = new AudioStreamPlayer();
                this.AudioPlayer = player;
                this.AddChild(player);
            }
        }

        public override void _EnterTree()
        {
            base._EnterTree();

            this.Connect("mouse_entered", this, "OnPointerEnter");
            this.Connect("mouse_exited", this, "OnPointerExit");

            this.ColourMultiplier = 1f;
            this.DefaultStateColours();
        }

        public override void _ExitTree()
        {
            base._ExitTree();

            this.Disconnect("mouse_entered", this, "OnPointerEnter");
            this.Disconnect("mouse_exited", this, "OnPointerExit");
        }

        protected void DefaultStateColours()
        {
            this.m_StateColours = new Dictionary<int, Color>
            {
                {NORMAL, Colors.White},
                {HIGHLIGHTED, Colors.LightGray},
                {SELECTED, Colors.LightGray},
                {PRESSED, Colors.DarkGray},
                {DISABLED, Colors.DarkSlateGray}
            };
        }

        public override void _Draw()
        {
            this.Initialise();
            base._Draw();
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
                    tintColor = this.NormalColour;
                    //transitionSprite = null;
                    //triggerName = this.m_AnimationTriggers.normalTrigger;
                    break;
                case SelectionState.Pressed:
                    tintColor = this.PressedColour;
                    //transitionSprite = this.m_SpriteState.pressedSprite;
                    //triggerName = this.m_AnimationTriggers.pressedTrigger;
                    break;
                case SelectionState.Selected:
                    tintColor = this.SelectedColour;
                    //transitionSprite = this.m_SpriteState.selectedSprite;
                    //triggerName = this.m_AnimationTriggers.selectedTrigger;
                    break;
                case SelectionState.Disabled:
                    tintColor = this.DisabledColour;
                    //transitionSprite = this.m_SpriteState.disabledSprite;
                    //triggerName = this.m_AnimationTriggers.disabledTrigger;
                    break;
                default:
                    tintColor = Colors.Magenta;
                    //transitionSprite = null;
                    //triggerName = string.Empty;
                    break;
            }

            this.TintWithSingleColour(tintColor * this.ColourMultiplier, crossFade, 0.2f);
        }

        protected virtual void EvaluateAndTransitionToSelectionState()
        {
            if (!this.Visible || this.Disabled)
            {
                return;
            }

            this.DoStateTransition(this.CurrentSelectionState, true);
        }

        protected void PlayMouseOverSound()
        {
            if (this.AudioPlayer.Playing)
            {
                return;
            }
            this.AudioPlayer.Stream = this.MouseOverAudioStream;
            this.AudioPlayer.Play();
        }

        protected void PlayMouseOffSound()
        {
            if (this.AudioPlayer.Playing)
            {
                return;
            }

            this.AudioPlayer.Stream = this.MouseOffAudioStream;
            this.AudioPlayer.Play();
        }

        protected void PlayClickSound()
        {
            this.AudioPlayer.Stop();
            this.AudioPlayer.Stream = this.ClickAudioStream;
            this.AudioPlayer.Play();
        }

        protected override void UpdateSprites()
        {
            base.UpdateSprites();

            foreach (var part in this.Parts)
            {
                part.UseParentMaterial = false;
                part.Material = GlobalConstants.GameManager.ObjectIconHandler.UiMaterial;
            }
        }

        protected virtual void Press()
        {
            if (!this.Visible || this.Disabled || this.IsInsideTree() == false)
            {
                return;
            }

            this.EmitSignal("_Press");
            this.PlayClickSound();

            if (this.ToggleMode)
            {
                this.EmitSignal("_Toggle", this.Pressed);
            }

            this.EvaluateAndTransitionToSelectionState();
        }

        public override void _Input(InputEvent @event)
        {
            base._Input(@event);

            if (this.IsVisibleInTree() == false)
            {
                return;
            }
            
            if (@event is InputEventMouseMotion motion)
            {
                bool last = this.MouseInside;
                this.MouseInside = this.GetGlobalRect().HasPoint(motion.GlobalPosition);
                if (last != this.MouseInside)
                {
                    if (this.MouseInside)
                    {
                        this.PlayMouseOverSound();
                    }
                    else
                    {
                        this.PlayMouseOffSound();
                    }
                    this.EmitSignal(this.MouseInside ? "mouse_entered" : "mouse_exited");
                }
            }
        }

        public override void _GuiInput(InputEvent @event)
        {
            base._GuiInput(@event);

            if (this.Visible == false)
            {
                return;
            }

            if (this.MouseInside)
            {
                if (this.ActionMode == BaseButton.ActionModeEnum.Press)
                {
                    if (@event is InputEventMouseButton mouseButton
                        && (mouseButton.IsActionPressed("ui_select")
                        || mouseButton.IsActionPressed("ui_accept")))
                    {
                        if (this.ToggleMode)
                        {
                            this.Pressed = !this.Pressed;
                        }
                        else
                        {
                            this.Pressed = true;
                        }
                        this.Press();
                    }
                    else if (this.ToggleMode == false
                             && (Input.IsActionJustReleased("ui_select")
                                 || Input.IsActionJustReleased("ui_accept")))
                    {
                        this.Pressed = false;
                        this.EvaluateAndTransitionToSelectionState();
                    }
                }
                else
                {
                    if (Input.IsActionJustReleased("ui_select")
                        || Input.IsActionJustReleased("ui_accept"))
                    {
                        if (this.ToggleMode)
                        {
                            this.Pressed = !this.Pressed;
                        }
                        else
                        {
                            this.Pressed = true;
                        }
                        this.Press();
                    }
                    else if (this.ToggleMode == false
                             && (Input.IsActionJustPressed("ui_select")
                                 || Input.IsActionJustPressed("ui_accept")))
                    {
                        this.Pressed = false;
                        this.EvaluateAndTransitionToSelectionState();
                    }
                }
            }
            else
            {
                if (this.KeepPressedOutside || this.ToggleMode)
                {
                    return;
                }

                this.Pressed = false;
                this.EvaluateAndTransitionToSelectionState();
            }
        }

        public virtual void OnPointerEnter()
        {
            this.MouseInside = true;
            this.EvaluateAndTransitionToSelectionState();
        }

        public virtual void OnPointerExit()
        {
            this.MouseInside = false;
            this.EvaluateAndTransitionToSelectionState();
        }

        public override void _Process(float delta)
        {
            base._Process(delta);
            if (this.Pressed
                && this.ToggleMode == false
                && this.MouseInside == false
                && this.KeepPressedOutside == false)
            {
                this.Pressed = false;
            }
        }
    }

    public enum SelectionState
    {
        /// <summary>
        /// The UI object can be selected.
        /// </summary>
        Normal,

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
        Disabled
    }
}