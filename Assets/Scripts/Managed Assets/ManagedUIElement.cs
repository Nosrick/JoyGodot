using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using JoyGodot.addons.Managed_Assets;
using JoyLib.Code;
using JoyLib.Code.Graphics;
using JoyLib.Code.Helpers;

namespace JoyGodot.Assets.Scripts.GUI.Managed_Assets
{
#if TOOLS
    [Tool]
#endif
    public class ManagedUIElement :
        Control,
        IColourableElement,
        ISpriteStateElement
    {
        public string ElementName
        {
            get => this.m_ElementName;
            set => this.m_ElementName = value;
        }

        [Export]
        protected string m_ElementName = "SlotSprite";
        
        public bool Initialised { get; protected set; }
        protected Color Tint { get; set; }
        public bool Finished { get; protected set; }
        protected bool ForwardAnimation { get; set; }

        public ISpriteState CurrentSpriteState
        {
            get
            {
                if (this.IsDirty)
                {
                    if (this.m_States.ContainsKey(this.ChosenSprite)
                        && this.m_States[this.ChosenSprite].SpriteData.m_State
                            .Equals(this.ChosenState, StringComparison.OrdinalIgnoreCase))
                    {
                        this.CachedState = this.m_States[this.ChosenSprite];
                        this.LastSprite = this.ChosenSprite;
                        this.LastState = this.ChosenState;
                        this.IsDirty = false;
                    }
                }

                return this.CachedState;
            }
        }

        protected bool IsDirty { get; set; }

        protected ISpriteState CachedState { get; set; }

        public int FrameIndex { get; protected set; }

        public string ChosenSprite
        {
            get => this.m_ChosenSprite;
            protected set
            {
                this.m_ChosenSprite = value;
                this.IsDirty = true;
            }
        }

        protected string m_ChosenSprite;

        public string ChosenState
        {
            get => this.m_ChosenState;
            protected set
            {
                this.m_ChosenState = value;
                this.IsDirty = true;
            }
        }

        protected string m_ChosenState;

        protected int FramesInCurrentState { get; set; }

        protected string LastState { get; set; }
        protected string LastSprite { get; set; }

        public float TimeSinceLastChange { get; protected set; }

        public IEnumerable<ISpriteState> States => this.m_States.Values;

        protected IDictionary<string, ISpriteState> m_States;

        protected List<NinePatchRect> Parts { get; set; }

        protected Tween TweenNode { get; set; }

        protected const float TIME_BETWEEN_FRAMES = 1f / GlobalConstants.FRAMES_PER_SECOND;

        public override void _Ready()
        {
            this.Initialise();
        }

        public virtual void Initialise()
        {
            if (this.Initialised || this.IsInsideTree() == false)
            {
                return;
            }

            this.Parts = new List<NinePatchRect>();
            var children = this.GetAllChildren();
            foreach (var child in children)
            {
                if (child is NinePatchRect patchRect)
                {
                    this.Parts.Add(patchRect);
                }

                if (child is Tween tween)
                {
                    this.TweenNode = tween;
                }
            }

            if (this.TweenNode is null)
            {
                //GD.Print("Tween needs to be created");
                this.TweenNode = new Tween
                {
                    Name = "Colour Lerper"
                };
                this.AddChild(this.TweenNode);
#if TOOLS
                this.TweenNode.Owner = this.GetTree()?.EditedSceneRoot;
#endif
            }

            this.m_States = new Dictionary<string, ISpriteState>();

            //GD.Print(this.Name + " initialised!");
            this.Initialised = true;
        }

        public virtual void AddSpriteState(ISpriteState state, bool changeToNew = true)
        {
            if (this.IsInsideTree() == false)
            {
                return;
            }
            
            this.Initialise();
            this.m_States.Add(state.Name, state);
            this.IsDirty = true;

            if (changeToNew)
            {
                this.ChangeState(state.Name);
            }
        }

        public virtual bool RemoveStatesByName(string name)
        {
            return this.m_States.Remove(name);
        }

        public new void SetTheme(Theme theme)
        {
            foreach (ISpriteState state in this.States)
            {
                foreach (SpritePart part in state.SpriteData.m_Parts)
                {
                    Texture icon = theme.GetIcon(part.m_Name, nameof(this.GetType));
                    if (icon is null == false)
                    {
                        part.m_FrameSprite = new List<Texture>
                        {
                            icon
                        };
                    }

                    Color colour = theme.GetColor(part.m_Name, nameof(this.GetType));
                    part.m_PossibleColours = new List<Color>
                    {
                        colour
                    };
                    part.m_SelectedColour = 0;
                }
            }
        }

        public virtual ISpriteState GetState(string name)
        {
            return this.m_States.First(pair => pair.Key.Equals(name, StringComparison.OrdinalIgnoreCase)).Value;
        }

        public virtual IEnumerable<ISpriteState> GetStatesByName(string name)
        {
            return this.m_States.Where(pair => pair.Key.Equals(name, StringComparison.OrdinalIgnoreCase))
                .Select(pair => pair.Value);
        }

        public virtual bool ChangeState(string name)
        {
            this.Initialise();

            if (!this.m_States.ContainsKey(name))
            {
                return false;
            }

            this.ChosenSprite = name;
            this.ChosenState = this.m_States[this.ChosenSprite].SpriteData.m_State;

            this.FramesInCurrentState = this.CurrentSpriteState.SpriteData.m_Parts.Max(part => part.m_Frames);

            this.FrameIndex = 0;
            this.Finished = false;

            this.UpdateSprites();

            return true;
        }

        public virtual void Clear()
        {
            if (this.IsInsideTree() == false)
            {
                return;
            }

            this.Initialise();

            this.m_States = new System.Collections.Generic.Dictionary<string, ISpriteState>();
            foreach (NinePatchRect part in this.Parts)
            {
                part.Visible = false;
            }
        }

        public override void _Process(float delta)
        {
            if (this.CurrentSpriteState is null)
            {
                return;
            }

            if (!this.CurrentSpriteState.IsAnimated || this.Finished)
            {
                return;
            }

            this.TimeSinceLastChange += delta;
            if (!(this.TimeSinceLastChange >= TIME_BETWEEN_FRAMES))
            {
                return;
            }

            int lastIndex = this.FrameIndex;
            this.TimeSinceLastChange = 0f;

            switch (this.CurrentSpriteState.AnimationType)
            {
                case AnimationType.Forward:
                    this.FrameIndex += 1;
                    if (this.FrameIndex == this.FramesInCurrentState)
                    {
                        this.FrameIndex = 0;
                        if (this.CurrentSpriteState.Looping == false)
                        {
                            this.Finished = true;
                        }
                    }

                    break;

                case AnimationType.Reverse:
                    this.FrameIndex -= 1;
                    if (this.FrameIndex < 0)
                    {
                        this.FrameIndex = this.FramesInCurrentState - 1;
                        if (this.CurrentSpriteState.Looping == false)
                        {
                            this.Finished = true;
                        }
                    }

                    break;

                case AnimationType.PingPong:
                    if (this.FrameIndex == 0)
                    {
                        this.ForwardAnimation = true;
                    }
                    else if (this.FrameIndex == this.FramesInCurrentState - 1)
                    {
                        this.ForwardAnimation = false;
                    }

                    if (this.ForwardAnimation)
                    {
                        this.FrameIndex += 1;
                    }
                    else
                    {
                        this.FrameIndex -= 1;
                    }

                    break;
            }

            if (lastIndex != this.FrameIndex)
            {
                this.UpdateSprites();
            }
        }

        public virtual void OverrideAllColours(
            IDictionary<string,
                Color> colours,
            bool crossFade = false,
            float duration = 0.1f,
            bool modulateChildren = false)
        {
            this.Initialise();

            if (this.CurrentSpriteState is null)
            {
                return;
            }

            foreach (ISpriteState state in this.m_States.Values)
            {
                state.OverrideColours(colours);
            }

            if (crossFade)
            {
                for (int i = 0; i < this.CurrentSpriteState.SpriteData.m_Parts.Count; i++)
                {
                    if (colours.TryGetValue(this.Parts[i].Name, out Color colour))
                    {
                        this.ColourLerp(
                            this.Parts[i],
                            this.Parts[i].SelfModulate,
                            colour,
                            duration);
                    }
                }
            }
            else
            {
                for (int i = 0; i < this.CurrentSpriteState.SpriteData.m_Parts.Count; i++)
                {
                    colours.TryGetValue(this.CurrentSpriteState.SpriteData.m_Parts[i].m_Name, out Color colour);
                    this.Parts[i].SelfModulate = colour;
                }
            }

            this.IsDirty = true;
        }

        public virtual void TintWithSingleColour(
            Color colour,
            bool crossFade = false,
            float duration = 0.1f,
            bool modulateChildren = false)
        {
            this.Initialise();

            this.Tint = colour;

            if (this.CurrentSpriteState is null)
            {
                return;
            }

            if (crossFade)
            {
                if (modulateChildren)
                {
                    for (int i = 0; i < this.CurrentSpriteState.SpriteData.m_Parts.Count; i++)
                    {
                        this.ColourLerp(
                            this.Parts[i],
                            this.Parts[i].SelfModulate,
                            colour,
                            duration);
                    }
                }
                else
                {
                    this.ColourLerp(
                        this,
                        this.Modulate,
                        colour,
                        duration,
                        "modulate");
                }
            }
            else
            {
                if (modulateChildren)
                {
                    for (int i = 0; i < this.CurrentSpriteState.SpriteData.m_Parts.Count; i++)
                    {
                        this.Parts[i].SelfModulate = colour;
                    }
                }
                else
                {
                    this.Modulate = colour;
                }
            }

            this.IsDirty = true;
        }

        protected virtual void UpdateSprites()
        {
            this.Initialise();

            foreach (NinePatchRect part in this.Parts)
            {
                part.Visible = false;
            }

            int partsCount = this.CurrentSpriteState.SpriteData.m_Parts.Count;
            if (this.Parts.Count < partsCount)
            {
                for (int i = this.Parts.Count; i < partsCount; i++)
                {
                    NinePatchRect patchRect = new NinePatchRect
                    {
                        Visible = false,
                        AxisStretchVertical = NinePatchRect.AxisStretchMode.Stretch,
                        AxisStretchHorizontal = NinePatchRect.AxisStretchMode.Stretch,
                        AnchorBottom = 1,
                        AnchorTop = 0,
                        AnchorLeft = 0,
                        AnchorRight = 1,
                        MarginBottom = 0,
                        MarginLeft = 0,
                        MarginRight = 0,
                        MarginTop = 0
                    };
                    this.Parts.Add(patchRect);
                    this.AddChild(patchRect);
#if TOOLS
                    patchRect.Owner = this.GetTree()?.EditedSceneRoot;
#endif
                }
            }

            int maxSortingOrder = this.CurrentSpriteState.SpriteData.m_Parts.Max(part => part.m_SortingOrder);
            int minSortingOrder = this.CurrentSpriteState.SpriteData.m_Parts.Min(part => part.m_SortingOrder);
            for (int i = 0; i < partsCount; i++)
            {
                var part = this.CurrentSpriteState.SpriteData.m_Parts[i];
                NinePatchRect patchRect = this.Parts[i];
                patchRect.Name = part.m_Name;
                patchRect.Visible = true;
                patchRect.Texture = part.m_FrameSprite[this.FrameIndex];
                int normaliseSortOrder = part.m_SortingOrder - minSortingOrder;
                this.MoveChild(patchRect, normaliseSortOrder);
                patchRect.PatchMarginLeft = part.m_PatchMargins[0];
                patchRect.PatchMarginTop = part.m_PatchMargins[1];
                patchRect.PatchMarginRight = part.m_PatchMargins[2];
                patchRect.PatchMarginBottom = part.m_PatchMargins[3];
                patchRect.DrawCenter = part.m_DrawCentre;
                patchRect.AxisStretchHorizontal = part.m_StretchMode;
                patchRect.AxisStretchVertical = part.m_StretchMode;
                patchRect.SizeFlagsHorizontal = 3;
                patchRect.SizeFlagsVertical = 3;
            }

            var array = this.GetChildren();
            int index = 1;
            foreach (object obj in array)
            {
                if (obj is NinePatchRect || !(obj is Node node))
                {
                    continue;
                }

                int sortOrder = this.Parts.Count + index;
                this.MoveChild(node, sortOrder);
                index += 1;
            }
        }

        protected virtual void ColourLerp(
            Control sprite,
            Color originalColour,
            Color newColour,
            float duration,
            string property = "self_modulate",
            bool modulateChildren = false)
        {
            if (this.IsInsideTree() == false)
            {
                return;
            }

            if (modulateChildren)
            {
                this.TweenNode.InterpolateProperty(
                    sprite,
                    property,
                    originalColour,
                    newColour,
                    duration);
                this.TweenNode.Start();

                var children = this.GetAllChildren();
                foreach (var child in children)
                {
                    if (child is Control control)
                    {
                        this.TweenNode.InterpolateProperty(
                            control,
                            property,
                            originalColour,
                            newColour,
                            duration);

                        this.TweenNode.Start();
                    }
                }
            }
            else
            {
                this.TweenNode.InterpolateProperty(
                    sprite,
                    property,
                    originalColour,
                    newColour,
                    duration);

                this.TweenNode.Start();
            }
        }
    }
}