using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using JoyLib.Code;
using JoyLib.Code.Helpers;
using Array = Godot.Collections.Array;

namespace JoyGodot.addons.Managed_Assets
{
    public class SpriteData
    {
        public string Name { get; set; }
        public string State { get; set; }
        public List<SpritePart> Parts { get; set; }

        public int Size { get; set; }

        public SpriteData()
        {
            this.Parts = new List<SpritePart>();
        }

        public IDictionary<string, Color> GetRandomPartColours()
        {
            IDictionary<string, Color> colours = new System.Collections.Generic.Dictionary<string, Color>();

            foreach (SpritePart part in this.Parts)
            {
                colours.Add(
                    part.m_Name,
                    part.m_PossibleColours.GetRandom());
            }

            return colours;
        }

        public IDictionary<string, Color> GetCurrentPartColours()
        {
            return this.Parts.ToDictionary(part => part.m_Name, part => part.SelectedColour);
        }

        public void SetColourIndices(List<int> indices)
        {
            if (indices.Count != this.Parts.Count)
            {
                GlobalConstants.ActionLog.Log("Index count mismatch for SpriteData!", LogLevel.Warning);
                return;
            }

            for (int i = 0; i < indices.Count; i++)
            {
                SpritePart part = this.Parts[i];
                part.m_SelectedColour = indices[i];
                this.Parts[i] = part;
            }
        }

        public List<int> RandomiseIndices()
        {
            List<int> indices = new List<int>();
            foreach (SpritePart part in this.Parts)
            {
                part.m_SelectedColour = GlobalConstants.GameManager.Roller.Roll(0, part.m_PossibleColours.Count);
                indices.Add(part.m_SelectedColour);
            }

            return indices;
        }

        public List<int> GetIndices()
        {
            return this.Parts.Select(part => part.m_SelectedColour).ToList();
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