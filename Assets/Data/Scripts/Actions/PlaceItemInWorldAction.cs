using System.Collections.Generic;
using JoyGodot.Assets.Scripts;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Items;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Scripting;

namespace JoyGodot.Assets.Data.Scripts.Actions
{
    public class PlaceItemInWorldAction : AbstractAction
    {
        public override string Name => "placeiteminworldaction";
        public override string ActionString => "placing item in world";

        public override bool Execute(IJoyObject[] participants, IEnumerable<string> tags = null,
            IDictionary<string, object> args = null)
        {
            this.ClearLastParameters();
            
            if (!(participants[0] is IEntity entity))
            {
                return false;
            }

            if (!(participants[1] is IItemInstance item))
            {
                return false;
            }
            
            bool result = entity.RemoveContents(item);
            item.Move(entity.WorldPosition);
            entity.MyWorld.AddItem(item);

            if (result)
            {
                this.SetLastParameters(participants, tags, args);
            }
            else
            {
                GlobalConstants.ActionLog.Log("FAILED TO REMOVE ITEM");
            }

            return result;
        }
    }
}