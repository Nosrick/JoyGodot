using System;
using System.Collections.Generic;
using JoyGodot.Assets.Scripts;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Abilities;
using JoyGodot.Assets.Scripts.Entities.Items;
using JoyGodot.Assets.Scripts.JoyObject;

namespace JoyGodot.Assets.Data.Scripts.Abilities
{
    public class GenericNonAlcoholicDrink : AbstractAbility
    {
        public GenericNonAlcoholicDrink() : base(
            "Drink",
            "genericnonalcoholicdrink",
            "Quench your thirst with a cool refreshment.",
            false,
            0,
            0,
            0,
            false,
            new string[] { "fulfillneedaction" },
            new Tuple<string, int>[0], 
            GetPrerequisites(), 
            AbilityTarget.Self,
            0,
            GetSprite("genericnonalcoholicdrink"),
            "active", "ingestion", "drink")
        { }

        protected static Dictionary<string, int> GetPrerequisites()
        {
            Dictionary<string, int> prereqs = new Dictionary<string, int>();
            prereqs.Add("drink", 1);
            return prereqs;
        }

        public override bool OnUse(IEntity user, IJoyObject target)
        {
            if(target is IItemInstance item)
            {
                this.m_CachedActions["fulfillneedaction"].Execute(
                    new IJoyObject[] { user },
                    new string[] { "thirst", "need", "fulfill" },
                    new Dictionary<string, object>
                    {
                        {"need", "thirst"}, 
                        {"value" , item.ItemType.Value}, 
                        {"counter", 10}
                    }
                );
                user.RemoveContents(item);
                GlobalConstants.GameManager.ItemHandler?.Destroy(item.Guid);

                if (user.PlayerControlled)
                {
                    GlobalConstants.GameManager.GUIManager?.CloseAllGUIs();
                }
                return true;
            }
            return false;
        }
    }
}
