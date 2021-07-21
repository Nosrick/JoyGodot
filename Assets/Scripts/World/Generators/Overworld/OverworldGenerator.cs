using System.Collections.Generic;
using System.Linq;
using JoyGodot.Assets.Scripts.JoyObject;

namespace JoyGodot.Assets.Scripts.World.Generators.Overworld
{
    public class OverworldGenerator : IWorldSpaceGenerator
    {
        protected IWorldInfoHandler WorldInfoHandler { get; set; }
        
        public OverworldGenerator(IWorldInfoHandler worldInfoHandler)
        {
            this.WorldInfoHandler = worldInfoHandler;
        }
        
        public WorldTile[,] GenerateWorldSpace(int sizeRef, string tileSet)
        {
            WorldTile[,] tiles = new WorldTile[sizeRef, sizeRef];

            WorldTile template = this.WorldInfoHandler.GetByTileSet(tileSet).FirstOrDefault();

            for (int i = 0; i < tiles.GetLength(0); i++)
            {
                for(int j = 0; j < tiles.GetLength(1); j++)
                {
                    //TODO: Make this better!
                    tiles[i, j] = template;
                }
            }

            return tiles;
        }

        public void GenerateTileObjects(WorldTile[,] tiles)
        {
        }

        public HashSet<Vector2Int> GenerateWalls(WorldTile[,] worldTiles)
        {
            return new HashSet<Vector2Int>();
        }
    }
}
