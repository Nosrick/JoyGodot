using System.Collections.Generic;
using JoyLib.Code.Collections;
using JoyLib.Code.Entities.Needs;
using JoyLib.Code.Entities.Statistics;

namespace JoyLib.Code.Entities
{
    public interface IEntitySkillHandler : IHandler<IEntitySkill, string>
    {
        IEnumerable<string> SkillsNames { get; }
        
        IDictionary<string, IEntitySkill> GetDefaultSkillBlock();
    }
}