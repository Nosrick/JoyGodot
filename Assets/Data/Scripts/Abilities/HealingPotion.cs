using System;
using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Abilities;
using JoyGodot.Assets.Scripts.Entities.Statistics;
using JoyGodot.Assets.Scripts.Items;
using JoyGodot.Assets.Scripts.JoyObject;

namespace JoyGodot.Assets.Data.Scripts.Abilities
{
    public class HealingPotion : AbstractAbility
    {
        public HealingPotion()
            : base(
                "Quaff",
                "healingpotion",
                "Heal 1/3 of your HP.",
                false,
                0,
                0,
                0,
                false,
                new string[0], 
                new Tuple<string, int>[0],
                new Dictionary<string, int>
                {
                    {"potion", 1}
                }, 
                AbilityTarget.Ranged,
                3,
                GetSprite("healingpotion"),
                "healing", "active")
        {}

        public override bool OnUse(IEntity user, IJoyObject target)
        {
            int restore = user.DerivedValues[DerivedValueName.HITPOINTS].Maximum / 3;
            user.ModifyValue(DerivedValueName.HITPOINTS, restore);

            user.RemoveContents(target as IItemInstance);
            user.MyWorld.Tick();
            
            return true;
        }
    }
}