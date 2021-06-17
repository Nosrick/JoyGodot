using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Base_Interfaces;
using JoyGodot.Assets.Scripts.Entities.Abilities;
using JoyGodot.Assets.Scripts.Entities.AI.LOS.Providers;
using JoyGodot.Assets.Scripts.Entities.Statistics;

namespace JoyGodot.Assets.Scripts.Entities
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