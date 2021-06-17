using System;
using System.Collections.Generic;
using System.Linq;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Abilities;
using JoyGodot.Assets.Scripts.Entities.Items;
using JoyGodot.Assets.Scripts.Entities.Statistics;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.Rollers;

namespace JoyGodot.Assets.Scripts.Combat
{
    public class CombatEngine : ICombatEngine
    {
        protected IRollable Roller { get; set; }

        public CombatEngine(IRollable roller = null)
        {
            this.Roller = roller is null ? new RNG() : roller;
        }

        public int MakeAttack(IEntity attacker,
            IEntity defender,
            IEnumerable<string> attackerTags,
            IEnumerable<string> defenderTags)
        {
            List<IRollableValue<int>> attackerStuff = attacker.Statistics
                .Where(pair => attackerTags.Any(tag => tag.Equals(pair.Key, StringComparison.OrdinalIgnoreCase)))
                .Select(pair => (IRollableValue<int>) pair.Value)
                .ToList();

            attackerStuff.AddRange(attacker.Skills
                .Where(pair => attackerTags.Any(tag => tag.Equals(pair.Key, StringComparison.OrdinalIgnoreCase)))
                .Select(pair => pair.Value));

            List<IRollableValue<int>> defenderStuff = defender.Statistics
                .Where(pair => defenderTags.Any(tag => tag.Equals(pair.Key, StringComparison.OrdinalIgnoreCase)))
                .Select(pair => (IRollableValue<int>) pair.Value)
                .ToList();

            defenderStuff.AddRange(defender.Skills
                .Where(pair => defenderTags.Any(tag => tag.Equals(pair.Key, StringComparison.OrdinalIgnoreCase)))
                .Select(pair => pair.Value));

            List<IItemInstance> attackerWeapons = attacker.Equipment.Contents.Where(instance =>
                instance.Tags.Any(tag => tag.Equals("weapon", StringComparison.OrdinalIgnoreCase))
                && instance.Tags.Intersect(attackerTags).Any())
                .ToList();

            List<IItemInstance> defenderArmour = defender.Equipment.Contents.Where(instance =>
                    instance.Tags.Any(tag => tag.Equals("armour", StringComparison.OrdinalIgnoreCase)
                    && instance.Tags.Intersect(defenderTags).Any()))
                .ToList();

            List<IAbility> attackerAbilities = attacker.Abilities.Where(ability =>
                ability.Tags.Intersect(attackerTags).Any()).ToList();
            attackerAbilities.AddRange(attacker.Equipment.Contents
                .SelectMany(instance => instance.AllAbilities)
                .Where(ability => ability.Tags.Intersect(attackerTags).Any()));

            List<IAbility> defenderAbilities = defender.Abilities.Where(ability =>
                ability.Tags.Intersect(attackerTags).Any()).ToList();
            defenderAbilities.AddRange(defender.Equipment.Contents
                .SelectMany(instance => instance.AllAbilities)
                .Where(ability => ability.Tags.Intersect(defenderTags).Any()));

            attackerAbilities.ForEach(ability => ability.OnAttack(
                attacker, 
                defender, 
                attackerTags, 
                defenderTags));
            
            int attackerSuccesses = 0;
            int totalDice = 0;
            int successThreshold = GlobalConstants.DEFAULT_SUCCESS_THRESHOLD;
            foreach (IRollableValue<int> stat in attackerStuff)
            {
                totalDice += stat.Value;
                successThreshold = Math.Min(successThreshold, stat.SuccessThreshold);
            }
            
            attackerAbilities.ForEach(ability => totalDice = ability.OnCheckRollModifyDice(
                totalDice, 
                attackerStuff, 
                attackerTags, 
                defenderTags));
            attackerAbilities.ForEach(ability =>
                successThreshold = ability.OnCheckRollModifyThreshold(
                    successThreshold, 
                    attackerStuff, 
                    attackerTags, 
                    defenderTags));
                
            attackerSuccesses = this.Roller.RollSuccesses(
                totalDice,
                successThreshold);
            
            attackerAbilities.ForEach(ability =>
                attackerSuccesses = ability.OnCheckSuccess(
                    attackerSuccesses, 
                    attackerStuff, 
                    attackerTags, 
                    defenderTags));

            int defenderSuccesses = 0;
            totalDice = 0;
            successThreshold = GlobalConstants.DEFAULT_SUCCESS_THRESHOLD;
            foreach (IRollableValue<int> stat in defenderStuff)
            {
                totalDice += stat.Value;
                successThreshold = Math.Min(successThreshold, stat.SuccessThreshold);
            }

            defenderAbilities.ForEach(ability => totalDice = ability.OnCheckRollModifyDice(
                totalDice, 
                defenderStuff, 
                attackerTags, 
                defenderTags));
            defenderAbilities.ForEach(ability =>
                successThreshold = ability.OnCheckRollModifyThreshold(
                    successThreshold, 
                    defenderStuff, 
                    attackerTags, 
                    defenderTags));

            defenderSuccesses = this.Roller.RollSuccesses(
                totalDice,
                successThreshold);

            defenderAbilities.ForEach(ability =>
                defenderSuccesses = ability.OnCheckSuccess(
                    defenderSuccesses, 
                    defenderStuff, 
                    attackerTags, 
                    defenderTags));

            defenderAbilities.ForEach(ability => defenderSuccesses = ability.OnTakeHit(
                attacker, 
                defender, 
                attackerSuccesses, 
                attackerTags, 
                defenderTags));

            int result = attackerSuccesses - defenderSuccesses;
            if (result > 0)
            {
                result += attackerWeapons.Select(instance => instance.Efficiency).Sum();
                result -= defenderArmour.Select(instance => instance.Efficiency).Sum();
                result = Math.Max(0, result);
            }

            GlobalConstants.ActionLog.Log(
                attacker.JoyName + " attacks " + defender.JoyName + " for " + result + " damage.",
                LogLevel.Gameplay);
            return result;
        }
    }
}