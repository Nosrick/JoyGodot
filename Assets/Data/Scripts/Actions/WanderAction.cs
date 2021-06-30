using System.Collections.Generic;
using System.Linq;
using JoyGodot.Assets.Scripts;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.AI;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Scripting;

namespace JoyGodot.Assets.Data.Scripts.Actions
{
    public class WanderAction : AbstractAction
    {
        public override string Name => "wanderaction";

        public override string ActionString => "wandering";

        public override bool Execute(IJoyObject[] participants, IEnumerable<string> tags = null,
            IDictionary<string, object> args = null)
        {
            this.ClearLastParameters();
            
            if(!(participants[0] is Entity actor))
            {
                return false;
            }

            List<Vector2Int> visibleWalls = actor.MyWorld.GetVisibleWalls(actor);
            Vector2Int[] viablePoints = actor.Vision
                .Where(i => visibleWalls.Contains(i) == false)
                .ToArray();

            Vector2Int result = GlobalConstants.NO_TARGET;

            if (viablePoints.Length > 0)
            {
                result = viablePoints[GlobalConstants.GameManager.Roller.Roll(0, viablePoints.Length)];
            }

            NeedAIData needAIData = new NeedAIData
            {
                Idle = false,
                Intent = Intent.Interact,
                Searching = true,
                TargetPoint = result
            };
            
            //GlobalConstants.ActionLog.AddText(actor.JoyName + " is wandering.");

            actor.CurrentTarget = needAIData;

            this.SetLastParameters(participants, tags, args);

            return true;
        }
    }
}