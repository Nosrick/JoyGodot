using System.Collections.Generic;
using JoyLib.Code.Entities.Statistics;

namespace JoyLib.Code.Entities.Abilities
{
    public interface IAbilityHandler : IHandler<IAbility, string>
    {
        IEnumerable<IAbility> GetAvailableAbilities(IEntity actor);

        IEnumerable<IAbility> GetAvailableAbilities(
            IEntityTemplate template, 
            IDictionary<string, IEntityStatistic> stats, 
            IDictionary<string, IEntitySkill> skills);
    }
}