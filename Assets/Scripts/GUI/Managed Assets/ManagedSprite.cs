using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using JoyGodot.Assets.Scripts.GUI.Managed_Assets;
using JoyLib.Code.Graphics;
using Thread = System.Threading.Thread;
using Timer = Godot.Timer;
using Array = Godot.Collections.Array;

namespace JoyLib.Code.Unity
{
    public class ManagedSprite :
        Node2D,
        IManagedElement
    {
        [Export] public string ElementName { get; protected set; }
        public bool Initialised { get; protected set; }
        protected Color Tint { get; set; }
        public bool Finished { get; protected set; }

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
        
        public IEnumerable<ISpriteState> States => this.m_States.Values;

        protected IDictionary<string, ISpriteState> m_States;
        
        protected List<Node2D> Parts { get; set; }

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
            
            this.Parts = new List<Node2D>();
            this.m_States = new System.Collections.Generic.Dictionary<string, ISpriteState>();

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
                        part.m_FrameSprite.Frames = new Array
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
            foreach (Node2D part in this.Parts)
            {
                part.Visible = false;
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

            foreach (Node2D part in this.Parts)
            {
                part.Visible = false;
            }
            if (this.Parts.Count < this.CurrentSpriteState.SpriteData.m_Parts.Count)
            {
                for (int i = this.Parts.Count; i < this.CurrentSpriteState.SpriteData.m_Parts.Count; i++)
                {
                    AnimatedSprite newSprite = new AnimatedSprite
                    {
                        Centered = true
                    };
                    this.Parts.Add(newSprite);
                    this.AddChild(newSprite);
                }
            }

            for (int i = 0; i < this.CurrentSpriteState.SpriteData.m_Parts.Count; i++)
            {
                AnimatedSprite animatedSprite = (AnimatedSprite) this.Parts[i];
                SpritePart spriteDataPart = this.CurrentSpriteState.SpriteData.m_Parts[i];
                animatedSprite.Name = spriteDataPart.m_Name;
                animatedSprite.Visible = true;
                animatedSprite.Frames = spriteDataPart.m_FrameSprite;
                animatedSprite.ZIndex = spriteDataPart.m_SortingOrder;
                animatedSprite.Play(this.CurrentSpriteState.SpriteData.m_State);
                animatedSprite.Frame = 0;
            }
        }

        protected virtual void ColourLerp(
            Node2D sprite,
            Color newColour,
            float duration)
        {
            Thread lerpThread = new Thread(this.ColourLerpForThread);
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
            public Node2D m_Sprite;
            public Color m_NewColour;
            public float m_Duration;
        }
    }
}