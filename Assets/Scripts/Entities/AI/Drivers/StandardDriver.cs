using System.Collections.Generic;
using System.Linq;
using JoyLib.Code.Entities.Items;
using JoyLib.Code.Entities.Needs;
using JoyLib.Code.Helpers;
using JoyLib.Code.Rollers;
using JoyLib.Code.Scripting;

namespace JoyLib.Code.Entities.AI.Drivers
{
    public class StandardDriver : AbstractDriver
    {
        protected IJoyAction WanderAction { get; set; }

        //protected IPhysicsManager PhysicsManager { get; set; }
        
        protected RNG Roller { get; set; }
        
        protected bool Initialised { get; set; }

        /*
        public StandardDriver(IPhysicsManager physicsManager = null, RNG roller = null)
        {
            this.Roller = roller ?? new RNG();
            this.Initialise();
        }
        */

        protected void Initialise()
        {
            if (this.Initialised)
            {
                return;
            }

            this.WanderAction = ScriptingEngine.Instance.FetchAction("wanderaction");
            //this.PhysicsManager = GlobalConstants.GameManager.PhysicsManager;
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
            if (vehicle.FulfillmentData is null == false 
                && vehicle.FulfillmentData.Counter > 0)
            {
                return;
            }
            
            //If you're idle
            if (vehicle.CurrentTarget.idle)
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
            if (vehicle.WorldPosition != vehicle.CurrentTarget.targetPoint 
                || vehicle.CurrentTarget.target != null)
            {
                if (vehicle.CurrentTarget.target is IItemInstance 
                    && vehicle.WorldPosition != vehicle.CurrentTarget.target.WorldPosition)
                {
                    if (vehicle.CurrentTarget.targetPoint.Equals(GlobalConstants.NO_TARGET))
                    {
                        NeedAIData newData = new NeedAIData
                        {
                            idle = vehicle.CurrentTarget.idle,
                            intent = vehicle.CurrentTarget.intent,
                            need = vehicle.CurrentTarget.need,
                            searching = vehicle.CurrentTarget.searching,
                            target = vehicle.CurrentTarget.target,
                            targetPoint = vehicle.CurrentTarget.target.WorldPosition
                        };
                        vehicle.CurrentTarget = newData;
                    }

                    this.MoveToTarget(vehicle);
                }
                else if(vehicle.CurrentTarget.target is IEntity
                    && AdjacencyHelper.IsAdjacent(vehicle.WorldPosition, vehicle.CurrentTarget.target.WorldPosition) == false)
                {
                    this.MoveToTarget(vehicle);
                }
                else if (vehicle.CurrentTarget.target is null 
                && vehicle.CurrentTarget.targetPoint.Equals(GlobalConstants.NO_TARGET) == false)
                {
                    this.MoveToTarget(vehicle);
                }
            }

            if (vehicle.MyWorld.GetEntity(vehicle.CurrentTarget.targetPoint) is null == false
                || vehicle.MyWorld.GetObject(vehicle.CurrentTarget.targetPoint) is null == false)
            {
                //If we've arrived at our destination, then we do our thing
                if ((vehicle.WorldPosition == vehicle.CurrentTarget.targetPoint
                     && (vehicle.CurrentTarget.target is IItemInstance || vehicle.CurrentTarget.target is null)
                     || (vehicle.CurrentTarget.target is IEntity 
                         && AdjacencyHelper.IsAdjacent(vehicle.WorldPosition, vehicle.CurrentTarget.target.WorldPosition))))
                {
                    //If we have a target
                    if (vehicle.CurrentTarget.target is null == false)
                    {
                        if (vehicle.CurrentTarget.intent == Intent.Attack)
                        {
                            //CombatEngine.SwingWeapon(this, CurrentTarget.target);
                        }
                        else if (vehicle.CurrentTarget.intent == Intent.Interact)
                        {
                            INeed need = vehicle.Needs[vehicle.CurrentTarget.need];

                            need.Interact(vehicle, vehicle.CurrentTarget.target);
                            vehicle.CurrentTarget = NeedAIData.IdleState();
                        }
                    }
                    //If we do not, we were probably wandering
                    else
                    {
                        if (vehicle.CurrentTarget.searching)
                        {
                            NeedAIData currentTarget = vehicle.CurrentTarget;

                            currentTarget.targetPoint = GlobalConstants.NO_TARGET;

                            //Set idle to true so we look for stuff when we arrive
                            currentTarget.idle = true;

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
            /*
            if (!vehicle.HasMoved && vehicle.PathfindingData.Count > 0)
            {
                Vector2Int nextPoint = vehicle.PathfindingData.Peek();
                PhysicsResult physicsResult = this.PhysicsManager.IsCollision(vehicle.WorldPosition, nextPoint, vehicle.MyWorld);
                if (physicsResult != PhysicsResult.EntityCollision)
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
            else if (vehicle.PathfindingData.Count == 0)
            {
                if (vehicle.CurrentTarget.target != null)
                {
                    vehicle.PathfindingData = vehicle.Pathfinder.FindPath(
                        vehicle.WorldPosition, 
                        vehicle.CurrentTarget.target.WorldPosition, 
                        vehicle.MyWorld.Costs, vehicle.VisionProvider.GetFullVisionRect(vehicle));
                }
                else if (vehicle.CurrentTarget.targetPoint != GlobalConstants.NO_TARGET)
                {
                    vehicle.PathfindingData = vehicle.Pathfinder.FindPath(
                        vehicle.WorldPosition, 
                        vehicle.CurrentTarget.targetPoint, 
                        vehicle.MyWorld.Costs, 
                        vehicle.VisionProvider.GetFullVisionRect(vehicle));
                }
            }
            */
        }
    }
}