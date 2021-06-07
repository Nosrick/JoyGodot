using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Godot;
using JoyLib.Code;
using JoyLib.Code.Graphics;
using JoyLib.Code.Helpers;

namespace JoyGodot.Assets.Scripts.GUI.Managed_Assets
{
#if TOOLS
    [Tool]
#endif
    public class ManagedLabel :
        Label,
        ISpriteStateElement,
        IColourableElement
    {
        public string ElementName
        {
            get => this.m_ElementName;
            set => this.m_ElementName = value;
        }

        [Export] protected string m_ElementName = "SlotSprite";
        
        [Export]
        public bool TitleCase
        {
            get => this.m_TitleCase;
            set
            {
                this.m_TitleCase = value;
                this.Text = this.Text;
            }
        }

        protected bool m_TitleCase;

        [Export] public bool AutoSize { get; set; }
        [Export] public bool OverrideSize { get; set; }
        [Export] public bool OverrideColour { get; set; }
        [Export] public bool OverrideOutline { get; set; }

        public bool Initialised { get; protected set; }

        protected List<NinePatchRect> Parts { get; set; }

        protected Tween TweenNode { get; set; }

        protected bool ForwardAnimation { get; set; }

        protected float TimeSinceLastChange { get; set; }

        public bool HasFont => this.Theme.DefaultFont is null == false;
        public bool HasFontColours => this.m_HasFontColours;

        protected const float TIME_BETWEEN_FRAMES = 1f / GlobalConstants.FRAMES_PER_SECOND;

        protected bool m_HasFontColours;

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

        protected string LastState { get; set; }
        protected string LastSprite { get; set; }

        protected bool Finished { get; set; }

        protected int FrameIndex { get; set; }

        protected int FramesInCurrentState { get; set; }

        public ISpriteState CurrentSpriteState
        {
            get
            {
                if (this.IsDirty)
                {
                    if (this.ChosenSprite.IsNullOrEmpty()
                        || this.ChosenState.IsNullOrEmpty())
                    {
                        return this.States.FirstOrDefault();
                    }
                    
                    if (this.m_States.ContainsKey(this.ChosenSprite)
                        && this.m_States[this.ChosenSprite].SpriteData.State
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

        protected IDictionary<string, Color> CachedColours { get; set; }

        public IEnumerable<ISpriteState> States => this.m_States.Values;

        protected IDictionary<string, ISpriteState> m_States;

        [Export]
        public Color FontColour
        {
            get => this.m_FontColour;
            set
            {
                if (this.OverrideColour == false)
                {
                    return;
                }
                
                this.m_FontColour = value;
                this.AddColorOverride("font_color", this.m_FontColour);
                this.m_HasFontColours = true;
                this.UpdateTheme();
            }
        }

        protected Color m_FontColour = Colors.White;

        [Export]
        public bool CacheFont
        {
            get => this.m_CacheFont;
            set
            {
                this.m_CacheFont = value;
                if (value)
                {
                    this.Theme.DefaultFont = this.Theme.DefaultFont.Duplicate() as DynamicFont;
                }
            }
        }

        protected bool m_CacheFont;

        [Export]
        public int FontSize
        {
            get => this.m_FontSize;
            set
            {
                this.m_FontSize = value;
                if (this.OverrideSize)
                {
                    this.UpdateTheme();
                }
            }
        }

        protected int m_FontSize;

        [Export]
        public Color OutlineColour
        {
            get => this.m_OutlineColour;
            set
            {
                this.m_OutlineColour = value;
                if (this.OverrideOutline)
                {
                    this.UpdateTheme();
                }
            }
        }

        protected Color m_OutlineColour = Colors.Black;

        [Export]
        public int OutlineThickness
        {
            get => this.m_OutlineThickness;
            set
            {
                this.m_OutlineThickness = value;
                if (this.OverrideOutline)
                {
                    this.UpdateTheme();
                }
            }
        }

        protected int m_OutlineThickness;

        [Export] public int FontMinSize { get; set; }

        [Export] public int FontMaxSize { get; set; }

        protected void UpdateTheme()
        {
            if (!(this.Theme.DefaultFont is DynamicFont defaultFont))
            {
                return;
            }

            if (this.OverrideSize && this.FontSize > 0)
            {
                defaultFont.Size = this.FontSize;
            }

            if (this.OverrideOutline)
            {
                defaultFont.OutlineColor = this.OutlineColour;
                defaultFont.OutlineSize = this.OutlineThickness;
            }

            if (this.OverrideColour)
            {
                this.AddColorOverride("font", this.FontColour);
            }
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

            if (this.IsInsideTree() == false)
            {
                this.CallDeferred("Initialise");
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
                this.TweenNode = new Tween
                {
                    Name = "Colour Lerper"
                };
                this.AddChild(this.TweenNode);
            }

            this.m_States = new Dictionary<string, ISpriteState>();
            if (this.CachedState is null == false)
            {
                this.AddSpriteState(this.CachedState);
            }

            if (this.CachedColours is null == false)
            {
                this.OverrideAllColours(this.CachedColours);
            }

            this.Initialised = true;
            
            this.OutlineColour = this.OutlineColour;
            this.OutlineThickness = this.OutlineThickness;
            this.FontColour = this.FontColour;
            this.FontSize = this.FontSize;
        }

        public virtual void AddSpriteState(ISpriteState state, bool changeToNew = true)
        {
            if (this.IsInsideTree() == false)
            {
                this.CachedState = state;
                return;
            }

            this.Initialise();
            if (this.m_States.ContainsKey(state.Name) == false)
            {
                this.m_States.Add(state.Name, state);
            }

            this.IsDirty = true;

            if (changeToNew)
            {
                this.ChangeState(state.Name);
            }
        }

        public virtual bool ChangeState(string name)
        {
            this.Initialise();

            if (!this.m_States.ContainsKey(name))
            {
                return false;
            }

            this.ChosenSprite = name;
            this.ChosenState = this.m_States[this.ChosenSprite].SpriteData.State;

            this.FramesInCurrentState = this.CurrentSpriteState.SpriteData.Parts.Max(part => part.m_Frames);

            this.FrameIndex = 0;
            this.Finished = false;

            this.UpdateSprites();

            return true;
        }

        public virtual ISpriteState GetState(string name)
        {
            return this.m_States.First(pair => pair.Key.Equals(name, StringComparison.OrdinalIgnoreCase)).Value;
        }

        public virtual bool RemoveStatesByName(string name)
        {
            return this.m_States.Remove(name);
        }

        public virtual void Clear()
        {
            if (this.IsInsideTree() == false)
            {
                return;
            }

            this.Initialise();

            this.m_States = new Dictionary<string, ISpriteState>();
            foreach (NinePatchRect part in this.Parts)
            {
                part.Visible = false;
            }
        }
        
        public override void _PhysicsProcess(float delta)
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

            if (colours.IsNullOrEmpty())
            {
                return;
            }

            if (this.CurrentSpriteState is null)
            {
                return;
            }

            foreach (ISpriteState state in this.m_States.Values)
            {
                state.OverrideColours(colours);
            }

            if (this.IsInsideTree() == false)
            {
                this.CachedColours = colours;
                return;
            }

            if (crossFade)
            {
                for (int i = 0; i < this.CurrentSpriteState.SpriteData.Parts.Count; i++)
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
                for (int i = 0; i < this.CurrentSpriteState.SpriteData.Parts.Count; i++)
                {
                    colours.TryGetValue(this.CurrentSpriteState.SpriteData.Parts[i].m_Name, out Color colour);
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

            if (this.CurrentSpriteState is null)
            {
                return;
            }

            if (crossFade)
            {
                if (modulateChildren)
                {
                    for (int i = 0; i < this.CurrentSpriteState.SpriteData.Parts.Count; i++)
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
                    for (int i = 0; i < this.CurrentSpriteState.SpriteData.Parts.Count; i++)
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

            int partsCount = this.CurrentSpriteState.SpriteData.Parts.Count;
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
                        MarginTop = 0,
                        ShowBehindParent = true,
                        UseParentMaterial = true
                    };
                    this.Parts.Add(patchRect);
                    this.AddChild(patchRect);
                }
            }

            int minSortingOrder = this.CurrentSpriteState.SpriteData.Parts.Min(part => part.m_SortingOrder);
            for (int i = 0; i < partsCount; i++)
            {
                var part = this.CurrentSpriteState.SpriteData.Parts[i];
                NinePatchRect patchRect = this.Parts[i];
                patchRect.Name = part.m_Name;
                patchRect.Visible = true;
                patchRect.Texture = part.m_FrameSprite[this.FrameIndex];
                int normaliseSortOrder = part.m_SortingOrder - minSortingOrder;
                this.MoveChild(patchRect, normaliseSortOrder);
                if (part.m_PatchMargins.IsNullOrEmpty() == false
                    && part.m_PatchMargins.Length == 4)
                {
                    patchRect.PatchMarginLeft = part.m_PatchMargins[0];
                    patchRect.PatchMarginTop = part.m_PatchMargins[1];
                    patchRect.PatchMarginRight = part.m_PatchMargins[2];
                    patchRect.PatchMarginBottom = part.m_PatchMargins[3];
                }

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