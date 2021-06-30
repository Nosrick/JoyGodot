using System;
using System.Collections.Generic;
using System.Linq;
using JoyGodot.Assets.Scripts;
using JoyGodot.Assets.Scripts.Entities.Abilities;
using JoyGodot.Assets.Scripts.Entities.Statistics;

namespace JoyGodot.Assets.Data.Scripts.Abilities
{
    public class PiercingGaze : AbstractAbility
    {
        public PiercingGaze()
            : base(
                "piercing gaze",
                "piercinggaze",
                "Roll at advantage on Intimidate checks.",
                false,
                0,
                0,
                0,
                false,
                new string[0],
                new Tuple<string, int>[0],
                GetPrerequisites(),
                AbilityTarget.Ranged,
                3,
                GetSprite("piercinggaze"),
                "attack", "social", "threshold", "intimidate", "passive")
        {}
        
        protected static Dictionary<string, int> GetPrerequisites()
        {
            Dictionary<string, int> prereqs = new Dictionary<string, int>();
            prereqs.Add("personality", 7);
            prereqs.Add("intimidate", 3);
            return prereqs;
        }

        public override int OnCheckRollModifyThreshold(int successThreshold, IEnumerable<IBasicValue<int>> values, IEnumerable<string> attackerTags,
            IEnumerable<string> defenderTags)
        {
            if (attackerTags.Any(tag => tag.Equals("intimidate"))
                && attackerTags.Any(tag => tag.Equals("attack")))
            {
                return Math.Max(GlobalConstants.MINIMUM_SUCCESS_THRESHOLD, successThreshold - 1);
            }
                
            return successThreshold;
        }
    }
}