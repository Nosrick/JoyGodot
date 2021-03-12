using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JoyLib.Code.Helpers;

namespace JoyLib.Code.Entities.Statistics
{
    public class EntityStatisticHandler : IEntityStatisticHandler
    {
        public IEnumerable<string> StatisticNames => this.Statistics.Keys;

        public IEnumerable<IEntityStatistic> Values => this.Statistics.Values;
        
        protected IDictionary<string, IEntityStatistic> Statistics { get; set; }
        protected IDictionary<string, IEntityStatistic> DefaultStatistics { get; set; }

        public EntityStatisticHandler()
        {
            this.Statistics = this.Load().ToDictionary(statistic => statistic.Name, stat => stat);
        }

        public IEnumerable<IEntityStatistic> Load()
        {
            string file = Directory.GetCurrentDirectory() + GlobalConstants.DATA_FOLDER + "/Statistics/Statistics.json";

            List<IEntityStatistic> statistics = new List<IEntityStatistic>();

            /*
            using (StreamReader reader = new StreamReader(file))
            {
                using (JsonTextReader jsonReader = new JsonTextReader(reader))
                {
                    try
                    {
                        JObject jToken = JObject.Load(jsonReader);

                        if (jToken["Statistics"].IsNullOrEmpty() == false)
                        {
                            statistics.AddRange(jToken["Statistics"].Select(child =>
                                new EntityStatistic((string) child, 0, GlobalConstants.DEFAULT_SUCCESS_THRESHOLD)));
                        }
                    }
                    catch (Exception e)
                    {
                        GlobalConstants.ActionLog.AddText("Could not load skills from " + file);
                        GlobalConstants.ActionLog.StackTrace(e);
                    }
                    finally
                    {
                        jsonReader.Close();
                        reader.Close();
                    }
                }
            }
            */

            this.DefaultStatistics = statistics.ToDictionary(stat => stat.Name, statistic => statistic);
            
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