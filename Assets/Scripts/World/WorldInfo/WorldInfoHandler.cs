﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using Godot.Collections;
using JoyGodot.Assets.Scripts.Base_Interfaces;
using JoyGodot.Assets.Scripts.Collections;
using JoyGodot.Assets.Scripts.Graphics;
using JoyGodot.Assets.Scripts.Helpers;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace JoyGodot.Assets.Scripts.World.WorldInfo
{
    public interface IWorldInfoHandler : IHandler<WorldInfo, string>
    {
        IEnumerable<WorldTile> GetByTileSet(string tileSet);

        IEnumerable<WorldTile> GetByTileName(string tileName);

        WorldTile GetSpecificTile(string tileSet, string tileName);

        IEnumerable<WorldTile> GetByTags(IEnumerable<string> tags);

        WorldInfo GetRandom(params string[] tags);
    }

    public class WorldInfoHandler : IWorldInfoHandler
    {
        protected IObjectIconHandler ObjectIcons { get; set; }

        protected IDictionary<string, WorldInfo> WorldInfoDict { get; set; }

        protected NonUniqueDictionary<string, WorldTile> WorldTiles { get; set; }

        public IEnumerable<WorldInfo> Values => this.WorldInfoDict.Values;
        public JSONValueExtractor ValueExtractor { get; protected set; }

        public WorldInfoHandler(IObjectIconHandler objectIconHandler)
        {
            this.ObjectIcons = objectIconHandler;
            this.ValueExtractor = new JSONValueExtractor();
            this.WorldTiles = new NonUniqueDictionary<string, WorldTile>();

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

        public IEnumerable<WorldInfo> GetMany(IEnumerable<string> keys)
        {
            return keys.Select(this.Get);
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
                Directory.GetFiles(
                    Directory.GetCurrentDirectory() +
                    GlobalConstants.ASSETS_FOLDER +
                    GlobalConstants.DATA_FOLDER +
                    "World Spaces",
                    "*.json",
                    SearchOption.AllDirectories);
            List<WorldInfo> worldInfos = new List<WorldInfo>();

            foreach (string file in files)
            {
                JSONParseResult result = JSON.Parse(File.ReadAllText(file));

                if (result.Error != Error.Ok)
                {
                    this.ValueExtractor.PrintFileParsingError(result, file);
                    continue;
                }

                if (!(result.Result is Dictionary dictionary))
                {
                    GlobalConstants.ActionLog.Log("Could not parse JSON to Dictionary from " + file, LogLevel.Warning);
                    continue;
                }

                ICollection<Dictionary> infoCollection =
                    this.ValueExtractor.GetArrayValuesCollectionFromDictionary<Dictionary>(dictionary, "WorldInfo");

                foreach (Dictionary worldInfo in infoCollection)
                {
                    string name = this.ValueExtractor.GetValueFromDictionary<string>(worldInfo, "Name");
                    if (name.IsNullOrEmpty())
                    {
                        continue;
                    }

                    IEnumerable<string> inhabitants = worldInfo.Contains("Inhabitants")
                        ? this.ValueExtractor.GetArrayValuesCollectionFromDictionary<string>(worldInfo, "Inhabitants")
                        : new string[0];

                    IEnumerable<string> cultures = worldInfo.Contains("Cultures")
                        ? this.ValueExtractor.GetArrayValuesCollectionFromDictionary<string>(worldInfo, "Cultures")
                        : new string[0];

                    IEnumerable<string> tags = worldInfo.Contains("Tags")
                        ? this.ValueExtractor.GetArrayValuesCollectionFromDictionary<string>(worldInfo, "Tags")
                        : new string[0];

                    string tileSetName = this.ValueExtractor.GetValueFromDictionary<string>(
                        this.ValueExtractor.GetValueFromDictionary<Dictionary>(worldInfo, "TileSet"),
                        "Name");
                    this.ObjectIcons.AddSpriteDataFromJson(worldInfo);

                    worldInfos.Add(new WorldInfo
                    {
                        inhabitants = inhabitants,
                        name = name,
                        tags = tags,
                        cultures = cultures
                    });

                    this.WorldTiles.Add(
                        name,
                        new WorldTile(
                            name,
                            tileSetName,
                            tags));
                }
            }

            return worldInfos;
        }

        public IEnumerable<WorldTile> GetByTileSet(string tileSet)
        {
            if (this.WorldTiles.Any(tuple => tuple.Item1.Equals(tileSet, StringComparison.OrdinalIgnoreCase)) == false)
            {
                return new WorldTile[0];
            }

            return this.WorldTiles
                .Where(t => t.Item1.Equals(tileSet, StringComparison.OrdinalIgnoreCase))
                .Select(tuple => tuple.Item2);
        }

        public IEnumerable<WorldTile> GetByTileName(string tileName)
        {
            return this.WorldTiles.Values.Where(tile =>
                tile.TileName.Equals(tileName, StringComparison.OrdinalIgnoreCase));
        }

        public WorldTile GetSpecificTile(string tileSet, string tileName)
        {
            if (this.WorldTiles.ContainsKey(tileSet))
            {
                this.WorldTiles[tileSet]
                    .FirstOrDefault(tile => tile.TileName.Equals(tileName, StringComparison.OrdinalIgnoreCase));
            }

            return null;
        }

        public IEnumerable<WorldTile> GetByTags(IEnumerable<string> tags)
        {
            return this.WorldTiles.Values.Where(tile => tile.Tags.Intersect(tags).Any());
        }

        public WorldInfo GetRandom(params string[] tags)
        {
            IEnumerable<WorldInfo> matching = this.WorldInfoDict.Values.Where(info =>
                info.tags.Intersect(tags, StringComparer.OrdinalIgnoreCase).Any());
            return GlobalConstants.GameManager.Roller.SelectFromCollection(matching);
        }

        public void Dispose()
        {
            var keys = new List<string>(this.WorldTiles.Keys);
            foreach (string key in keys)
            {
                this.WorldTiles.RemoveAll(key);
            }

            this.WorldTiles = null;
            
            GarbageMan.Dispose(this.WorldInfoDict);
            this.WorldInfoDict = null;
        }
    }

    public struct WorldInfo
    {
        public string name;
        public IEnumerable<string> cultures;
        public IEnumerable<string> inhabitants;
        public IEnumerable<string> tags;
    }
}