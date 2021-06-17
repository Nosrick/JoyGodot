using System.Linq;
using Castle.Core.Internal;
using JoyGodot.Assets.Scripts.World;

namespace JoyGodot.Assets.Scripts.Helpers
{
    public static class WorldConversationDataHelper
    {
        public static int GetNumberOfFloors(int floorsSoFar, IWorldInstance worldToCheck)
        {
            if (worldToCheck.Areas.Count > 0)
            {
                foreach (IWorldInstance world in worldToCheck.Areas.Values)
                {
                    return GetNumberOfFloors(floorsSoFar + 1, world);
                }
            }
                
            return floorsSoFar;
        }

        public static int GetNumberOfCreatures(string entityType, IWorldInstance worldToCheck)
        {
            return entityType.IsNullOrEmpty() ? 
                worldToCheck.Entities.Count 
                : worldToCheck.Entities.Count(x => x.CreatureType.Equals(entityType));
        }
    }
}
