using System;
using System.Collections.Generic;

namespace JoyLib.Code.World
{
    [Serializable]
    public class WorldTile
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
    }
}
