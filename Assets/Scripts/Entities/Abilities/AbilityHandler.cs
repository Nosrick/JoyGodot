using System;
using System.Collections.Generic;
using System.Linq;
using JoyLib.Code.Entities.Statistics;
using JoyLib.Code.Scripting;

namespace JoyLib.Code.Entities.Abilities
{
    public class AbilityHandler : IAbilityHandler
    {
        protected List<IAbility> Abilities { get; set; }

        public IEnumerable<IAbility> Values => this.Abilities;

        public AbilityHandler()
        {
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
            IEnumerable<string> prereqs = this.Abilities.SelectMany(ability => ability.Prerequisites.Select(pair => pair.Key)).Distinct();

            IEnumerable<Tuple<string, int>> data = actor.GetData(prereqs);

            return this.Abilities.Where(ability => ability.MeetsPrerequisites(data));
        }

        public IEnumerable<IAbility> GetAvailableAbilities(IEntityTemplate template,
            IDictionary<string, IEntityStatistic> stats,
            IDictionary<string, IEntitySkill> skills)
        {
            List<Tuple<string, int>> data =
                stats.Select(stat => new Tuple<string, int>(stat.Key, stat.Value.Value)).ToList();

            data.AddRange(skills.Select(skill => new Tuple<string, int>(skill.Key, skill.Value.Value)));
            data.Add(new Tuple<string, int>(template.CreatureType, 1));

            return this.Abilities.Where(ability => ability.MeetsPrerequisites(data));
        }

        public void Dispose()
        {
            this.Abilities = null;
        }
    }
}