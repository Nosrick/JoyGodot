﻿using System.Collections.Generic;
using System.Linq;
using Godot;
using JoyGodot.Assets.Scripts.Helpers;

namespace JoyGodot.Assets.Scripts.Managed_Assets
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

        public SpriteData Duplicate()
        {
            return new SpriteData
            {
                Name = this.Name,
                Parts = this.Parts.Select(part => part.Duplicate()).ToList(),
                Size = this.Size,
                State = this.State
            };
        }
    }

    public class SpritePart
    {
        public string m_Name;
        public int m_Frames;

        public string[] m_Data;

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

        public SpritePart Duplicate()
        {
            return new SpritePart
            {
                m_Data = this.m_Data.ToArray(),
                m_Filename = this.m_Filename,
                m_DrawCentre = this.m_DrawCentre,
                m_Frames = this.m_Frames,
                m_FrameSprite = this.m_FrameSprite.Select(texture => (Texture) texture.Duplicate()).ToList(),
                m_Name = this.m_Name,
                m_PatchMargins = this.m_PatchMargins,
                m_Position = this.m_Position,
                m_PossibleColours = this.m_PossibleColours.Select(colour => new Color(colour)).ToList(),
                m_SelectedColour = this.m_SelectedColour,
                m_SortingOrder = this.m_SortingOrder,
                m_StretchMode = this.m_StretchMode
            };
        }
    }
}