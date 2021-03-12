using System.Collections.Generic;
using JoyLib.Code.World;

namespace JoyLib.Code.Entities.AI.LOS
{
    public interface IFOVHandler
    {
        IFOVBoard Do(
            IEntity viewer, 
            IWorldInstance world, 
            Vector2Int dimensions,
            IEnumerable<Vector2Int> walls);
        LinkedList<Vector2Int> HasLOS(Vector2Int origin, Vector2Int target);
    }
}
