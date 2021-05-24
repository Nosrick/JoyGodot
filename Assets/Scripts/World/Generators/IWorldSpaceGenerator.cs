using System.Collections.Generic;

namespace JoyLib.Code.World.Generators
{
    public interface IWorldSpaceGenerator
    {
        WorldTile[,] GenerateWorldSpace(int sizeRef, string tileSet);
        void GenerateTileObjects(WorldTile[,] worldTiles);
        IEnumerable<IJoyObject> GenerateWalls(WorldTile[,] worldTiles);
    }
}
