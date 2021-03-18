using System;
using System.Collections.Generic;
using System.Linq;
using JoyLib.Code.Entities.Statistics;

namespace JoyLib.Code.Entities.Abilities
{
    public class Indomitable : AbstractAbility
    {
        public Indomitable()
            : base(
                "indomitable",
                "indomitable",
                "Roll at advantage when defending against Social attacks.",
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
                GetSprite("indomitable"),
                "defend", "social", "threshold", "passive")
        {}
        
        protected static Dictionary<string, int> GetPrerequisites()
        {
            Dictionary<string, int> prereqs = new Dictionary<string, int>();
            prereqs.Add("wit", 7);
            return prereqs;
        }

        public override int OnCheckRollModifyThreshold(int successThreshold, IEnumerable<IBasicValue<int>> values, IEnumerable<string> attackerTags,
            IEnumerable<string> defenderTags)
        {
            if (defenderTags.Any(tag => tag.Equals("social"))
                && defenderTags.Any(tag => tag.Equals("defend")))
            {
                return Math.Max(GlobalConstants.MINIMUM_SUCCESS_THRESHOLD, successThreshold - 1);
            }
                
            return successThreshold;
        }
    }
}