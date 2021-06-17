using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Base_Interfaces;

namespace JoyGodot.Assets.Scripts.Entities.Statistics
{
    public interface IEntitySkillHandler : IHandler<IEntitySkill, string>
    {
        IEnumerable<string> SkillsNames { get; }
        
        IDictionary<string, IEntitySkill> GetDefaultSkillBlock();
    }
}