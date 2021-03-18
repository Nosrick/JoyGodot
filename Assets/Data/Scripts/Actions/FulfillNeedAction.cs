using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using JoyLib.Code.Entities;
using JoyLib.Code.Entities.Needs;

namespace JoyLib.Code.Scripting.Actions
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
            actor.FulfillmentData = actor.FulfillmentData is null 
                ? new FulfillmentData(need, counter, fellowActors) 
                : new FulfillmentData(
                    overwrite 
                    || actor.FulfillmentData.Name.IsNullOrEmpty() 
                    || actor.FulfillmentData.Name.Equals("none", StringComparison.OrdinalIgnoreCase)
                        ? need 
                        : actor.FulfillmentData.Name, 
                    overwrite ? counter : actor.FulfillmentData.Counter + counter,
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
                    entity.FulfillmentData = entity.FulfillmentData is null 
                        ? new FulfillmentData(need, counter, others) 
                        : new FulfillmentData(
                            overwrite ? need : entity.FulfillmentData.Name, 
                            overwrite ? counter : entity.FulfillmentData.Counter + counter,
                            others);
                }
            }

            //GlobalConstants.ActionLog.LogAction(actor, this.ActionString + " " + need);

            this.SetLastParameters(participants, tags, args);

            return true;
        }
    }
}