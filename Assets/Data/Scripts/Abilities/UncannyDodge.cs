using System;
using System.Collections.Generic;
using System.Linq;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Abilities;
using JoyGodot.Assets.Scripts.Entities.Statistics;

namespace JoyGodot.Assets.Data.Scripts.Abilities
{
    public class UncannyDodge : AbstractAbility
    {
        public UncannyDodge()
            :base(
                "uncanny dodge",
                "uncannydodge",
                "Reduces any incoming physical damage by your Cunning value.",
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
                GetSprite("uncannydodge"),
                "defend", "success", "cunning", "physical", "passive")
        {}
        
        protected static Dictionary<string, int> GetPrerequisites()
        {
            Dictionary<string, int> prereqs = new Dictionary<string, int>();
            prereqs.Add("agility", 6);
            prereqs.Add("cunning", 6);
            return prereqs;
        }

        public override int OnTakeHit(IEntity attacker, IEntity defender, int damage, IEnumerable<string> attackerTags,
            IEnumerable<string> defenderTags)
        {
            if (defenderTags.Any(tag => tag.Equals("defend", StringComparison.OrdinalIgnoreCase))
                && defenderTags.Any(tag => tag.Equals("physical", StringComparison.OrdinalIgnoreCase)))
            {
                damage -= defender.Statistics[EntityStatistic.CUNNING].Value;
            }

            return damage;
        }
    }
}