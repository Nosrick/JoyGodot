using System.Collections.Generic;
using Godot;
using Godot.Collections;
using JoyGodot.Assets.Scripts.GUI.Managed_Assets;

namespace JoyLib.Code.Unity.GUI
{
    public class ManagedFonts : Node2D, IManagedElement
    {
        public string ElementName { get; protected set; }
        public bool Initialised { get; protected set; }
        
        protected bool m_OverrideSize = true;
        protected bool m_OverrideColour = true;
        protected bool m_OverrideOutline = true;
        
        protected Array Texts { get; set; }
        
        public bool HasFont { get; protected set; }
        public bool HasFontColours { get; protected set; }

        public virtual void Awake()
        {
            this.Texts = this.GetChildren();
            this.Initialised = true;
        }

        public void SetTheme(Theme theme)
        {
            if (this.Initialised == false)
            {
                this.Awake();
            }

            foreach (var text in this.Texts)
            {
                if (text is Control control)
                {
                    control.Theme = theme;
                }
            }
        }

        public void SetFonts(Font font)
        {
            if (this.Initialised == false)
            {
                this.Awake();
            }
            /*
            foreach (var text in this.Texts)
            {
                text.font = font;
            }
            */

            this.HasFont = true;
        }

        public void SetFontColour(Color colour)
        {
            if (this.Initialised == false)
            {
                this.Awake();
            }

            if (this.m_OverrideColour == false)
            {
                return;
            }
            
            /*
            foreach (var text in this.Texts)
            {
                text.color = colour;
            }
            */

            this.HasFontColours = true;
        }

        public void SetOutline(Color colour, float thickness = 0)
        {
            if (this.Initialised == false)
            {
                this.Awake();
            }

            if (this.m_OverrideOutline == false)
            {
                return;
            }
            /*
            foreach (var text in this.Texts)
            {
                text.outlineWidth = thickness;
                text.outlineColor = colour;
            }
            */
        }

        public void SetMinMaxFontSizes(float min, float max)
        {
            if (this.m_OverrideSize == false)
            {
                return;
            }
            
            /*
            foreach (var text in this.Texts)
            {
                if (min > 0)
                {
                    text.fontSizeMin = min;
                }

                if (max > 0 && max > min)
                {
                    text.fontSizeMax = max;
                }
            }
            */
        }

        public void SetFontSizes(float size)
        {
            if (this.m_OverrideSize == false)
            {
                return;
            }
            if (this.Initialised == false)
            {
                this.Awake();
            }
            /*
            foreach (var text in this.Texts)
            {
                text.fontSize = size;
            }
            */
        }
    }
}