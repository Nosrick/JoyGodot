using System;
using System.Collections.Generic;
using System.Linq;
using JoyGodot.Assets.Scripts;
using JoyGodot.Assets.Scripts.Entities.Abilities;
using JoyGodot.Assets.Scripts.Entities.Statistics;

namespace JoyGodot.Assets.Data.Scripts.Abilities
{
    public class KeenReflexes : AbstractAbility
    {
        public KeenReflexes()
            :base(
                "keen reflexes",
                "keenreflexes",
                "Roll at advantage on Agility checks to dodge attacks.",
                false,
                0,
                0,
                0,
                false,
                new string[0],
                new Tuple<string, int>[0],
                GetPrerequisites(),
                AbilityTarget.Self,
                0,
                GetSprite("keenreflexes"),
                "defend", "threshold", "agility", "physical", "passive")
        {}
        
        protected static Dictionary<string, int> GetPrerequisites()
        {
            Dictionary<string, int> prereqs = new Dictionary<string, int>();
            prereqs.Add("agility", 6);
            return prereqs;
        }

        public override int OnCheckRollModifyThreshold(int successThreshold, IEnumerable<IBasicValue<int>> values,
            IEnumerable<string> attackerTags, IEnumerable<string> defenderTags)
        {
            if(defenderTags.Any(tag => tag.Equals("agility", StringComparison.OrdinalIgnoreCase))
            && defenderTags.Any(tag => tag.Equals("defend", StringComparison.OrdinalIgnoreCase))
            && defenderTags.Any(tag => tag.Equals("physical", StringComparison.OrdinalIgnoreCase))
            && values.Any(value => value.Name.Equals(EntityStatistic.AGILITY, StringComparison.OrdinalIgnoreCase)))
            {
                return Math.Max(GlobalConstants.MINIMUM_SUCCESS_THRESHOLD, successThreshold - 1);
            }

            return successThreshold;
        }
    }
}