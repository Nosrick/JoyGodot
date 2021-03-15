using System.Collections.Generic;
using JoyLib.Code.Graphics;
using UnityEngine;

namespace JoyLib.Code.Unity.GUI
{
    public class ManagedBackground : ManagedUISprite
    {
        public bool HasBackground { get; protected set; }
        public bool HasColours { get; protected set; }

        public override void Awake()
        {
            if (this.Initialised)
            {
                return;
            }

            base.Awake();
            this.Initialised = true;
        }
        
        public void SetBackground(ISpriteState sprite)
        {
            this.Awake();
            this.Clear();
            this.AddSpriteState(sprite);
            this.HasBackground = true;
        }

        public void SetColours(IDictionary<string, Color> colours, bool crossFade = false, float duration = 0.1f)
        {
            this.Awake();
            if (this.CurrentSpriteState is null)
            {
                GlobalConstants.ActionLog.AddText("Trying to set colours of a null sprite state. " + this.name);
                //GlobalConstants.ActionLog.AddText();
                return;
            }

            this.OverrideAllColours(colours, crossFade, duration);
            this.HasColours = true;
        }

        protected override void UpdateSprites()
        {
            base.UpdateSprites();
            for (int i = 0; i < this.ImageParts.Count; i++)
            {
                this.ImageParts[i].transform.SetSiblingIndex(i);
            }
        }
    }
}