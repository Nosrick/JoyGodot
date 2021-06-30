using System;
using System.Collections.Generic;
using System.Linq;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Abilities;
using JoyGodot.Assets.Scripts.Entities.Statistics;
using JoyGodot.Assets.Scripts.JoyObject;

namespace JoyGodot.Assets.Data.Scripts.Abilities
{
    public class Backdraft : AbstractAbility
    {
        public Backdraft()
            : base(
                "Backdraft",
                "backdraft",
                "Deal heavy damage to a target, but take a portion of the damage dealt.",
                false,
                0,
                0,
                0,
                false,
                new string[0], 
                GetCosts(),
                GetPrerequisites(), 
                AbilityTarget.Adjacent,
                1,
                GetSprite("backdraft"),
                new []{ "attack", "success", "active" })
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
            prereqs.Add("warrior", 1);
            return prereqs;
        }

        public override bool OnUse(IEntity user, IJoyObject target)
        {
            int hp = user.DerivedValues[DerivedValueName.HITPOINTS].Value;
            int selfDamage = Math.Max(1, (hp / 5));
            user.ModifyValue(DerivedValueName.HITPOINTS, -selfDamage);
            return true;
        }

        public override int OnCheckSuccess(int successes, IEnumerable<IBasicValue<int>> values,
            IEnumerable<string> attackerTags, IEnumerable<string> defenderTags)
        {
            if (attackerTags.Any(tag => tag.Equals("physical", StringComparison.OrdinalIgnoreCase))
            && attackerTags.Any(tag => tag.Equals("attack", StringComparison.OrdinalIgnoreCase))
            && attackerTags.Any(tag => tag.Equals(this.InternalName, StringComparison.OrdinalIgnoreCase)))
            {
                return successes *= 2;
            }
            return successes;
        }
    }
}