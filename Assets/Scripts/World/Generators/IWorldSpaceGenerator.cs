using System.Collections.Generic;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.World.WorldInfo;

namespace JoyGodot.Assets.Scripts.World.Generators
{
    public interface IWorldSpaceGenerator
    {
        WorldTile[,] Tiles { get; }
        HashSet<Vector2Int> Walls { get; }
        
        void GenerateWorldSpace(int sizeRef, string tileSet);
    }
}
