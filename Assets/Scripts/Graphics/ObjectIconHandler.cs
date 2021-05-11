using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Castle.Core.Internal;
using Godot;
using Godot.Collections;
using JoyGodot.addons.Managed_Assets;
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

        protected IDictionary<string, Texture> CachedTextures { get; set; }

        //First key is the file name
        //Second key is the position
        //Value is the List of frames from that position
        protected IDictionary<string, IDictionary<int, Texture>> CachedTiles { get; set; }

        public ObjectIconHandler(RNG roller)
        {
            this.Roller = roller;
            this.ValueExtractor = new JSONValueExtractor();
            this.CachedTextures = new System.Collections.Generic.Dictionary<string, Texture>();
            this.CachedTiles = new System.Collections.Generic.Dictionary<string, IDictionary<int, Texture>>();
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
                Name = "default",
                State = "default",
                Parts = new List<SpritePart>
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
                        m_FrameSprite = new List<Texture>()
                        {
                            defaultImageTexture
                        },
                        m_PossibleColours = new List<Color> {Colors.White},
                        m_DrawCentre = true,
                        m_PatchMargins = new int[4],
                        m_StretchMode = NinePatchRect.AxisStretchMode.Stretch
                    }
                }
            };

            this.Icons.Add("default", new List<Tuple<string, SpriteData>>
            {
                new Tuple<string, SpriteData>(iconData.Name, iconData)
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
                this.Icons[tileSet].Add(new Tuple<string, SpriteData>(dataToAdd.Name, dataToAdd));
            }
            else
            {
                this.Icons.Add(new KeyValuePair<string, List<Tuple<string, SpriteData>>>(
                    tileSet,
                    new List<Tuple<string, SpriteData>>
                    {
                        new Tuple<string, SpriteData>(dataToAdd.Name, dataToAdd)
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
            ICollection<Dictionary> tileSetArray =
                this.ValueExtractor.GetArrayValuesCollectionFromDictionary<Dictionary>(tileSetDict, "SpriteData");
            foreach (Dictionary dict in tileSetArray)
            {
                try
                {
                    List<SpritePart> parts = new List<SpritePart>();

                    string name = this.ValueExtractor.GetValueFromDictionary<string>(dict, "Name");
                    int size = this.ValueExtractor.GetValueFromDictionary<int>(dict, "Size");
                    if (size == 0)
                    {
                        size = GlobalConstants.SPRITE_TEXTURE_SIZE;
                    }

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

                        List<Texture> frames = this.ChopSprites(
                            fileName,
                            this.TryGetTextureFromCache(fileName),
                            partFrames,
                            position);

                        int halfway = frames.Count / 2;
                        if (frames.Count > 1)
                        {
                            for (int i = halfway; i >= halfway; i--)
                            {
                                frames.Add(frames[i]);
                            }
                        }

                        SpritePart part = new SpritePart
                        {
                            m_Data = data.ToArray(),
                            m_Filename = fileName,
                            m_Frames = partFrames,
                            m_FrameSprite = frames,
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
                            Name = name,
                            Parts = parts,
                            State = state,
                            Size = size
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

        protected Texture TryGetTextureFromCache(string fileName)
        {
            if (this.CachedTextures.ContainsKey(fileName))
            {
                return this.CachedTextures[fileName];
            }

            Texture texture = GD.Load<Texture>(
                GlobalConstants.GODOT_ASSETS_FOLDER +
                GlobalConstants.SPRITES_FOLDER +
                fileName);
            this.CachedTextures.Add(fileName, texture);
            return texture;
        }

        protected List<Texture> ChopSprites(string fileName, Texture texture, int frames, int position)
        {
            List<Texture> sprites = new List<Texture>();

            Image sheet = texture.GetData();
            int width = sheet.GetWidth();
            int height = sheet.GetHeight();
            const int size = GlobalConstants.SPRITE_TEXTURE_SIZE;

            int index = 0;
            if (this.CachedTiles.ContainsKey(fileName))
            {
                if (this.CachedTiles[fileName].ContainsKey(position))
                {
                    for (index = position; index < position + frames; index++)
                    {
                        if (this.CachedTiles[fileName].ContainsKey(index))
                        {
                            sprites.Add(this.CachedTiles[fileName][index]);
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (sprites.Count == frames)
                    {
                        return sprites;
                    }
                }
            }

            int p = 0;
            for (int y = 0; y < height; y += size)
            {
                for (int x = 0; x < width; x += size)
                {
                    ImageTexture imageTexture = new ImageTexture();
                    imageTexture.CreateFromImage(sheet.GetRect(new Rect2(x, y, size, size)), 2);
                    if (imageTexture.GetData().GetData().All(b => b == 0))
                    {
                        continue;
                    }

                    if (p >= position && p < position + frames)
                    {
                        sprites.Add(imageTexture);
                        if (this.CachedTiles.ContainsKey(fileName))
                        {
                            if (this.CachedTiles[fileName].ContainsKey(p) == false)
                            {
                                this.CachedTiles[fileName].Add(p, imageTexture);
                            }
                        }
                        else
                        {
                            this.CachedTiles.Add(
                                fileName,
                                new System.Collections.Generic.Dictionary<int, Texture>
                                {
                                    {p, imageTexture}
                                });
                        }
                    }
                    else if (p > position + frames)
                    {
                        return sprites;
                    }

                    p++;
                }
            }

            return sprites;
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
                .Where(pair => pair.Item2.State.Equals(state, StringComparison.OrdinalIgnoreCase))
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
}