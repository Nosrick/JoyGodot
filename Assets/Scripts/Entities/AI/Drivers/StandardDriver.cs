using System.Collections.Generic;
using System.Linq;
using JoyGodot.Assets.Scripts.Entities.Items;
using JoyGodot.Assets.Scripts.Entities.Needs;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Physics;
using JoyGodot.Assets.Scripts.Rollers;
using JoyGodot.Assets.Scripts.Scripting;

namespace JoyGodot.Assets.Scripts.Entities.AI.Drivers
{
    public class StandardDriver : AbstractDriver
    {
        protected IJoyAction WanderAction { get; set; }

        protected IPhysicsManager PhysicsManager { get; set; }
        
        protected RNG Roller { get; set; }
        
        protected bool Initialised { get; set; }

        public StandardDriver(IPhysicsManager physicsManager = null, RNG roller = null)
        {
            this.PhysicsManager = physicsManager ?? GlobalConstants.GameManager.PhysicsManager;
            this.Roller = roller ?? new RNG();
            this.Initialise();
        }

        protected void Initialise()
        {
            if (this.Initialised)
            {
                return;
            }

            this.WanderAction = GlobalConstants.ScriptingEngine.FetchAction("wanderaction");
            this.Initialised = true;
        }

        public override void Interact()
        {
            throw new System.NotImplementedException();
        }

        public override void Locomotion(IEntity vehicle)
        {
            this.Initialise();
            
            //Don't move if you're currently busy
            if (vehicle.NeedFulfillmentData.Name.IsNullOrEmpty() == false 
                && vehicle.NeedFulfillmentData.Counter > 0)
            {
                return;
            }
            
            //If you're idle
            if (vehicle.CurrentTarget.Idle)
            {
                //Let's find something to do
                List<INeed> needs = vehicle.Needs.Values.OrderByDescending(x => x.Priority).ToList();
                //Act on first need

                bool idle = true;
                bool wander = false;
                foreach (INeed need in needs)
                {
                    if (need.ContributingHappiness)
                    {
                        continue;
                    }

                    idle &= need.FindFulfilmentObject(vehicle);
                    break;
                }

                if(idle)
                {
                    int result = this.Roller.Roll(0, 10);
                    if (result < 1)
                    {
                        wander = true;
                    }
                }

                if(wander)
                {
                    this.WanderAction.Execute(
                        new IJoyObject[] { vehicle },
                        new[] { "wander", "idle"});
                }
            }

            //If we have somewhere to be, move there
            if (vehicle.WorldPosition != vehicle.CurrentTarget.TargetPoint 
                || vehicle.CurrentTarget.Target != null)
            {
                if (vehicle.CurrentTarget.Target is IItemInstance 
                    && vehicle.WorldPosition != vehicle.CurrentTarget.Target.WorldPosition)
                {
                    if (vehicle.CurrentTarget.TargetPoint.Equals(GlobalConstants.NO_TARGET))
                    {
                        NeedAIData newData = new NeedAIData
                        {
                            Idle = vehicle.CurrentTarget.Idle,
                            Intent = vehicle.CurrentTarget.Intent,
                            Need = vehicle.CurrentTarget.Need,
                            Searching = vehicle.CurrentTarget.Searching,
                            Target = vehicle.CurrentTarget.Target,
                            TargetPoint = vehicle.CurrentTarget.Target.WorldPosition
                        };
                        vehicle.CurrentTarget = newData;
                    }

                    this.MoveToTarget(vehicle);
                }
                else if(vehicle.CurrentTarget.Target is IEntity
                    && AdjacencyHelper.IsAdjacent(vehicle.WorldPosition, vehicle.CurrentTarget.Target.WorldPosition) == false)
                {
                    this.MoveToTarget(vehicle);
                }
                else if (vehicle.CurrentTarget.Target is null 
                && vehicle.CurrentTarget.TargetPoint.Equals(GlobalConstants.NO_TARGET) == false)
                {
                    this.MoveToTarget(vehicle);
                }
            }

            if (vehicle.MyWorld.GetEntity(vehicle.CurrentTarget.TargetPoint) is null == false
                || vehicle.MyWorld.GetObject(vehicle.CurrentTarget.TargetPoint) is null == false)
            {
                //If we've arrived at our destination, then we do our thing
                if ((vehicle.WorldPosition == vehicle.CurrentTarget.TargetPoint
                     && (vehicle.CurrentTarget.Target is IItemInstance || vehicle.CurrentTarget.Target is null)
                     || (vehicle.CurrentTarget.Target is IEntity 
                         && AdjacencyHelper.IsAdjacent(vehicle.WorldPosition, vehicle.CurrentTarget.Target.WorldPosition))))
                {
                    //If we have a target
                    if (vehicle.CurrentTarget.Target is null == false)
                    {
                        if (vehicle.CurrentTarget.Intent == Intent.Attack)
                        {
                            //CombatEngine.SwingWeapon(this, CurrentTarget.target);
                        }
                        else if (vehicle.CurrentTarget.Intent == Intent.Interact)
                        {
                            INeed need = vehicle.Needs[vehicle.CurrentTarget.Need];

                            need.Interact(vehicle, vehicle.CurrentTarget.Target);
                            vehicle.CurrentTarget = NeedAIData.IdleState();
                        }
                    }
                    //If we do not, we were probably wandering
                    else
                    {
                        if (vehicle.CurrentTarget.Searching)
                        {
                            NeedAIData currentTarget = vehicle.CurrentTarget;

                            currentTarget.TargetPoint = GlobalConstants.NO_TARGET;

                            //Set idle to true so we look for stuff when we arrive
                            currentTarget.Idle = true;

                            vehicle.CurrentTarget = currentTarget;
                        }
                    }
                }
            }
            else
            {
                this.WanderAction.Execute(
                    new IJoyObject[] { vehicle },
                    new[] { "wander", "idle"});
            }
        }

        protected void MoveToTarget(IEntity vehicle)
        {
            if (!vehicle.HasMoved && vehicle.PathfindingData.Count > 0)
            {
                Vector2Int nextPoint = vehicle.PathfindingData.Peek();
                PhysicsResult physicsResult = this.PhysicsManager.IsCollision(vehicle.WorldPosition, nextPoint, vehicle.MyWorld);
                if (physicsResult != PhysicsResult.EntityCollision
                    && physicsResult != PhysicsResult.WallCollision)
                {
                    vehicle.PathfindingData.Dequeue();
                    vehicle.Move(nextPoint);
                    vehicle.HasMoved = true;
                }
                else
                {
                    vehicle.MyWorld.SwapPosition(vehicle, vehicle.MyWorld.GetEntity(nextPoint));
                    vehicle.PathfindingData.Dequeue();
                    vehicle.Move(nextPoint);
                    vehicle.HasMoved = true;
                }
            }
            else if (vehicle.PathfindingData.Count == 0)
            {
                if (vehicle.CurrentTarget.Target != null)
                {
                    vehicle.PathfindingData = vehicle.Pathfinder.FindPath(
                        vehicle.WorldPosition, 
                        vehicle.CurrentTarget.Target.WorldPosition, 
                        vehicle.MyWorld.Costs, vehicle.VisionProvider.GetFullVisionRect(vehicle));
                }
                else if (vehicle.CurrentTarget.TargetPoint != GlobalConstants.NO_TARGET)
                {
                    vehicle.PathfindingData = vehicle.Pathfinder.FindPath(
                        vehicle.WorldPosition, 
                        vehicle.CurrentTarget.TargetPoint, 
                        vehicle.MyWorld.Costs, 
                        vehicle.VisionProvider.GetFullVisionRect(vehicle));
                }
            }
        }
    }
}