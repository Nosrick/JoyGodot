using System;
using System.Collections.Generic;
using System.Linq;
using JoyLib.Code.Entities.Statistics;

namespace JoyLib.Code.Entities.Abilities
{
    public class Distraction : AbstractAbility
    {
        public Distraction()
            : base(
                "distraction",
                "distraction",
                "Roll at advantage when making a mental attack.",
                false,
                0,
                0,
                0,
                false,
                new string[0],
                GetCosts(),
                GetPrerequisites(),
                AbilityTarget.Ranged,
                5,
                GetSprite("distraction"),
                new []{ "attack", "mental", "threshold", "passive" })
        {}
        
        protected static Tuple<string, int>[] GetCosts()
        {
            List<Tuple<string, int>> costs = new List<Tuple<string, int>>();
            costs.Add(new Tuple<string, int>("mana", 5));
            return costs.ToArray();
        }

        protected static Dictionary<string, int> GetPrerequisites()
        {
            Dictionary<string, int> prereqs = new Dictionary<string, int>();
            prereqs.Add("thief", 1);
            prereqs.Add("personality", 6);
            return prereqs;
        }

        public override int OnCheckRollModifyThreshold(
            int successThreshold, 
            IEnumerable<IBasicValue<int>> values, 
            IEnumerable<string> attackerTags,
            IEnumerable<string> defenderTags)
        {
            if (attackerTags.Any(tag => tag.Equals("attack", StringComparison.OrdinalIgnoreCase))
                && attackerTags.Any(tag => tag.Equals("mental", StringComparison.OrdinalIgnoreCase))
                && attackerTags.Any(tag => tag.Equals(this.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return Math.Max(GlobalConstants.MINIMUM_SUCCESS_THRESHOLD, successThreshold - 1);
            }
            
            return successThreshold;
        }
    }
}