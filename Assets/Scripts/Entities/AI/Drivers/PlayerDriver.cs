namespace JoyGodot.Assets.Scripts.Entities.AI.Drivers
{
    public class PlayerDriver : AbstractDriver
    {
        //protected IPhysicsManager PhysicsManager { get; set; }

        public override bool PlayerControlled => true;

        public PlayerDriver()
        {
            //this.PhysicsManager = GlobalConstants.GameManager.PhysicsManager;
        }

        public override void Interact()
        {
            throw new System.NotImplementedException();
        }

        public override void Locomotion(IEntity vehicle)
        {
            /*
            if (!vehicle.HasMoved && vehicle.PathfindingData.Count > 0)
            {
                Vector2Int nextPoint = vehicle.PathfindingData.Peek();
                PhysicsResult physicsResult = this.PhysicsManager.IsCollision(vehicle.WorldPosition, nextPoint, vehicle.MyWorld);
                if (physicsResult != PhysicsResult.EntityCollision && physicsResult != PhysicsResult.WallCollision)
                {
                    vehicle.PathfindingData.Dequeue();
                    vehicle.Move(nextPoint);
                    vehicle.HasMoved = true;
                }
                else if (physicsResult == PhysicsResult.EntityCollision)
                {
                    vehicle.MyWorld.SwapPosition(vehicle, vehicle.MyWorld.GetEntity(nextPoint));

                    vehicle.PathfindingData.Dequeue();
                    vehicle.Move(nextPoint);
                    vehicle.HasMoved = true;
                }
            }
            */
        }
    }
}