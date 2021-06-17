using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using JoyGodot.Assets.Scripts.Helpers;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace JoyGodot.Assets.Scripts.Entities.Statistics
{
    public class EntityStatisticHandler : IEntityStatisticHandler
    {
        public IEnumerable<string> StatisticNames => this.Statistics.Keys;

        public IEnumerable<IEntityStatistic> Values => this.Statistics.Values;
        
        public JSONValueExtractor ValueExtractor { get; protected set; }
        
        protected IDictionary<string, IEntityStatistic> Statistics { get; set; }
        protected IDictionary<string, IEntityStatistic> DefaultStatistics { get; set; }

        public EntityStatisticHandler()
        {
            this.ValueExtractor = new JSONValueExtractor();
            this.Statistics = this.Load().ToDictionary(statistic => statistic.Name, stat => stat);
        }

        public IEnumerable<IEntityStatistic> Load()
        {
            string file = Directory.GetCurrentDirectory() + 
                          GlobalConstants.ASSETS_FOLDER + 
                          GlobalConstants.DATA_FOLDER + 
                          "/Statistics/Statistics.json";

            List<IEntityStatistic> statistics = new List<IEntityStatistic>();
            
            JSONParseResult result = JSON.Parse(File.ReadAllText(file));
            
            if (result.Error != Error.Ok)
            {
                this.ValueExtractor.PrintFileParsingError(result, file);
                return statistics;
            }

            if (!(result.Result is Dictionary dictionary))
            {
                GlobalConstants.ActionLog.Log("Failed to parse JSON from " + file + " into a Dictionary.", LogLevel.Warning);
                return statistics;
            }

            ICollection<string> statNames =
                this.ValueExtractor.GetArrayValuesCollectionFromDictionary<string>(dictionary, "Statistics");

            statistics.AddRange(statNames.Select(statName => new EntityStatistic(statName, 0, GlobalConstants.DEFAULT_SUCCESS_THRESHOLD)));

            this.DefaultStatistics = statistics.ToDictionary(statistic => statistic.Name, statistic => statistic);
            
            return statistics;
        }
        
        public IEntityStatistic Get(string name)
        {
            return this.Statistics.TryGetValue(name, out IEntityStatistic statistic) ? statistic.Copy() : null;
        }

        public bool Add(IEntityStatistic value)
        {
            if (this.Statistics.ContainsKey(value.Name))
            {
                return false;
            }

            this.Statistics.Add(value.Name, value);
            return true;
        }

        public bool Destroy(string key)
        {
            if (this.Statistics.ContainsKey(key) == false)
            {
                return false;
            }

            this.Statistics[key] = null;
            this.Statistics.Remove(key);
            return true;
        }

        public IDictionary<string, IEntityStatistic> GetDefaultBlock()
        {
            return this.DefaultStatistics.Copy();
        }

        public void Dispose()
        {
            string[] keys = this.StatisticNames.ToArray();
            foreach (string key in keys)
            {
                this.Statistics[key] = null;
            }

            keys = this.DefaultStatistics.Keys.ToArray();
            foreach (string key in keys)
            {
                this.DefaultStatistics[key] = null;
            }

            this.Statistics = null;
            this.DefaultStatistics = null;
        }

        ~EntityStatisticHandler()
        {
            this.Dispose();
        }
    }
}