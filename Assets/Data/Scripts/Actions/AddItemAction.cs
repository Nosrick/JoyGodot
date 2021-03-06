using System;
using System.Collections.Generic;
using System.Linq;

using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.Items;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Scripting;

namespace JoyGodot.Assets.Data.Scripts.Actions
{
    public class AddItemAction : AbstractAction
    {
        public override string Name => "additemaction";

        public override string ActionString => "adding item";

        public override bool Execute(IJoyObject[] participants, IEnumerable<string> tags = null,
            IDictionary<string, object> args = null)
        {
            this.ClearLastParameters();
            
            if(!(participants[0] is IItemContainer container))
            {
                return false;
            }

            if(!(participants[1] is ItemInstance item))
            {
                return false;
            }

            bool newOwner = false;
            if (args.IsNullOrEmpty() == false)
            {
                newOwner = args.TryGetValue("newOwner", out object arg) && (bool) arg;
            }

            bool result = true;
            if (newOwner && container is IEntity owner)
            {
                if (tags is null == false 
                    && tags.Any(tag => tag.Equals("theft", StringComparison.OrdinalIgnoreCase)) 
                    && owner.Guid != item.OwnerGUID
                    && item.OwnerGUID != Guid.Empty)
                {
                    item.SetOwner(owner.Guid);
                }
            }

            result &= item.MyWorld?.RemoveObject(item.WorldPosition, item) ?? true;
            this.SetLastParameters(participants, tags, args);

            return result && container.AddContents(item);
        }
    }
}