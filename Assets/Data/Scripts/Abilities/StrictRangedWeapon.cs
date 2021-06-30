using System;
using System.Collections.Generic;
using System.Linq;
using JoyGodot.Assets.Scripts;
using JoyGodot.Assets.Scripts.Entities.Abilities;
using JoyGodot.Assets.Scripts.Entities.Statistics;

namespace JoyGodot.Assets.Data.Scripts.Abilities
{
    public class StrictRangedWeapon : AbstractAbility
    {
        public StrictRangedWeapon()
            : base(
                "adjacency penalty",
                "strictrangedweapon",
                "Roll at disadvantage when attacking with this weapon in melee range",
                false,
                0,
                0,
                0,
                false,
                new string[0],
                new Tuple<string, int>[0],
                new Dictionary<string, int>
                {
                    {"ranged", 1}
                },
                AbilityTarget.Self,
                0,
                GetSprite("strictrangedweapon"),
                "attack", "ranged", "threshold", "passive")
        {}
        
        public override int OnCheckRollModifyThreshold(int successThreshold, IEnumerable<IBasicValue<int>> values, IEnumerable<string> attackerTags,
            IEnumerable<string> defenderTags)
        {
            if (attackerTags.Any(tag => tag.Equals("adjacent"))
                && attackerTags.Any(tag => tag.Equals("attack")))
            {
                return Math.Min(GlobalConstants.MAXIMUM_SUCCESS_THRESHOLD, successThreshold + 1);
            }
                
            return successThreshold;
        }
    }
}