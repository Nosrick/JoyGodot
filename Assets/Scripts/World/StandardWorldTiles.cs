using System;
using System.Collections.Generic;
using System.Linq;

namespace JoyLib.Code.World
{
    public class StandardWorldTiles
    {
        private static Lazy<StandardWorldTiles> lazy = new Lazy<StandardWorldTiles>(() => new StandardWorldTiles());

        public static StandardWorldTiles instance = lazy.Value;

        protected HashSet<WorldTile> m_StandardTypes;

        public StandardWorldTiles()
        {
            this.m_StandardTypes = new HashSet<WorldTile>();
        }

        public bool AddType(WorldTile tile)
        {
            return this.m_StandardTypes.Add(tile);
        }

        public IEnumerable<WorldTile> GetByTileSet(string tileSet)
        {
            return this.m_StandardTypes.Where(x => x.TileSet.Equals(tileSet, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<WorldTile> GetByTileName(string tileName)
        {
            return this.m_StandardTypes.Where(x => x.TileName.Equals(tileName, StringComparison.OrdinalIgnoreCase));
        }

        public WorldTile GetSpecificTile(string tileSet, string tileName)
        {
            return this.m_StandardTypes.First(x => x.TileSet.Equals(tileSet, StringComparison.OrdinalIgnoreCase) && x.TileName.Equals(tileName, StringComparison.OrdinalIgnoreCase));
        }
        
        public IEnumerable<WorldTile> GetByTag(string tag)
        {
            return this.m_StandardTypes.Where(x => x.Tags.Contains(tag));
        }

        public IEnumerable<WorldTile> GetByTags(IEnumerable<string> tags)
        {
            return this.m_StandardTypes.Where(x => x.Tags.Intersect(tags).ToArray().Length > 0);
        }
    }
}