using System.Collections.Generic;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.World.WorldInfo;

namespace JoyGodot.Assets.Scripts.World.Generators
{
    public interface IWorldSpaceGenerator
    {
        WorldTile[,] GenerateWorldSpace(int sizeRef, string tileSet);
        void GenerateTileObjects(WorldTile[,] worldTiles);
        HashSet<Vector2Int> GenerateWalls(WorldTile[,] worldTiles);
    }
}
