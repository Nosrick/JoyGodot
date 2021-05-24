﻿using System.Linq;
using System.Collections.Generic;

namespace JoyLib.Code.World.Generators.Overworld
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

            WorldTile template = this.WorldInfoHandler.GetByTileSet(tileSet).ToArray()[0];

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

        public IEnumerable<IJoyObject> GenerateWalls(WorldTile[,] worldTiles)
        {
            return new List<JoyObject>();
        }
    }
}
