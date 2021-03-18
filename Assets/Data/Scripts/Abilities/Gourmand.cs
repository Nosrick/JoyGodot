using System;
using System.Collections.Generic;
using JoyLib.Code.Entities.Items;

namespace JoyLib.Code.Entities.Abilities
{
    public class Gourmand : AbstractAbility
    {
        public Gourmand()
        : base(
            "Gourmand",
            "Gourmand",
            "Doubles the efficacy of food items.",
            false,
            0,
            0,
            0,
            true,
            new string[0], 
            new Tuple<string, int>[0], 
            new Dictionary<string, int>(),
            AbilityTarget.Self,
            0,
            GetSprite("gourmand"),
            "passive","ingestion")
        {
        }

        public override bool OnUse(IEntity user, IJoyObject target)
        {
            if (target is IItemInstance item 
                && item.HasTag("food"))
            {
                user.Needs["hunger"].ModifyValue(item.Value);
                return true;
            }

            return false;
        }
    }
}