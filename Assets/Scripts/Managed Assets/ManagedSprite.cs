using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using JoyGodot.Assets.Scripts.Base_Interfaces;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Events;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Settings;

namespace JoyGodot.Assets.Scripts.Managed_Assets
{
    public class ManagedSprite :
        Node2D,
        IColourableElement,
        ISpriteStateElement,
        IPosition
    {
        [Export] 
        public string ElementName
        {
            get => this.m_ElementName;
            set => this.m_ElementName = value;
        }

        protected string m_ElementName;
        public bool Initialised { get; protected set; }
        protected Color Tint { get; set; }
        public bool Finished { get; protected set; }

        protected Tween TweenNode { get; set; }

        public ISpriteState CurrentSpriteState
        {
            get
            {
                if (this.IsDirty)
                {
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

        public Vector2Int WorldPosition { get; protected set; }

        protected bool EnableHappiness { get; set; }
        
        protected static IEntity Player { get; set; }

        public override void _Ready()
        {
            this.Initialise();
        }

        protected void GrabPlayer()
        {
            if (Player is null)
            {
                Player = GlobalConstants.GameManager?.Player;

                if (Player is null)
                {
                    return;
                }
                
                Player.HappinessChange -= this.SetHappiness;
                Player.HappinessChange += this.SetHappiness;

                GlobalConstants.GameManager.SettingsManager.ValueChanged -= this.SettingChanged;
                GlobalConstants.GameManager.SettingsManager.ValueChanged += this.SettingChanged;
                
                this.EnableHappiness = (bool) GlobalConstants.GameManager.SettingsManager
                    .Get(SettingsManager.HAPPINESS_WORLD)
                    .ObjectValue;
            
                this.SetHappiness(this, new ValueChangedEventArgs<float>
                {
                    NewValue = Player.OverallHappiness
                });
            }
        }

        protected void SettingChanged(object sender, ValueChangedEventArgs<object> args)
        {
            if (args.Name.Equals(SettingsManager.HAPPINESS_WORLD))
            {
                this.EnableHappiness = (bool) args.NewValue;
                this.SetHappiness(this, new ValueChangedEventArgs<float>
                {
                    NewValue = Player.OverallHappiness
                });
            }
        }

        protected void SetHappiness(object sender, ValueChangedEventArgs<float> args)
        {
            float happiness = this.EnableHappiness ? args.NewValue : 1f;

            if (this.Material is ShaderMaterial shaderMaterial)
            {
                shaderMaterial.SetShaderParam("happiness", happiness);
                foreach (ShaderMaterial childMaterial in this.Parts.Select(part => part.Material as ShaderMaterial))
                {
                    childMaterial?.SetShaderParam("happiness", happiness);
                }
            }
        }

        public virtual void Initialise()
        {
            if (this.Initialised)
            {
                return;
            }

            this.Parts = new List<Node2D>();
            this.m_States = new Dictionary<string, ISpriteState>();
            this.TweenNode = this.GetNodeOrNull<Tween>("Tween");

            if (this.TweenNode is null)
            {
                this.TweenNode = new Tween();
                this.AddChild(this.TweenNode);
            }
            
            this.GrabPlayer();

            this.Initialised = true;
        }

        public virtual void AddSpriteState(ISpriteState state, bool changeToNew = true)
        {
            this.GrabPlayer();
            
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

            if (this.m_States.ContainsKey(name) == false)
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
            IDictionary<string, Color> colours,
            bool crossFade = false,
            float duration = 0.1f,
            bool modulateChildren = false)
        {
            this.Initialise();

            foreach (ISpriteState state in this.m_States.Values)
            {
                state.OverrideColours(colours);
            }

            if (crossFade)
            {
                for (int i = 0; i < this.CurrentSpriteState.SpriteData.Parts.Count; i++)
                {
                    if (colours.TryGetValue(this.Parts[i].Name, out Color colour))
                    {
                        this.ColourLerp(
                            this.Parts[i],
                            this.Modulate,
                            colour,
                            duration);
                    }
                }
            }
            else
            {
                for (int i = 0; i < this.CurrentSpriteState.SpriteData.Parts.Count; i++)
                {
                    this.Parts[i].Modulate = this.CurrentSpriteState.SpriteData.Parts[i].SelectedColour;
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

            if (crossFade)
            {
                for (int i = 0; i < this.CurrentSpriteState.SpriteData.Parts.Count; i++)
                {
                    this.ColourLerp(
                        this.Parts[i], 
                        this.Modulate, 
                        colour, 
                        duration,
                        "modulate");
                }
            }
            else
            {
                for (int i = 0; i < this.CurrentSpriteState.SpriteData.Parts.Count; i++)
                {
                    this.Parts[i].Modulate = this.CurrentSpriteState.SpriteData.Parts[i].SelectedColour;
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

            if (this.Parts.Count < this.CurrentSpriteState.SpriteData.Parts.Count)
            {
                for (int i = this.Parts.Count; i < this.CurrentSpriteState.SpriteData.Parts.Count; i++)
                {
                    AnimatedSprite newSprite = new AnimatedSprite
                    {
                        Centered = true,
                        UseParentMaterial = true
                    };
                    this.Parts.Add(newSprite);
                    this.AddChild(newSprite);
                }
            }

            int minSortingOrder = this.CurrentSpriteState.SpriteData.Parts.Min(part => part.m_SortingOrder);
            for (int i = 0; i < this.CurrentSpriteState.SpriteData.Parts.Count; i++)
            {
                AnimatedSprite animatedSprite = (AnimatedSprite) this.Parts[i];
                SpritePart spriteDataPart = this.CurrentSpriteState.SpriteData.Parts[i];
                animatedSprite.Name = spriteDataPart.m_Name;
                animatedSprite.Visible = true;
                SpriteFrames frames = new SpriteFrames();
                if(frames.HasAnimation(this.CurrentSpriteState.SpriteData.State) == false)
                {
                    frames.AddAnimation(this.CurrentSpriteState.SpriteData.State);
                }
                foreach (var texture in spriteDataPart.m_FrameSprite)
                {
                    frames.AddFrame(this.CurrentSpriteState.SpriteData.State, texture);
                }
                
                animatedSprite.Frames = frames;
                int normaliseSortOrder = spriteDataPart.m_SortingOrder - minSortingOrder;
                this.MoveChild(animatedSprite, normaliseSortOrder);
                animatedSprite.ZIndex = spriteDataPart.m_SortingOrder;
                animatedSprite.Play(this.CurrentSpriteState.SpriteData.State);
                animatedSprite.Frame = 0;
                animatedSprite.Modulate = spriteDataPart.SelectedColour;
            }
        }

        protected virtual void ColourLerp(
            Node2D sprite,
            Color originalColour,
            Color newColour,
            float duration,
            string property = "modulate",
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
                    if (child is Node2D node)
                    {
                        this.TweenNode.InterpolateProperty(
                            node,
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
        
        public virtual void Move(Vector2Int position)
        {
            this.WorldPosition = position;
            this.Position = (this.WorldPosition * GlobalConstants.SPRITE_WORLD_SIZE).ToVec2();
        }

        public override void _ExitTree()
        {
            if (Player is null == false)
            {
                Player.HappinessChange -= this.SetHappiness;
            }

            GlobalConstants.GameManager.SettingsManager.ValueChanged -= this.SettingChanged;

            base._ExitTree();
        }
    }
}