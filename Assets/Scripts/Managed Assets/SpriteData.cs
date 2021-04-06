using System;
using System.Collections.Generic;
using Godot;
using JoyLib.Code;

namespace JoyGodot.addons.Managed_Assets
{
    

    [Serializable]
    public class SpriteData
    {
        public string m_Name;
        public string m_State;
        public List<SpritePart> m_Parts;

        public IDictionary<string, Color> GetRandomPartColours()
        {
            IDictionary<string, Color> colours = new System.Collections.Generic.Dictionary<string, Color>();

            foreach (SpritePart part in this.m_Parts)
            {
                colours.Add(
                    part.m_Name,
                    GlobalConstants.GameManager.Roller.SelectFromCollection(part.m_PossibleColours));
            }

            return colours;
        }

        /*
        public void Dispose()
        {
            if (this.m_Parts is null == false)
            {
                for (int i = 0; i < this.m_Parts.Count; i++)
                {
                    this.m_Parts[i]?.Dispose();
                    this.m_Parts[i] = null;
                }
            }

            this.m_Parts = null;
            this.m_Name = null;
            this.m_State = null;
        }

        ~SpriteData()
        {
            this.Dispose();
        }
        */
    }

    [Serializable]
    public class SpritePart
    {
        public string m_Name;
        public int m_Frames;
        public string[] m_Data;
        //public SpriteFrames m_FrameSprite;
        public List<Texture> m_FrameSprite;
        public string m_Filename;
        public int m_Position;
        public List<Color> m_PossibleColours;
        public int[] m_PatchMargins;
        public int m_SelectedColour;
        public int m_SortingOrder;
        public bool m_DrawCentre;
        public NinePatchRect.AxisStretchMode m_StretchMode;

        public Color SelectedColour => this.m_PossibleColours[this.m_SelectedColour];

        /*
        public void Dispose()
        {
            this.m_FrameSprite = null;
        }

        ~SpritePart()
        {
            this.Dispose();
        }
        */
    }
}