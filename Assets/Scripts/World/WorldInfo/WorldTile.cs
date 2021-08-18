using System.Collections.Generic;
using Godot.Collections;
using JoyGodot.Assets.Scripts.Base_Interfaces;
using JoyGodot.Assets.Scripts.Helpers;
using Array = Godot.Collections.Array;

namespace JoyGodot.Assets.Scripts.World.WorldInfo
{
    public class WorldTile : ISerialisationHandler
    {
        protected HashSet<string> m_Tags;
        
        public byte BitMask { get; set; }

        public WorldTile()
        {
            this.m_Tags = new HashSet<string>();
        }
        
        public WorldTile(
            string tileName, 
            string tileSet, 
            IEnumerable<string> tags,
            byte bitMask = 0)
        {
            this.TileName = tileName;
            this.TileSet = tileSet;
            this.m_Tags = new HashSet<string>(tags);
            this.BitMask = bitMask;
        }

        public bool AddTag(string tag)
        {
            return this.m_Tags.Add(tag);
        }

        public bool RemoveTag(string tag)
        {
            return this.m_Tags.Remove(tag);
        }

        public HashSet<string> Tags => this.m_Tags;

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
                {"TileSet", this.TileSet},
                {"Tags", new Array(this.m_Tags)},
                {"BitMask", this.BitMask}
            };

            return saveDict;
        }

        public void Load(Dictionary data)
        {
            JSONValueExtractor valueExtractor = GlobalConstants.GameManager.SettingsManager.ValueExtractor;

            this.m_Tags = new HashSet<string>(
                valueExtractor.GetArrayValuesCollectionFromDictionary<string>(
                    data, 
                    "Tags"));

            this.TileName = valueExtractor.GetValueFromDictionary<string>(
                data,
                "TileName");

            this.TileSet = valueExtractor.GetValueFromDictionary<string>(
                data,
                "TileSet");

            this.BitMask = valueExtractor.GetValueFromDictionary<byte>(
                data,
                "BitMask");
        }
    }
}
