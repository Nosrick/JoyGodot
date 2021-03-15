﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Castle.Core.Internal;
using Godot;
using Godot.Collections;
using JoyLib.Code.Helpers;
using JoyLib.Code.Rollers;
using Array = Godot.Collections.Array;
using Directory = System.IO.Directory;

namespace JoyLib.Code.Graphics
{
    public class ObjectIconHandler : IObjectIconHandler
    {
        protected RNG Roller { get; set; }

        protected JSONValueExtractor ValueExtractor { get; set; }

        public ObjectIconHandler(RNG roller)
        {
            this.Roller = roller;
            this.Load();
            this.ValueExtractor = new JSONValueExtractor();
        }

        protected bool Load()
        {
            if (!(this.Icons is null))
            {
                return true;
            }

            this.Icons = new System.Collections.Generic.Dictionary<string, List<Tuple<string, SpriteData>>>();

            Texture defaultSprite = GD.Load<Texture>(
                GlobalConstants.GODOT_ASSETS_FOLDER + 
                GlobalConstants.SPRITES_FOLDER + 
                "default.png");
            //defaultSprite.pivot = new Vector2(0.5f, 0.5f);
            SpriteData iconData = new SpriteData
            {
                m_Name = "DEFAULT",
                m_State = "DEFAULT",
                m_Parts = new List<SpritePart>
                {
                    new SpritePart
                    {
                        m_Data = new[] {"DEFAULT"},
                        m_Filename = GlobalConstants.GODOT_ASSETS_FOLDER + 
                                     GlobalConstants.SPRITES_FOLDER + 
                                     "default.png",
                        m_Frames = 1,
                        m_Name = "DEFAULT",
                        m_Position = 0,
                        m_FrameSprite = defaultSprite,
                        m_PossibleColours = new List<Color> {Colors.White}
                    }
                }
            };

            this.Icons.Add("DEFAULT", new List<Tuple<string, SpriteData>>
            {
                new Tuple<string, SpriteData>(iconData.m_Name, iconData)
            });

            string[] files =
                Directory.GetFiles(
                    Directory.GetCurrentDirectory() + 
                    GlobalConstants.ASSETS_FOLDER + 
                    GlobalConstants.DATA_FOLDER + 
                    "/Sprite Definitions", "*.json",
                    SearchOption.AllDirectories);

            foreach (string file in files)
            {
                /*
                using (StreamReader reader = new StreamReader(file))
                {
                    using (JsonTextReader jsonReader = new JsonTextReader(reader))
                    {
                        try
                        {
                            JObject jToken = JObject.Load(jsonReader);

                            if (jToken["Objects"].IsNullOrEmpty())
                            {
                                continue;
                            }

                            JToken tileSet = jToken["Objects"]["TileSet"];

                            string name = (string) tileSet["Name"];

                            this.AddSpriteDataFromJson(name, tileSet["SpriteData"]);
                        }
                        catch (Exception e)
                        {
                            GlobalConstants.ActionLog.AddText("Cannot load sprite definitions from " + file);
                            GlobalConstants.ActionLog.StackTrace(e);
                        }
                    }
                }
                */
            }

            return true;
        }

        public bool AddSpriteData(string tileSet, SpriteData dataToAdd)
        {
            List<SpritePart> parts = new List<SpritePart>();
            /*
            foreach (SpritePart part in dataToAdd.m_Parts)
            {
                SpritePart copy = part;
                copy.m_FrameSprites = GD.Load<Sprite>("Sprites/" + part.m_Filename)
                    .Where(
                        (sprite, i) =>
                            i >= part.m_Position
                            && i < part.m_Position +
                            part.m_Frames)
                    .ToList();
                parts.Add(copy);
            }
            */

            dataToAdd.m_Parts = parts;

            if (this.Icons.ContainsKey(tileSet))
            {
                this.Icons[tileSet].Add(new Tuple<string, SpriteData>(dataToAdd.m_Name, dataToAdd));
            }
            else
            {
                this.Icons.Add(new KeyValuePair<string, List<Tuple<string, SpriteData>>>(
                    tileSet,
                    new List<Tuple<string, SpriteData>>
                    {
                        new Tuple<string, SpriteData>(dataToAdd.m_Name, dataToAdd)
                    }));
            }

            return true;
        }

        public bool AddSpriteDataRange(string tileSet, IEnumerable<SpriteData> dataToAdd)
        {
            return dataToAdd.Aggregate(true, (current, data) => current & this.AddSpriteData(tileSet, data));
        }

        /// <summary>
        /// This must be passed the "SpriteData" node of the JSON
        /// </summary>
        /// <param name="tileSet">The tileset that the data belongs to</param>
        /// <param name="spriteDataToken">The JSON to pull the data from. MUST have a root of "SpriteData"</param>
        /// <returns></returns>
        public bool AddSpriteDataFromJson(Dictionary spriteDict)
        {
            List<SpriteData> spriteData = new List<SpriteData>();
            Dictionary tileSetDict = this.ValueExtractor.GetValueFromDictionary<Dictionary>(spriteDict, "TileSet");
            string tileSetName = this.ValueExtractor.GetValueFromDictionary<string>(tileSetDict, "Name");
            Array tileSetArray = this.ValueExtractor.GetValueFromDictionary<Array>(tileSetDict, "SpriteData");
            ICollection<Dictionary> tileSetDicts =
                this.ValueExtractor.GetCollectionFromArray<Dictionary>(tileSetArray);
            foreach (Dictionary dict in tileSetDicts)
            {
                List<SpritePart> parts = new List<SpritePart>();
                
                string name = this.ValueExtractor.GetValueFromDictionary<string>(dict, "Name");
                string state = this.ValueExtractor.GetValueFromDictionary<string>(dict, "State");
                Array partsArray = this.ValueExtractor.GetValueFromDictionary<Array>(dict, "Part");
                ICollection<Dictionary> partDicts =
                    this.ValueExtractor.GetCollectionFromArray<Dictionary>(partsArray);
                foreach (Dictionary innerDict in partDicts)
                {
                    string partName = this.ValueExtractor.GetValueFromDictionary<string>(innerDict, "Name");
                    int partFrames = this.ValueExtractor.GetValueFromDictionary<int>(innerDict, "Frames");
                    string fileName = this.ValueExtractor.GetValueFromDictionary<string>(innerDict, "Filename");
                    int sortOrder = this.ValueExtractor.GetValueFromDictionary<int>(innerDict, "SortOrder");
                    int position = this.ValueExtractor.GetValueFromDictionary<int>(innerDict, "Position");
                    Array partDataArray = this.ValueExtractor.GetValueFromDictionary<Array>(innerDict, "Data");
                    ICollection<string> data = this.ValueExtractor.GetCollectionFromArray<string>(partDataArray);
                    Array partColourArray =
                        this.ValueExtractor.GetValueFromDictionary<Array>(innerDict, "Colour");
                    ICollection<Color> colours = new List<Color>();
                    ICollection<string> colourCodes =
                        this.ValueExtractor.GetCollectionFromArray<string>(partColourArray);
                    foreach (string code in colourCodes)
                    {
                        colours.Add(new Color(code));
                    }

                    if (colours.IsNullOrEmpty())
                    {
                        colours.Add(Colors.White);
                    }

                    Texture sprite = GD.Load<Texture>(
                        GlobalConstants.GODOT_ASSETS_FOLDER + 
                        GlobalConstants.SPRITES_FOLDER + 
                        fileName);

                    SpritePart part = new SpritePart
                    {
                        m_Data = data.ToArray(),
                        m_Filename = fileName,
                        m_Frames = partFrames,
                        m_FrameSprite = sprite,
                        m_Name = partName,
                        m_Position = position,
                        m_PossibleColours = colours.ToList(),
                        m_SortingOrder = sortOrder
                    };

                    parts.Add(part);
                }
                
                spriteData.Add(
                    new SpriteData
                    {
                        m_Name = name,
                        m_Parts = parts,
                        m_State = state
                    });
            }

            GlobalConstants.ActionLog.Log(tileSetDict);

            return this.AddSpriteDataRange(tileSetName, spriteData);
        }

        public IEnumerable<SpriteData> ReturnDefaultData()
        {
            return this.Icons["DEFAULT"]
                .Select(tuple => tuple.Item2.Copy());
        }

        public SpriteData ReturnDefaultIcon()
        {
            return this.Icons["DEFAULT"].First().Item2;
        }

        public SpriteData GetFrame(string tileSet, string tileName, string state = "DEFAULT", int frame = 0)
        {
            SpriteData[] frames = this.GetSprites(tileSet, tileName, state).ToArray();
            return frames.Length >= frame ? frames[frame] : this.ReturnDefaultIcon();
        }

        public List<Texture> GetRawFrames(string tileSet, string tileName, string partName, string state = "DEFAULT")
        {
            List<Texture> sprites = this.GetSprites(tileSet, tileName, state).SelectMany(data => data.m_Parts)
                .Where(part => part.m_Name.Equals(partName, StringComparison.OrdinalIgnoreCase))
                .Select(part => part.m_FrameSprite)
                .ToList();

            return sprites;
        }

        public IEnumerable<SpriteData> GetSprites(string tileSet, string tileName, string state = "DEFAULT")
        {
            List<SpriteData> data = this.Icons.Where(x => x.Key.Equals(tileSet, StringComparison.OrdinalIgnoreCase))
                .SelectMany(x => x.Value.Where(pair => pair.Item1.Equals(tileName, StringComparison.OrdinalIgnoreCase)))
                .Where(pair => pair.Item2.m_State.Equals(state, StringComparison.OrdinalIgnoreCase))
                .Select(x => x.Item2.Copy())
                .ToList();

            return data.Any() == false ? this.ReturnDefaultData() : data;
        }

        public IEnumerable<SpriteData> GetTileSet(string tileSet)
        {
            return this.Icons.Where(pair => pair.Key.Equals(tileSet, StringComparison.OrdinalIgnoreCase))
                .SelectMany(pair => pair.Value)
                .Select(pair => pair.Item2);
        }

        protected IDictionary<string, List<Tuple<string, SpriteData>>> Icons { get; set; }
    }

    [Serializable]
    public class SpriteData : IDisposable
    {
        public string m_Name;
        public string m_State;
        public List<SpritePart> m_Parts;

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
    }

    [Serializable]
    public class SpritePart : IDisposable
    {
        public string m_Name;
        public int m_Frames;
        public string[] m_Data;
        [NonSerialized] public Texture m_FrameSprite;
        public string m_Filename;
        public int m_Position;
        public List<Color> m_PossibleColours;
        public int m_SelectedColour;
        public int m_SortingOrder;

        public Color SelectedColour => this.m_PossibleColours[this.m_SelectedColour];

        public void Dispose()
        {
            this.m_FrameSprite = null;
        }

        ~SpritePart()
        {
            this.Dispose();
        }
    }
}