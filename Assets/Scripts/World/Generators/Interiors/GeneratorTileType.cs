using System;

namespace JoyLib.Code.World.Generators.Interiors
{
    [Flags]
    public enum GeneratorTileType
    {
        None = 0,
        Wall = 1,
        Floor = 2,
        Corridor = 4,
        Perimeter = 8,
        Entrance = 16
    }
}
