using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using JoyGodot.addons.Managed_Assets;

namespace JoyLib.Code.Graphics
{
    public class SpriteState : ISpriteState
    {
        public SpriteData SpriteData
        {
            get => this.m_SpriteData;
            protected set => this.m_SpriteData = value;
        }

        protected SpriteData m_SpriteData;

        public string Name { get; protected set; }
        
        public string TileSet { get; protected set; }

        public AnimationType AnimationType { get; protected set; }

        public bool Looping { get; protected set; }

        public bool IsAnimated { get; set; }

        public SpriteState()
        { }

        public SpriteState(
            string name,
            string tileSet,
            SpriteData spriteData,
            AnimationType animationType = AnimationType.PingPong,
            bool animated = true,
            bool looping = true,
            bool randomiseColours = false)
        {
            this.SpriteData = spriteData;

            this.Name = name;

            this.TileSet = tileSet;

            this.AnimationType = animationType;

            this.IsAnimated = animated;
            this.Looping = looping;

            if (this.SpriteData.Parts.Max(part => part.m_Frames) == 1)
            {
                this.IsAnimated = false;
            }
            else
            {
                this.IsAnimated = true;
            }

            if (randomiseColours)
            {
                this.RandomiseColours();
            }
        }

        public SpritePart GetPart(string name)
        {
            return this.m_SpriteData.Parts.FirstOrDefault(part =>
                part.m_Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public void OverrideColours(IDictionary<string, Color> colours, bool overwrite = false)
        {
            for (int i = 0; i < this.SpriteData.Parts.Count; i++)
            {
                SpritePart part = this.SpriteData.Parts[i];
                if (!colours.ContainsKey(part.m_Name))
                {
                    continue;
                }

                if (overwrite)
                {
                    part.m_PossibleColours = new List<Color>
                    {
                        colours[part.m_Name]
                    };
                    part.m_SelectedColour = 0;
                }
                else
                {
                    if (part.m_PossibleColours.Contains(colours[part.m_Name]))
                    {
                        part.m_SelectedColour =
                            part.m_PossibleColours.FindIndex(color => color.Equals(colours[part.m_Name]));
                    }
                    else
                    {
                        part.m_PossibleColours.Add(colours[part.m_Name]);
                        part.m_SelectedColour = part.m_PossibleColours.Count - 1;
                    }
                }
                
                this.SpriteData.Parts[i] = part;
            }
        }

        public void OverrideWithSingleColour(Color colour)
        {
            for (int i = 0; i < this.SpriteData.Parts.Count; i++)
            {
                SpritePart part = this.SpriteData.Parts[i];
                part.m_PossibleColours = new List<Color>
                {
                    colour
                };
                part.m_SelectedColour = 0;
                this.SpriteData.Parts[i] = part;
            }
        }

        public void RandomiseColours()
        {
            this.OverrideColours(this.SpriteData.GetRandomPartColours());
        }

        /*
        public void Dispose()
        {
            this.m_SpriteData?.Dispose();
            this.m_SpriteData = null;
        }

        ~SpriteState()
        {
            this.Dispose();
        }
        */
        public Dictionary Save()
        {
            Dictionary saveDict = new Dictionary
            {
                {"TileSet", this.TileSet},
                {"IsAnimated", this.IsAnimated},
                {"Looping", this.Looping},
                {"AnimationType", this.AnimationType.ToString()},
                {"Name", this.SpriteData?.Name},
                {"DataState", this.SpriteData?.State}
            };

            return saveDict;
        }

        public void Load(Dictionary data)
        {
            var valueExtractor = GlobalConstants.GameManager.EntityHandler.ValueExtractor;

            this.Name = valueExtractor.GetValueFromDictionary<string>(data, "Name");
            this.TileSet = valueExtractor.GetValueFromDictionary<string>(data, "TileSet");
            this.IsAnimated = valueExtractor.GetValueFromDictionary<bool>(data, "IsAnimated");
            this.Looping = valueExtractor.GetValueFromDictionary<bool>(data, "Looping");
            this.AnimationType = (AnimationType) Enum.Parse(
                typeof(AnimationType),
                valueExtractor.GetValueFromDictionary<string>(data, "AnimationType"));
            string dataState = valueExtractor.GetValueFromDictionary<string>(data, "DataState");
            this.m_SpriteData = GlobalConstants.GameManager.ObjectIconHandler
                .GetManagedSprites(
                    this.TileSet,
                    this.Name,
                    dataState)
                .FirstOrDefault();
        }
    }
}