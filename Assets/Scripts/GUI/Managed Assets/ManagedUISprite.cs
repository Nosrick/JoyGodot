using System.Collections;
using System.Collections.Generic;
using Castle.Core.Internal;
using JoyLib.Code.Graphics;
using UnityEngine;
using UnityEngine.UI;

namespace JoyLib.Code.Unity
{
    public class ManagedUISprite : ManagedSprite
    {
        protected List<Image> ImageParts { get; set; }

        public override void Awake()
        {
            this.Initialise();
        }

        protected override void Initialise()
        {
            if (this.Initialised)
            {
                return;
            }
            
            base.Initialise();
            this.ImageParts = new List<Image>();
        }

        public override void Clear()
        {
            this.Initialise();

            this.m_States = new Dictionary<string, ISpriteState>();
            foreach (Image part in this.ImageParts)
            {
                part.gameObject.SetActive(false);
            }
        }

        public override void OverrideAllColours(
            IDictionary<string, Color> colours, 
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
                    if (colours.TryGetValue(this.ImageParts[i].name, out Color colour))
                    {
                        this.StartCoroutine(
                            this.ColourLerp(
                                this.ImageParts[i].gameObject, 
                                colour, 
                                duration));
                    }
                }
            }
            else
            {
                for (int i = 0; i < this.CurrentSpriteState.SpriteData.m_Parts.Count; i++)
                {
                    this.ImageParts[i].color = this.CurrentSpriteState.SpriteData.m_Parts[i].SelectedColour; 
                }
            }
            
            this.IsDirty = true;
        }

        public override void TintWithSingleColour(
            Color colour, 
            bool crossFade = false, 
            float duration = 0.1f)
        {
            this.Initialise();

            if (this.m_States.IsNullOrEmpty())
            {
                return;
            }

            if (crossFade)
            {
                for (int i = 0; i < this.CurrentSpriteState.SpriteData.m_Parts.Count; i++)
                {
                    this.StartCoroutine(
                        this.ColourLerp(
                            this.ImageParts[i].gameObject, 
                            colour, 
                            duration, 
                            true));
                }
            }
            else
            {
                for (int i = 0; i < this.CurrentSpriteState.SpriteData.m_Parts.Count; i++)
                {
                    this.ImageParts[i].color = this.CurrentSpriteState.SpriteData.m_Parts[i].SelectedColour; 
                }
            }
            
            this.IsDirty = true;
        }
        
        protected override void UpdateSprites()
        {
            this.Initialise();

            foreach (Image spritePart in this.ImageParts)
            {
                spritePart.gameObject.SetActive(false);
            }
            if (this.ImageParts.Count < this.CurrentSpriteState.SpriteData.m_Parts.Count)
            {
                for (int i = this.ImageParts.Count; i < this.CurrentSpriteState.SpriteData.m_Parts.Count; i++)
                {
                    this.ImageParts.Add(GameObject.Instantiate(this.m_Prefab, this.transform).GetComponent<Image>());
                }
            }

            var data = this.CurrentSpriteState.GetSpriteForFrame(this.FrameIndex);
            for (int i = 0; i < data.Count; i++)
            {
                this.ImageParts[i].name = this.CurrentSpriteState.SpriteData.m_Parts[i].m_Name;
                this.ImageParts[i].gameObject.SetActive(true);
                this.ImageParts[i].sprite = data[i].Item2;
                this.ImageParts[i].color = data[i].Item1;
                this.ImageParts[i].type = this.CurrentSpriteState.SpriteData.m_Parts[i].m_ImageFillType;
            }
        }

        protected override IEnumerator ColourLerp(
            GameObject gameObject, 
            Color newColour, 
            float duration,
            params bool[] args)
        {
            Image image = gameObject.GetComponent<Image>();
            if (args.Length == 0 || args[0] == false)
            {
                Color original = image.color;

                float startTime = Time.time;
                float percentage = 0f;

                while (percentage < 1f)
                {
                    float elapsedTime = Time.time - startTime;
                    percentage = elapsedTime / duration;
                    image.color = Color.Lerp(original, newColour, percentage);
                    yield return null;
                }
            }
            else if(args[0])
            {
                image.CrossFadeColor(newColour, duration, false, true);
            }
        }
    }
}