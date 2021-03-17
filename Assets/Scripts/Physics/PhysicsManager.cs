using JoyLib.Code.Entities;
using JoyLib.Code.World;

namespace JoyLib.Code.Physics
{
    public interface IPhysicsManager
    {
        PhysicsResult IsCollision(Vector2Int from, Vector2Int to, IWorldInstance worldRef);
    }

    public class PhysicsManager : IPhysicsManager
    {
        public PhysicsResult IsCollision(Vector2Int @from, Vector2Int to, IWorldInstance worldRef)
        {
            IEntity tempEntity = worldRef.GetEntity(to);
            if (tempEntity != null && from != to)
            {
                if(tempEntity.WorldPosition != from)
                {
                    return PhysicsResult.EntityCollision;
                }
            }

            if (worldRef.Walls.ContainsKey(to))
            {
                return PhysicsResult.WallCollision;
            }

            IJoyObject obj = worldRef.GetObject(to);
            if (obj != null)
            {
                return PhysicsResult.ObjectCollision;
            }   
            else
            {
                return PhysicsResult.None;
            }
        }
    }
}
