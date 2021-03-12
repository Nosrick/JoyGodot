using System.Collections.Generic;
using JoyLib.Code.Entities.Abilities;
using JoyLib.Code.Entities.AI.LOS.Providers;
using JoyLib.Code.Entities.Statistics;

namespace JoyLib.Code.Entities
{
    public interface IEntityTemplate : ITagged
    {
        IEnumerable<string> Slots { get; }
        IDictionary<string, IEntityStatistic> Statistics { get; }
        IDictionary<string, IEntitySkill> Skills { get; }
        IEnumerable<string> Needs { get; }
        IEnumerable<IAbility> Abilities { get; }
        int Size { get; }
        bool Sentient { get; }
        IVision VisionType { get; }
        string CreatureType { get; }
        string JoyType { get; }
    }
}