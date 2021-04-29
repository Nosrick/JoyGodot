using System;
using System.Collections.Generic;
using System.Linq;
using JoyLib.Code.Entities.Statistics;
using JoyLib.Code.Helpers;
using JoyLib.Code.Scripting;

namespace JoyLib.Code.Entities.Abilities
{
    public class AbilityHandler : IAbilityHandler
    {
        protected List<IAbility> Abilities { get; set; }
        
        public JSONValueExtractor ValueExtractor { get; protected set; }

        public IEnumerable<IAbility> Values => this.Abilities;

        public AbilityHandler()
        {
            this.ValueExtractor = new JSONValueExtractor();
            this.Abilities = this.Load().ToList();
        }

        public bool Destroy(string key)
        {
            var found = this.Abilities.FirstOrDefault(ability =>
                ability.InternalName.Equals(key, StringComparison.OrdinalIgnoreCase));
            if (found is null)
            {
                return false;
            }
            this.Abilities.Remove(found);
            return true;
        }

        public IEnumerable<IAbility> Load()
        {
            return ScriptingEngine.Instance.FetchAndInitialiseChildren<IAbility>();
        }

        public IAbility Get(string nameRef)
        {
            if (this.Abilities.Any(x => x.InternalName.Equals(nameRef, StringComparison.OrdinalIgnoreCase)
                                        || x.Name.Equals(nameRef, StringComparison.OrdinalIgnoreCase)))
            {
                return this.Abilities.First(x => x.InternalName.Equals(nameRef, StringComparison.OrdinalIgnoreCase)
                                                 || x.Name.Equals(nameRef, StringComparison.OrdinalIgnoreCase));
            }

            throw new InvalidOperationException("Could not find IAbility with name " + nameRef);
        }

        public bool Add(IAbility value)
        {
            this.Abilities.Add(value);
            return true;
        }

        public IEnumerable<IAbility> GetAvailableAbilities(IEntity actor)
        {
            List<IBasicValue<int>> data = new List<IBasicValue<int>>();
            data.AddRange(actor.Statistics.Values);
            data.AddRange(actor.Skills.Values);
            data.AddRange(actor.DerivedValues.Values);

            return this.Abilities.Where(ability => ability.MeetsPrerequisites(data));
        }

        public IEnumerable<IAbility> GetAvailableAbilities(
            IEntityTemplate template,
            ICollection<IEntityStatistic> stats,
            ICollection<IEntitySkill> skills,
            ICollection<IDerivedValue> derivedValues)
        {
            List<IBasicValue<int>> data = new List<IBasicValue<int>>();
            data.AddRange(stats);
            data.AddRange(skills);
            data.AddRange(derivedValues);

            return this.Abilities.Where(ability => ability.MeetsPrerequisites(data));
        }

        public void Dispose()
        {
            this.Abilities = null;
        }
    }
}