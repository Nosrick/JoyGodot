using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Base_Interfaces;

namespace JoyGodot.Assets.Scripts.Entities.Statistics
{
    public interface IEntityStatisticHandler : IHandler<IEntityStatistic, string>
    {
        IEnumerable<string> StatisticNames { get; }

        IDictionary<string, IEntityStatistic> GetDefaultBlock();
    }
}