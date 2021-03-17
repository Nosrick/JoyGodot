using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using JoyLib.Code;
using JoyLib.Code.Graphics;
using Thread = System.Threading.Thread;

namespace JoyGodot.Assets.Scripts.GUI.Managed_Assets
{
    public class ManagedUIElement : Control, IManagedElement
    {
        [Export] public string ElementName { get; protected set; }
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
                        && this.m_States[this.ChosenSprite].SpriteData.m_State.Equals(this.ChosenState, StringComparison.OrdinalIgnoreCase))
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

        protected const float TIME_BETWEEN_FRAMES = 1f / GlobalConstants.FRAMES_PER_SECOND;
        
        public virtual void Awake()
        {
            this.Initialise();
        }

        protected virtual void Initialise()
        {
            if (this.Initialised)
            {
                return;
            }
            
            this.Parts = new List<NinePatchRect>();
            this.m_States = new Dictionary<string, ISpriteState>();

            this.Initialised = true;
        }
        
        public virtual void AddSpriteState(ISpriteState state, bool changeToNew = true)
        {
            this.Initialise();
            this.m_States.Add(state.Name, state);
            this.IsDirty = true;

            if (changeToNew)
            {
                this.ChangeState(state);
            }
        }

        public virtual bool RemoveStatesByName(string name)
        {
            return this.m_States.Remove(name);
        }
        
        public void SetTheme(Theme theme)
        {
            foreach (ISpriteState state in this.States)
            {
                foreach (SpritePart part in state.SpriteData.m_Parts)
                {
                    Texture icon = theme.GetIcon(part.m_Name, "SpritePart");
                    if (icon is null == false)
                    {
                        part.m_FrameSprite.Frames = new Godot.Collections.Array
                        {
                            icon
                        };
                    }
                    
                    Color colour = theme.GetColor(part.m_Name, "SpritePart");
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

        public virtual void ChangeState(string name)
        {
            this.Initialise();

            if (this.m_States.ContainsKey(name))
            {
                this.ChosenSprite = name;
                this.ChosenState = this.m_States[this.ChosenSprite].SpriteData.m_State;

                this.FramesInCurrentState = this.CurrentSpriteState.SpriteData.m_Parts.Max(part => part.m_Frames);

                this.FrameIndex = 0;
                this.Finished = false;
                
                this.UpdateSprites();
            }
        }

        public virtual void ChangeState(ISpriteState state)
        {
            this.ChangeState(state.Name);
        }

        public virtual void Clear()
        {
            this.Initialise();

            this.m_States = new System.Collections.Generic.Dictionary<string, ISpriteState>();
            foreach (Control part in this.Parts)
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
            float duration = 0.1f)
        {
            this.Initialise();

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
                            colour, 
                            duration);
                    }
                }
            }
            else
            {
                for (int i = 0; i < this.CurrentSpriteState.SpriteData.m_Parts.Count; i++)
                {
                    this.Parts[i].SelfModulate = this.CurrentSpriteState.SpriteData.m_Parts[i].SelectedColour; 
                }
            }
            this.IsDirty = true;
        }

        public virtual void TintWithSingleColour(
            Color colour, 
            bool crossFade = false, 
            float duration = 0.1f)
        {
            this.Initialise();

            this.Tint = colour;
            
            if (crossFade)
            {
                for (int i = 0; i < this.CurrentSpriteState.SpriteData.m_Parts.Count; i++)
                {
                    this.ColourLerp(this.Parts[i], colour, duration);
                }
            }
            else
            {
                for (int i = 0; i < this.CurrentSpriteState.SpriteData.m_Parts.Count; i++)
                {
                    this.Parts[i].SelfModulate = this.CurrentSpriteState.SpriteData.m_Parts[i].SelectedColour; 
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
            if (this.Parts.Count < this.CurrentSpriteState.SpriteData.m_Parts.Count)
            {
                for (int i = this.Parts.Count; i < this.CurrentSpriteState.SpriteData.m_Parts.Count; i++)
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
                }
            }

            for (int i = 0; i < this.CurrentSpriteState.SpriteData.m_Parts.Count; i++)
            {
                var part = this.CurrentSpriteState.SpriteData.m_Parts[i];
                NinePatchRect patchRect = this.Parts[i];
                patchRect.Name = part.m_Name;
                patchRect.Visible = true;
                patchRect.Texture = part.m_FrameSprite.GetFrame(this.CurrentSpriteState.SpriteData.m_State, 0);
                this.MoveChild(patchRect, part.m_SortingOrder);
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
        }

        protected virtual void ColourLerp(
            NinePatchRect sprite,
            Color newColour,
            float duration)
        {
            System.Threading.Thread lerpThread = new Thread(this.ColourLerpForThread);
            lerpThread.Start(new ThreadLerpParams
            {
                m_Duration = duration,
                m_NewColour = newColour,
                m_Sprite = sprite
            });
        }

        protected void ColourLerpForThread(object param)
        {
            if (param is ThreadLerpParams lerpParams)
            {
                Timer timer = new Timer
                {
                    OneShot = true
                };
                timer.Start(lerpParams.m_Duration);
                float lerp = 0f;
                while (lerp < 1f)
                {
                    lerpParams.m_Sprite.SelfModulate.LinearInterpolate(lerpParams.m_NewColour, lerp);
                    lerp = timer.WaitTime / lerpParams.m_Duration;
                }
                timer.Stop();
            }
        }

        public struct ThreadLerpParams
        {
            public NinePatchRect m_Sprite;
            public Color m_NewColour;
            public float m_Duration;
        }
    }
}