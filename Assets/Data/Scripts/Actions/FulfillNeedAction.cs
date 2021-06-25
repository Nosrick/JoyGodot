using System;
using System.Collections.Generic;
using System.Linq;

using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Needs;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Scripting;

namespace JoyGodot.Assets.Data.Scripts.Actions
{
    public class FulfillNeedAction : AbstractAction
    {
        public override string Name => "fulfillneedaction";

        public override string ActionString => "fulfilling need";

        public override bool Execute(
            IJoyObject[] participants,
            IEnumerable<string> tags = null,
            IDictionary<string, object> args = null)
        {
            this.ClearLastParameters();
            
            if(!(participants[0] is IEntity actor))
            {
                return false;
            }

            if (args.IsNullOrEmpty())
            {
                return false;
            }

            if(!(args.TryGetValue("need", out object arg)))
            {
                return false;
            }

            string need = (string) arg;

            if(!(args.TryGetValue("value", out arg)))
            {
                return false;
            }

            int value = (int) arg;

            int counter = args.TryGetValue("counter", out arg) ? (int) arg : 0;

            bool doAll = args.TryGetValue("doAll", out arg) && (bool) arg;

            bool overwrite = args.TryGetValue("overwrite", out arg) && (bool) arg;

            IJoyObject[] fellowActors = participants.Where(p => p.Guid != actor.Guid).ToArray();
            
            actor.Needs[need].Fulfill(value);
            actor.NeedFulfillmentData = actor.NeedFulfillmentData.IsEmpty()
                ? new NeedFulfillmentData(need, counter, fellowActors) 
                : new NeedFulfillmentData(
                    overwrite 
                    || actor.NeedFulfillmentData.Name.IsNullOrEmpty() 
                    || actor.NeedFulfillmentData.Name.Equals("none", StringComparison.OrdinalIgnoreCase)
                        ? need 
                        : actor.NeedFulfillmentData.Name, 
                    overwrite ? counter : actor.NeedFulfillmentData.Counter + counter,
                    fellowActors);

            if (doAll)
            {
                foreach (IJoyObject jo in fellowActors)
                {
                    if (!(jo is IEntity entity))
                    {
                        continue;
                    }
                    
                    IJoyObject[] others = participants.Where(p => p.Guid != entity.Guid).ToArray();
                    entity.Needs[need].Fulfill(value);
                    entity.NeedFulfillmentData = entity.NeedFulfillmentData.IsEmpty()
                        ? new NeedFulfillmentData(need, counter, others) 
                        : new NeedFulfillmentData(
                            overwrite ? need : entity.NeedFulfillmentData.Name, 
                            overwrite ? counter : entity.NeedFulfillmentData.Counter + counter,
                            others);
                }
            }

            //GlobalConstants.ActionLog.LogAction(actor, this.ActionString + " " + need);

            this.SetLastParameters(participants, tags, args);

            return true;
        }
    }
}