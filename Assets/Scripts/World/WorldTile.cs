using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using JoyLib.Code.Helpers;
using Array = Godot.Collections.Array;

namespace JoyLib.Code.World
{
    [Serializable]
    public class WorldTile : ISerialisationHandler
    {
        protected HashSet<string> m_Tags;

        public WorldTile(string tileName, string tileSet, IEnumerable<string> tags)
        {
            this.TileName = tileName;
            this.TileSet = tileSet;
            this.m_Tags = new HashSet<string>(tags);
        }

        public bool AddTag(string tag)
        {
            return this.m_Tags.Add(tag);
        }

        public bool RemoveTag(string tag)
        {
            return this.m_Tags.Remove(tag);
        }

        public HashSet<string> Tags
        {
            get
            {
                return new HashSet<string>(this.m_Tags);
            }
        }

        public string TileName
        {
            get;
            protected set;
        }

        public string TileSet
        {
            get;
            protected set;
        }

        public Dictionary Save()
        {
            Dictionary saveDict = new Dictionary
            {
                {"TileName", this.TileName}, 
                {"TileSet", this.TileSet}
            };

            Array tagArray = new Array();
            foreach (string tag in this.Tags)
            {
                tagArray.Add(tag);
            }
            saveDict.Add("Tags", tagArray);

            return saveDict;
        }

        public void Load(string data)
        {
            JSONValueExtractor valueExtractor = GlobalConstants.GameManager.SettingsManager.ValueExtractor;

            JSONParseResult result = JSON.Parse(data);
            
            if (result.Error != Error.Ok)
            {
                GlobalConstants.ActionLog.Log("Could not parse JSON for WorldTile", LogLevel.Warning);
                return;
            }

            if (!(result.Result is Dictionary dictionary))
            {
                GlobalConstants.ActionLog.Log("Could not parse JSON to Dictionary for WorldTile", LogLevel.Warning);
                return;
            }

            this.m_Tags = new HashSet<string>(
                valueExtractor.GetArrayValuesCollectionFromDictionary<string>(
                    dictionary, 
                    "Tags"));

            this.TileName = valueExtractor.GetValueFromDictionary<string>(
                dictionary,
                "TileName");

            this.TileSet = valueExtractor.GetValueFromDictionary<string>(
                dictionary,
                "TileSet");
        }
    }
}
