using System.Collections.Generic;

namespace JoyLib.Code.Entities.Statistics
{
    public interface IEntityStatisticHandler : IHandler<IEntityStatistic, string>
    {
        IEnumerable<string> StatisticNames { get; }

        IDictionary<string, IEntityStatistic> GetDefaultBlock();
    }
}