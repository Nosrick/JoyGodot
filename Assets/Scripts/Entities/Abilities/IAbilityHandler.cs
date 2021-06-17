using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Base_Interfaces;
using JoyGodot.Assets.Scripts.Entities.Statistics;

namespace JoyGodot.Assets.Scripts.Entities.Abilities
{
    public interface IAbilityHandler : IHandler<IAbility, string>
    {
        IEnumerable<IAbility> GetAvailableAbilities(IEntity actor);

        IEnumerable<IAbility> GetAvailableAbilities(IEntityTemplate template,
            ICollection<IEntityStatistic> stats,
            ICollection<IEntitySkill> skills, ICollection<IDerivedValue> derivedValues);
    }
}