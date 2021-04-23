using System.Collections.Generic;
using Castle.Core.Internal;
using Godot;
using JoyGodot.Assets.Scripts.GUI.Managed_Assets;
using JoyLib.Code;
using JoyLib.Code.Graphics;
using NUnit.Framework.Internal.Execution;

namespace Code.Unity.GUI.Managed_Assets
{
#if TOOLS
    [Tool]
#endif
    public class ManagedButton :
        Control,
        IColourableElement,
        ISpriteStateElement
    {
        [Export] public string ElementName { get; protected set; }
        public bool Initialised { get; protected set; }

        protected ManagedUIElement Element
        {
            get
            {
                if (this.m_Element is null)
                {
                    this.Initialise();
                }

                return this.m_Element;
            }
            set => this.m_Element = value;
        }

        protected ManagedUIElement m_Element;

        protected static PackedScene ElementPrefab { get; set; }

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

                return SelectionState.Normal;
            }
        }

        public override void _Ready()
        {
            this.Initialise();
        }

        public override void _EnterTree()
        {
            base._EnterTree();

            if (ElementPrefab is null)
            {
                GD.Print("Attempting to load ElementPrefab.");
                ElementPrefab = GD.Load<PackedScene>(
                    GlobalConstants.GODOT_ASSETS_FOLDER +
                    "Scenes/Parts/ManagedUIElement.tscn");

                GD.Print(ElementPrefab is null
                    ? "Could not load ElementPrefab!"
                    : "Got ElementPrefab.");
            }

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

            /*
            foreach (var obj in this.GetChildren())
            {
                if (obj is Node node)
                {
                    node.QueueFree();
                }
            }
            */
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

        public virtual void Initialise()
        {
            if (this.Initialised)
            {
                return;
            }

            if (this.Theme is null)
            {
                this.Theme = new Theme();
            }
            
            var child = this.GetNodeOrNull("Element");

            if (child is null)
            {
                //GD.Print("No element!");
            }
            else if (child is ManagedUIElement element)
            {
                //GD.Print("Element found!");
                //GD.Print(element.GetType().Name);
                this.Element = element;
            }
            else
            {
                //GD.Print("Element found, but not correct type!");
                //GD.Print(child.GetType().Name);
            }

            if (this.m_Element is null)
            {
                GD.Print("Creating managed UI element");
                if (ElementPrefab is null)
                {
                    //GD.Print("ManagedUIElement prefab is null, creating from code node!");
                    //GD.Print("Prepare for trouble!");
                    this.m_Element = new ManagedUIElement();
                }
                else
                {
                    this.m_Element = ElementPrefab.Instance() as ManagedUIElement;

                    if (this.m_Element is null)
                    {
                        //GD.Print("Failed to instantiate ManagedUIElement from Prefab!");
                        return;
                    }
                }

                this.m_Element.Name = "Element";
                this.m_Element.AnchorBottom = 1;
                this.m_Element.AnchorRight = 1;
                this.m_Element.MouseFilter = MouseFilterEnum.Ignore;
                this.AddChild(this.m_Element);
#if TOOLS
                this.m_Element.Owner = this.GetTree()?.EditedSceneRoot;
#endif
                this.MoveChild(this.m_Element, 0);
            }

            //GD.Print("Finished initialising " + this.Name);
            //GD.Print(this.GetChildren());
            this.Initialised = true;
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

        public void OverrideAllColours(
            IDictionary<string, Color> colours,
            bool crossFade = false,
            float duration = 0.1f,
            bool modulateChildren = false)
        {
            this.Element.OverrideAllColours(colours, crossFade, duration, modulateChildren);
        }

        public void TintWithSingleColour(
            Color colour,
            bool crossFade = false,
            float duration = 0.1f,
            bool modulateChildren = false)
        {
            this.Element.TintWithSingleColour(colour, crossFade, duration, modulateChildren);
        }

        protected void DoStateTransition(SelectionState state, bool crossFade)
        {
            if (!this.Visible)
            {
                return;
            }

            Color tintColor;

            GD.Print(this.Name + " transitioning to " + state);

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

        protected virtual void Press()
        {
            if (!this.Visible || this.Disabled || this.IsInsideTree() == false)
            {
                return;
            }

            this.EmitSignal("_Press");

            if (this.ToggleMode)
            {
                this.Pressed = !this.Pressed;
                this.EmitSignal("_Toggle");
            }

            this.EvaluateAndTransitionToSelectionState();

            GD.Print("Pressed " + this.Name);
        }

        public override void _GuiInput(InputEvent @event)
        {
            base._GuiInput(@event);

            if (@event is InputEventMouseMotion motion)
            {
                bool last = this.MouseInside;
                this.MouseInside = this.GetGlobalRect().HasPoint(motion.GlobalPosition);
                if (last != this.MouseInside)
                {
                    this.EmitSignal(this.MouseInside ? "mouse_entered" : "mouse_exited");
                }
            }

            if (this.MouseInside)
            {
                if (this.ActionMode == BaseButton.ActionModeEnum.Press)
                {
                    if (Input.IsActionJustPressed("ui_select")
                        || Input.IsActionJustPressed("ui_accept"))
                    {
                        this.Pressed = true;
                        this.Press();
                    }
                    else if (Input.IsActionJustReleased("ui_select")
                        || Input.IsActionJustReleased("ui_accept"))
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
                        this.Pressed = true;
                        this.Press();
                    }
                    else if (Input.IsActionJustPressed("ui_select")
                             || Input.IsActionJustPressed("ui_accept"))
                    {
                        this.Pressed = false;
                        this.EvaluateAndTransitionToSelectionState();
                    }
                }
            }
            else
            {
                if (this.KeepPressedOutside)
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
                && (this.MouseInside == false
                && this.KeepPressedOutside == false))
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