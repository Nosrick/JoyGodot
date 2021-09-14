using System.Collections.Generic;
using System.Linq;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.World.WorldInfo;

namespace JoyGodot.Assets.Scripts.World.Generators.Overworld
{
    public class OverworldGenerator : IWorldSpaceGenerator
    {
        protected IWorldInfoHandler WorldInfoHandler { get; set; }

        public WorldTile[,] Tiles { get; protected set; }
        public HashSet<Vector2Int> Walls { get; protected set; }

        public OverworldGenerator(IWorldInfoHandler worldInfoHandler)
        {
            this.WorldInfoHandler = worldInfoHandler;
        }
        
        public void GenerateWorldSpace(int sizeRef, string tileSet)
        {
            this.Tiles = new WorldTile[sizeRef, sizeRef];

            WorldTile template = this.WorldInfoHandler.GetByTileSet(tileSet).FirstOrDefault();

            for (int i = 0; i < this.Tiles.GetLength(0); i++)
            {
                for(int j = 0; j < this.Tiles.GetLength(1); j++)
                {
                    //TODO: Make this better!
                    this.Tiles[i, j] = template;
                }
            }
        }
    }
}
