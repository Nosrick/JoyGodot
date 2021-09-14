using System;
using System.Collections.Generic;
using System.Linq;
using JoyGodot.Assets.Scripts.Entities.Statistics;
using JoyGodot.Assets.Scripts.Graphics;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Managers;
using JoyGodot.Assets.Scripts.Rollers;
using JoyGodot.Assets.Scripts.World.WorldInfo;

namespace JoyGodot.Assets.Scripts.World.Generators.Interiors
{
    public class DungeonInteriorGenerator : IWorldSpaceGenerator
    {
        protected GeneratorTileType[,] m_UntreatedTiles;

        protected GUIDManager GuidManager { get; set; }
        protected IObjectIconHandler ObjectIcons { get; set; }
        protected IDerivedValueHandler DerivedValueHandler { get; set; }

        protected IWorldInfoHandler WorldInfoHandler { get; set; }

        protected string TileSet { get; set; }
        protected RNG Roller { get; set; }
        
        public WorldTile[,] Tiles { get; protected set; }
        public HashSet<Vector2Int> Walls { get; protected set; }

        public DungeonInteriorGenerator(
            GUIDManager guidManager,
            IObjectIconHandler objectIconHandler,
            IDerivedValueHandler derivedValueHandler,
            IWorldInfoHandler worldInfoHandler,
            RNG roller)
        {
            this.GuidManager = guidManager;
            this.DerivedValueHandler = derivedValueHandler;
            this.Roller = roller;
            this.WorldInfoHandler = worldInfoHandler;
            this.ObjectIcons = objectIconHandler;
        }

        public void GenerateWorldSpace(int sizeRef, string tileSet)
        {
            this.TileSet = tileSet;

            DungeonRoomGenerator roomGen = new DungeonRoomGenerator(sizeRef, this.Roller);

            this.m_UntreatedTiles = roomGen.GenerateRooms();

            DungeonCorridorGenerator corrGen =
                new DungeonCorridorGenerator(
                    this.m_UntreatedTiles, 
                    roomGen.rooms, 
                    50,
                    this.Roller);
            
            this.m_UntreatedTiles = corrGen.GenerateCorridors();

            this.GenerateWalls();
            this.TreatTiles();
        }

        protected void TreatTiles()
        {
            this.Tiles = new WorldTile[this.m_UntreatedTiles.GetLength(0), this.m_UntreatedTiles.GetLength(1)];

            WorldTile[] templates = this.WorldInfoHandler.GetByTileSet(this.TileSet).ToArray();

            WorldTile emptyTile = this.WorldInfoHandler.GetEmptyTile();

            for (int i = 0; i < this.Tiles.GetLength(0); i++)
            {
                for (int j = 0; j < this.Tiles.GetLength(1); j++)
                {
                    if (this.m_UntreatedTiles[i, j] == GeneratorTileType.Floor
                        || this.m_UntreatedTiles[i, j] == GeneratorTileType.Wall)
                    {
                        this.Tiles[i, j] = templates.FirstOrDefault();
                    }
                    else
                    {
                        this.Tiles[i, j] = emptyTile;
                    }
                }
            }
        }

        protected void GenerateWalls()
        {
            this.Walls = new HashSet<Vector2Int>();

            for (int i = 0; i < this.m_UntreatedTiles.GetLength(0); i++)
            {
                for (int j = 0; j < this.m_UntreatedTiles.GetLength(1); j++)
                {
                    if (this.m_UntreatedTiles[i, j] == GeneratorTileType.Perimeter ||
                        this.m_UntreatedTiles[i, j] == GeneratorTileType.Wall ||
                        this.m_UntreatedTiles[i, j] == GeneratorTileType.None)
                    {
                        this.Walls.Add(new Vector2Int(i, j));
                    }
                }
            }
        }
    }
}