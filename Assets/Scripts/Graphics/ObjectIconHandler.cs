using System;
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
using File = Godot.File;

namespace JoyLib.Code.Graphics
{
    public class ObjectIconHandler : IObjectIconHandler
    {
        protected RNG Roller { get; set; }

        protected JSONValueExtractor ValueExtractor { get; set; }

        public ObjectIconHandler(RNG roller)
        {
            this.Roller = roller;
            this.ValueExtractor = new JSONValueExtractor();
            this.Load();
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

            ImageTexture defaultImageTexture = new ImageTexture();
            defaultImageTexture.CreateFromImage(defaultSprite.GetData(), 2);
            //defaultSprite.pivot = new Vector2(0.5f, 0.5f);
            SpriteData iconData = new SpriteData
            {
                m_Name = "default",
                m_State = "default",
                m_Parts = new List<SpritePart>
                {
                    new SpritePart
                    {
                        m_Data = new[] {"default"},
                        m_Filename = GlobalConstants.GODOT_ASSETS_FOLDER +
                                     GlobalConstants.SPRITES_FOLDER +
                                     "default.png",
                        m_Frames = 1,
                        m_Name = "default",
                        m_Position = 0,
                        m_FrameSprite = new SpriteFrames
                        {
                            Frames = new Array
                            {
                                defaultImageTexture
                            }
                        },
                        m_PossibleColours = new List<Color> {Colors.White}
                    }
                }
            };

            this.Icons.Add("default", new List<Tuple<string, SpriteData>>
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
                Dictionary result = (Dictionary) JSON.Parse(System.IO.File.ReadAllText(file)).Result;

                this.AddSpriteDataFromJson(result);
            }

            return true;
        }

        public bool AddSpriteData(string tileSet, SpriteData dataToAdd)
        {
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
                try
                {
                    List<SpritePart> parts = new List<SpritePart>();

                    string name = this.ValueExtractor.GetValueFromDictionary<string>(dict, "Name");
                    string state = this.ValueExtractor.GetValueFromDictionary<string>(dict, "State") ?? "default";
                    Array partsArray = this.ValueExtractor.GetValueFromDictionary<Array>(dict, "Part");
                    ICollection<Dictionary> partDicts =
                        this.ValueExtractor.GetCollectionFromArray<Dictionary>(partsArray);
                    foreach (Dictionary innerDict in partDicts)
                    {
                        string partName = this.ValueExtractor.GetValueFromDictionary<string>(innerDict, "Name");
                        int partFrames = innerDict.Contains("Frames")
                            ? this.ValueExtractor.GetValueFromDictionary<int>(innerDict, "Frames")
                            : 1;
                        string fileName = this.ValueExtractor.GetValueFromDictionary<string>(innerDict, "Filename");
                        int sortOrder = this.ValueExtractor.GetValueFromDictionary<int>(innerDict, "SortOrder");
                        int position = this.ValueExtractor.GetValueFromDictionary<int>(innerDict, "Position");
                        NinePatchRect.AxisStretchMode stretchMode =
                            GraphicsHelper.ParseStretchMode(
                                this.ValueExtractor.GetValueFromDictionary<string>(innerDict, "FillType"));
                        bool drawCentre = !innerDict.Contains("DrawCentre")
                                          || this.ValueExtractor.GetValueFromDictionary<bool>(innerDict, "DrawCentre");
                        Array marginArray = this.ValueExtractor.GetValueFromDictionary<Array>(
                            innerDict,
                            "PatchMargins");
                        ICollection<int> patchMargins = marginArray.IsNullOrEmpty()
                            ? new[] {0, 0, 0, 0}
                            : this.ValueExtractor.GetCollectionFromArray<int>(marginArray);

                        Array partDataArray = innerDict.Contains("Data")
                            ? this.ValueExtractor.GetValueFromDictionary<Array>(innerDict, "Data")
                            : new Array();
                        ICollection<string> data = this.ValueExtractor.GetCollectionFromArray<string>(partDataArray);
                        Array partColourArray =
                            this.ValueExtractor.GetValueFromDictionary<Array>(innerDict, "Colour");
                        ICollection<Color> colours = new List<Color>();
                        ICollection<string> colourCodes = partColourArray.IsNullOrEmpty()
                            ? new List<string>
                            {
                                "#ffffff"
                            }
                            : this.ValueExtractor.GetCollectionFromArray<string>(partColourArray);
                        foreach (string code in colourCodes)
                        {
                            colours.Add(new Color(code));
                        }

                        Texture texture = GD.Load<Texture>(
                            GlobalConstants.GODOT_ASSETS_FOLDER +
                            GlobalConstants.SPRITES_FOLDER +
                            fileName);

                        Image image = texture.GetData();
                        int frameWidth = texture.GetWidth() / partFrames;
                        List<Texture> frames = new List<Texture>();
                        for (int i = 0; i < texture.GetWidth(); i += frameWidth)
                        {
                            ImageTexture imageTexture = new ImageTexture();
                            imageTexture.CreateFromImage(image.GetRect(new Rect2(new Vector2(i, 0),
                                new Vector2(frameWidth, frameWidth))), 2);
                            imageTexture.ResourceLocalToScene = true;
                            frames.Add(imageTexture);
                        }

                        int halfway = frames.Count / 2;
                        if (frames.Count > 1)
                        {
                            for (int i = halfway; i >= halfway; i--)
                            {
                                frames.Add(frames[i]);
                            }
                        }

                        SpriteFrames spriteFrames = new SpriteFrames();
                        if (spriteFrames.GetAnimationNames().Contains(state) == false)
                        {
                            spriteFrames.AddAnimation(state);
                            spriteFrames.SetAnimationLoop(state, true);
                            spriteFrames.SetAnimationSpeed(state, GlobalConstants.FRAMES_PER_SECOND);
                        }

                        for (int i = 0; i < frames.Count; i++)
                        {
                            spriteFrames.AddFrame(state, frames[i], i);
                        }

                        SpritePart part = new SpritePart
                        {
                            m_Data = data.ToArray(),
                            m_Filename = fileName,
                            m_Frames = partFrames,
                            m_FrameSprite = spriteFrames,
                            m_Name = partName,
                            m_Position = position,
                            m_PossibleColours = colours.ToList(),
                            m_SortingOrder = sortOrder,
                            m_PatchMargins = patchMargins.ToArray(),
                            m_DrawCentre = drawCentre,
                            m_StretchMode = stretchMode
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
                catch (Exception e)
                {
                    GlobalConstants.ActionLog.Log("Could not load sprite data from JSON. Offending JSON to follow.");
                    GlobalConstants.ActionLog.Log(dict);
                    GlobalConstants.ActionLog.StackTrace(e);
                }
            }

            return this.AddSpriteDataRange(tileSetName, spriteData);
        }

        public IEnumerable<SpriteData> ReturnDefaultData()
        {
            return this.Icons["default"]
                .Select(tuple => tuple.Item2.Copy());
        }

        public SpriteData ReturnDefaultIcon()
        {
            return this.Icons["DEFAULT"].First().Item2;
        }

        public IEnumerable<SpriteData> GetSprites(string tileSet, string tileName, string state = "DEFAULT")
        {
            List<SpriteData> data = this.Icons.Where(x => x.Key.Equals(tileSet, StringComparison.OrdinalIgnoreCase))
                .SelectMany(x => x.Value.Where(pair => pair.Item1.Equals(tileName, StringComparison.OrdinalIgnoreCase)))
                .Where(pair => pair.Item2.m_State.Equals(state, StringComparison.OrdinalIgnoreCase))
                .Select(x => x.Item2)
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
        public SpriteFrames m_FrameSprite;
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