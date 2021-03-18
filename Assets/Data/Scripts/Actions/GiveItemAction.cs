using System;
using System.Collections.Generic;
using Castle.Core.Internal;
using JoyLib.Code.Entities;
using JoyLib.Code.Entities.Items;

namespace JoyLib.Code.Scripting.Actions
{
    public class GiveItemAction : AbstractAction
    {
        public override string Name => "giveitemaction";
        public override string ActionString => "gives";
        
        public override bool Execute(
            IJoyObject[] participants, 
            IEnumerable<string> tags = null,
            IDictionary<string, object> args = null)
        {
            this.ClearLastParameters();

            if (participants.Length != 2)
            {
                return false;
            }

            if (args.IsNullOrEmpty())
            {
                return false;
            }

            if (!(participants[0] is IEntity left))
            {
                return false;
            }
            if (!(participants[1] is IEntity right))
            {
                return false;
            }

            if (args.TryGetValue("item", out object arg) == false)
            {
                return false;
            }

            if (!(arg is IItemInstance item))
            {
                return false;
            }

            if (!left.RemoveContents(item))
            {
                return false;
            }

            if (!right.AddContents(item))
            {
                return false;
            }

            item.SetOwner(right.Guid);

            this.SetLastParameters(participants, tags, args);

            return true;
        }
    }
}