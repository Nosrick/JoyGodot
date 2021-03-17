using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JoyLib.Code.Graphics;
using JoyLib.Code.Helpers;

namespace JoyLib.Code.World
{
    public interface IWorldInfoHandler : IHandler<WorldInfo, string>
    {
        WorldInfo GetRandom(params string[] tags);
    }

    public class WorldInfoHandler : IWorldInfoHandler
    {
        protected IObjectIconHandler ObjectIcons { get; set; }

        protected IDictionary<string, WorldInfo> WorldInfoDict { get; set; }

        public IEnumerable<WorldInfo> Values => this.WorldInfoDict.Values;
        public JSONValueExtractor ValueExtractor { get; protected set; }

        public WorldInfoHandler(IObjectIconHandler objectIconHandler)
        {
            this.ObjectIcons = objectIconHandler;
            this.ValueExtractor = new JSONValueExtractor();

            this.WorldInfoDict = this.Load().ToDictionary(info => info.name, info => info);
        }

        public WorldInfo Get(string name)
        {
            return this.WorldInfoDict.TryGetValue(name, out WorldInfo worldInfo)
                ? worldInfo
                : new WorldInfo
                {
                    name = "SOMETHING HAS GONE TERRIBLY WRONG"
                };
        }

        public bool Add(WorldInfo value)
        {
            if (this.WorldInfoDict.ContainsKey(value.name))
            {
                return false;
            }

            this.WorldInfoDict.Add(value.name, value);
            return true;
        }

        public bool Destroy(string key)
        {
            return this.WorldInfoDict.ContainsKey(key) && this.WorldInfoDict.Remove(key);
        }

        public IEnumerable<WorldInfo> Load()
        {
            string[] files =
                Directory.GetFiles(Directory.GetCurrentDirectory() + GlobalConstants.DATA_FOLDER + "World Spaces",
                    "*.json", SearchOption.AllDirectories);
            List<WorldInfo> worldInfos = new List<WorldInfo>();

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

                            if (jToken.IsNullOrEmpty())
                            {
                                continue;
                            }

                            foreach (JToken child in jToken["WorldInfo"])
                            {
                                string name = (string) child["Name"];
                                if (name.IsNullOrEmpty())
                                {
                                    continue;
                                }

                                IEnumerable<string> inhabitants = child["Inhabitants"] is null
                                    ? new string[0]
                                    : child["Inhabitants"].Select(token => (string) token);

                                string[] tags = child["Tags"] is null
                                    ? new string[0]
                                    : child["Tags"].Select(token => (string) token).ToArray();

                                if (child["TileSet"].IsNullOrEmpty())
                                {
                                    continue;
                                }

                                JToken tileSet = child["TileSet"];
                                string tileSetName = (string) tileSet["Name"];
                                this.ObjectIcons.AddSpriteDataFromJson(tileSetName, tileSet["SpriteData"]);

                                worldInfos.Add(new WorldInfo
                                {
                                    inhabitants = inhabitants,
                                    name = name,
                                    tags = tags
                                });

                                StandardWorldTiles.instance.AddType(
                                    new WorldTile(
                                        name,
                                        tileSetName,
                                        tags));
                            }
                        }
                        catch (Exception e)
                        {
                            GlobalConstants.ActionLog.AddText("Error loading world space definition from " + file, LogLevel.Error);
                            GlobalConstants.ActionLog.StackTrace(e);
                        }
                        finally
                        {
                            jsonReader.Close();
                            reader.Close();
                        }
                    }
                }
                */
            }
            return worldInfos;
        }

        public WorldInfo GetRandom(params string[] tags)
        {
            IEnumerable<WorldInfo> matching = this.WorldInfoDict.Values.Where(info =>
                info.tags.Intersect(tags, StringComparer.OrdinalIgnoreCase).Any());
            return new WorldInfo();
            //return GlobalConstants.GameManager.Roller.SelectFromCollection(matching);
        }

        public void Dispose()
        {
            this.WorldInfoDict = null;
        }
    }

    public struct WorldInfo
    {
        public string name;
        public IEnumerable<string> inhabitants;
        public IEnumerable<string> tags;
    }
}
