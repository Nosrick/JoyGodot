using System;
using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Abilities;
using JoyGodot.Assets.Scripts.Entities.Items;
using JoyGodot.Assets.Scripts.JoyObject;

namespace JoyGodot.Assets.Data.Scripts.Abilities
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