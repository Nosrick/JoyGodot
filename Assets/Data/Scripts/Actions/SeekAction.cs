using System.Collections.Generic;

using JoyGodot.Assets.Scripts;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.AI;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Scripting;

namespace JoyGodot.Assets.Data.Scripts.Actions
{
    public class SeekAction : AbstractAction
    {
        public override string Name => "seekaction";

        public override string ActionString => "seeking";

        public override bool Execute(
            IJoyObject[] participants, 
            IEnumerable<string> tags = null,
            IDictionary<string, object> args = null)
        {
            this.ClearLastParameters();

            if (args.IsNullOrEmpty())
            {
                return false;
            }
            
            if(!(participants[0] is Entity actor))
            {
                return false;
            }

            string needName = args.TryGetValue("need", out object arg) ? (string) arg : null;
            
            if(needName.IsNullOrEmpty())
            {
                return false;
            }

            NeedAIData needAIData = new NeedAIData
            {
                Idle = false,
                Intent = Intent.Interact,
                Searching = false,
                Target = participants[1],
                TargetPoint = GlobalConstants.NO_TARGET,
                Need = needName
            };

            actor.CurrentTarget = needAIData;
            //GlobalConstants.ActionLog.AddText(actor.JoyName + " is seeking " + participants[1].JoyName + " for " + needName);

            this.SetLastParameters(participants, tags, args);

            return true;
        }
    }
}