using System.Collections.Generic;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.World;

namespace JoyGodot.Assets.Scripts.Entities.AI.LOS.Boards
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
