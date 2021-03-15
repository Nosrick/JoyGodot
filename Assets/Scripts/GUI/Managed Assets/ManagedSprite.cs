using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using JoyLib.Code.Graphics;
using JoyLib.Code.Unity.GUI.Managed_Assets;
using UnityEngine;

namespace JoyLib.Code.Unity
{
    [RequireComponent(typeof(RectTransform))]
    public class ManagedSprite : 
        ManagedElement, 
        IAnimated
    {
        [SerializeField] protected GameObject m_Prefab;

        protected string SortingLayer { get; set; }
        
        protected RectTransform MyRect { get; set; }
        
        protected Color Tint { get; set; }
        public bool Finished { get; protected set; }
        protected bool ForwardAnimation { get; set; }

        protected const string _TINT_COLOUR = "_TintColour";

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
        
        public string TileSet { get; protected set; }
        public float TimeSinceLastChange { get; protected set; }
        
        public IEnumerable<ISpriteState> States => this.m_States.Values;

        protected IDictionary<string, ISpriteState> m_States;
        
        protected List<SpriteRenderer> SpriteParts { get; set; }

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
            
            this.SpriteParts = new List<SpriteRenderer>();
            this.m_States = new Dictionary<string, ISpriteState>();
            this.MyRect = this.GetComponent<RectTransform>();

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

        public virtual void SetSpriteLayer(string layerName)
        {
            this.Initialise();

            this.SortingLayer = layerName;
            foreach (SpriteRenderer spriteRenderer in this.SpriteParts)
            {
                spriteRenderer.sortingLayerName = layerName;
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

            this.m_States = new Dictionary<string, ISpriteState>();
            foreach (SpriteRenderer part in this.SpriteParts)
            {
                part.gameObject.SetActive(false);
            }
        }

        public virtual void FixedUpdate()
        {
            if (this.CurrentSpriteState is null)
            {
                return;
            }
            
            if (!this.CurrentSpriteState.IsAnimated || this.Finished)
            {
                return;
            }
            
            this.TimeSinceLastChange += Time.unscaledDeltaTime;
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

            if (this.isActiveAndEnabled == false)
            {
                return;
            }
            
            if (crossFade)
            {
                for (int i = 0; i < this.CurrentSpriteState.SpriteData.m_Parts.Count; i++)
                {
                    if (colours.TryGetValue(this.SpriteParts[i].name, out Color colour))
                    {
                        this.StartCoroutine(
                            this.ColourLerp(
                                this.SpriteParts[i].gameObject, 
                                colour, 
                                duration));
                    }
                }
            }
            else
            {
                for (int i = 0; i < this.CurrentSpriteState.SpriteData.m_Parts.Count; i++)
                {
                    this.SpriteParts[i].color = this.CurrentSpriteState.SpriteData.m_Parts[i].SelectedColour; 
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
                    this.StartCoroutine(this.ColourLerp(this.SpriteParts[i].gameObject, colour, duration));
                }
            }
            else
            {
                for (int i = 0; i < this.CurrentSpriteState.SpriteData.m_Parts.Count; i++)
                {
                    this.SpriteParts[i].color = this.CurrentSpriteState.SpriteData.m_Parts[i].SelectedColour; 
                }
            }
            
            this.IsDirty = true;
        }

        protected virtual void UpdateSprites()
        {
            this.Initialise();

            foreach (SpriteRenderer spritePart in this.SpriteParts)
            {
                spritePart.gameObject.SetActive(false);
            }
            if (this.SpriteParts.Count < this.CurrentSpriteState.SpriteData.m_Parts.Count)
            {
                for (int i = this.SpriteParts.Count; i < this.CurrentSpriteState.SpriteData.m_Parts.Count; i++)
                {
                    this.SpriteParts.Add(GameObject.Instantiate(this.m_Prefab, this.transform).GetComponent<SpriteRenderer>());
                }
            }

            var data = this.CurrentSpriteState.GetSpriteForFrame(this.FrameIndex);
            for (int i = 0; i < data.Count; i++)
            {
                SpriteRenderer spriteRenderer = this.SpriteParts[i];
                if (this.SortingLayer.IsNullOrEmpty() == false)
                {
                    spriteRenderer.sortingLayerName = this.SortingLayer;
                }
                spriteRenderer.name = this.CurrentSpriteState.SpriteData.m_Parts[i].m_Name;
                spriteRenderer.gameObject.SetActive(true);
                spriteRenderer.sprite = data[i].Item2;
                spriteRenderer.sortingOrder = this.CurrentSpriteState.SpriteData.m_Parts[i].m_SortingOrder;
                spriteRenderer.sortingLayerName = this.SortingLayer;
                spriteRenderer.drawMode = this.CurrentSpriteState.SpriteData.m_Parts[i].m_SpriteDrawMode;
                Material material = spriteRenderer.material;
                material.SetColor(_TINT_COLOUR, data[i].Item1);
            }
        }

        protected virtual IEnumerator ColourLerp(GameObject gameObject,
            Color newColour,
            float duration,
            params bool[] args)
        {
            yield return null;
        }
    }
}