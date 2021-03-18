using System;
using System.Collections.Generic;
using Castle.Core.Internal;
using JoyLib.Code.Entities;
using JoyLib.Code.Entities.AI;

namespace JoyLib.Code.Scripting.Actions
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
                idle = false,
                intent = Intent.Interact,
                searching = false,
                target = participants[1],
                targetPoint = GlobalConstants.NO_TARGET,
                need = needName
            };

            actor.CurrentTarget = needAIData;
            //GlobalConstants.ActionLog.AddText(actor.JoyName + " is seeking " + participants[1].JoyName + " for " + needName);

            this.SetLastParameters(participants, tags, args);

            return true;
        }
    }
}