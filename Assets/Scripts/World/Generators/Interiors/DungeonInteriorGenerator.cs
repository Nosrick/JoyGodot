using System.Collections.Generic;
using System.Linq;
using JoyGodot.addons.Managed_Assets;
using JoyLib.Code.Entities.Statistics;
using JoyLib.Code.Graphics;
using JoyLib.Code.Managers;
using JoyLib.Code.Rollers;

namespace JoyLib.Code.World.Generators.Interiors
{
    public class DungeonInteriorGenerator : IWorldSpaceGenerator
    {
        protected GeneratorTileType[,] m_UntreatedTiles;

        protected GUIDManager GuidManager { get; set; }
        protected IObjectIconHandler ObjectIcons { get; set; }
        protected IDerivedValueHandler DerivedValueHandler { get; set; }
        
        protected IWorldInfoHandler WorldInfoHandler { get; set; }
        protected RNG Roller { get; set; }

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

        public WorldTile[,] GenerateWorldSpace(int sizeRef, string tileSet)
        {
            this.TileSet = tileSet;
            WorldTile[,] tiles = new WorldTile[sizeRef, sizeRef];

            DungeonRoomGenerator roomGen = new DungeonRoomGenerator(sizeRef, this.Roller);

            this.m_UntreatedTiles = roomGen.GenerateRooms();

            DungeonCorridorGenerator corrGen = new DungeonCorridorGenerator(this.m_UntreatedTiles, roomGen.rooms, 50, this.Roller);
            this.m_UntreatedTiles = corrGen.GenerateCorridors();

            tiles = this.TreatTiles();

            return tiles;
        }

        protected WorldTile[,] TreatTiles()
        {
            WorldTile[,] tiles = new WorldTile[this.m_UntreatedTiles.GetLength(0), this.m_UntreatedTiles.GetLength(1)];

            WorldTile[] templates = this.WorldInfoHandler.GetByTileSet(this.TileSet).ToArray();

            for (int i = 0; i < tiles.GetLength(0); i++)
            {
                for (int j = 0; j < tiles.GetLength(1); j++)
                {
                    //TODO: Redo this!
                    tiles[i, j] = templates[0];
                }
            }

            return tiles;
        }

        public HashSet<Vector2Int> GenerateWalls(WorldTile[,] worldTiles)
        {
            HashSet<Vector2Int> walls = new HashSet<Vector2Int>();
            
            for (int i = 0; i < this.m_UntreatedTiles.GetLength(0); i++)
            {
                for (int j = 0; j < this.m_UntreatedTiles.GetLength(1); j++)
                {
                    if (this.m_UntreatedTiles[i, j] == GeneratorTileType.Perimeter || this.m_UntreatedTiles[i, j] == GeneratorTileType.Wall || this.m_UntreatedTiles[i, j] == GeneratorTileType.None)
                    {
                        walls.Add(new Vector2Int(i, j));
                    }
                }
            }

            return walls;
        }

        public void GenerateTileObjects(WorldTile[,] worldTiles)
        {
        }

        protected string TileSet
        {
            get;
            set;
        }
    }
}
