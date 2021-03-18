using System;
using System.Collections.Generic;
using Castle.Core.Internal;
using JoyLib.Code.Entities;
using JoyLib.Code.World;

namespace JoyLib.Code.Scripting.Actions
{
    public class EnterWorldAction : AbstractAction
    {
        public override string Name => "enterworldaction";

        public override string ActionString => "enters";

        public override bool Execute(IJoyObject[] participants, IEnumerable<string> tags = null,
            IDictionary<string, object> args = null)
        {
            this.ClearLastParameters();

            if (args.IsNullOrEmpty())
            {
                return false;
            }
            
            if (participants.Length != 1 || args.TryGetValue("world", out object arg) == false)
            {
                return false;
            }

            if (!(participants[0] is Entity actor))
            {
                return false;
            }

            if (!(arg is IWorldInstance worldInstance))
            {
                return false;
            }
            
            actor.MyWorld.RemoveEntity(actor.WorldPosition);
            worldInstance.AddEntity(actor);

            if (!actor.HasDataKey(worldInstance.Name))
            {
                actor.AddData(worldInstance.Name, "explored");
            }

            this.SetLastParameters(participants, tags, args);

            return true;
        }
    }
}