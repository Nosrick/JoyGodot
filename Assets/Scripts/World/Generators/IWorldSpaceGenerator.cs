using System.Collections.Generic;

namespace JoyLib.Code.World.Generators
{
    public interface IWorldSpaceGenerator
    {
        WorldTile[,] GenerateWorldSpace(int sizeRef, string tileSet);
        void GenerateTileObjects(WorldTile[,] worldTiles);
        HashSet<Vector2Int> GenerateWalls(WorldTile[,] worldTiles);
    }
}
