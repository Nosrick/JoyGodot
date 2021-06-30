using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using Godot.Collections;
using JoyGodot.Assets.Scripts.Entities.Abilities;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.Rollers;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace JoyGodot.Assets.Scripts.Entities.Jobs
{
    public class JobHandler : IJobHandler
    {
        protected List<IJob> m_Jobs;
        
        protected RNG Roller { get; set; }
        
        public JSONValueExtractor ValueExtractor { get; protected set; }
        
        protected IAbilityHandler AbilityHandler { get; set; }

        public JobHandler(
            IAbilityHandler abilityHandler,
            RNG roller)
        {
            this.ValueExtractor = new JSONValueExtractor();
            this.AbilityHandler = abilityHandler;
            this.Roller = roller;
            this.m_Jobs = this.Load().ToList();
        }

        public IJob Get(string jobName)
        {
            if (this.m_Jobs.Any(x => x.Name.Equals(jobName, StringComparison.OrdinalIgnoreCase)))
            {
                IJob job = this.m_Jobs.First(x => x.Name.Equals(jobName, StringComparison.OrdinalIgnoreCase));
                return job.Copy(job);
            }

            return null;
        }

        public bool Add(IJob value)
        {
            if (this.m_Jobs.Any(job => job.Name.Equals(value.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            this.m_Jobs.Add(value);
            return true;
        }

        public bool Destroy(string key)
        {
            IJob toRemove = this.m_Jobs.FirstOrDefault(job =>
                job.Name.Equals(key, StringComparison.OrdinalIgnoreCase));

            return !(toRemove is null) && this.m_Jobs.Remove(toRemove);
        }

        public IJob GetRandom()
        {
            int result = this.Roller.Roll(0, this.m_Jobs.Count);
            return this.m_Jobs[result].Copy(this.m_Jobs[result]);
        }

        public IEnumerable<IJob> Load()
        {
            List<IJob> jobTypes = new List<IJob>();

            string[] files = Directory.GetFiles(
                Directory.GetCurrentDirectory() +
                GlobalConstants.ASSETS_FOLDER + 
                GlobalConstants.DATA_FOLDER + 
                "Jobs", 
                "*.json", 
                SearchOption.AllDirectories);

            foreach (string file in files)
            {
                JSONParseResult result = JSON.Parse(File.ReadAllText(file));

                if (result.Error != Error.Ok)
                {
                    this.ValueExtractor.PrintFileParsingError(result, file);
                    continue;
                }

                if (!(result.Result is Dictionary dictionary))
                {
                    GlobalConstants.ActionLog.Log("Failed to parse JSON from " + file + " into a Dictionary.", LogLevel.Warning);
                    continue;
                }

                ICollection<Dictionary> jobCollection =
                    this.ValueExtractor.GetArrayValuesCollectionFromDictionary<Dictionary>(dictionary, "Jobs");

                foreach (Dictionary job in jobCollection)
                {
                    string name = this.ValueExtractor.GetValueFromDictionary<string>(job, "Name");
                    string description = this.ValueExtractor.GetValueFromDictionary<string>(job, "Description") ?? "NO DESCRIPTION PROVIDED.";
                    IDictionary<string, int> statisticDiscounts = new System.Collections.Generic.Dictionary<string, int>();
                    if (job.Contains("Statistics"))
                    {
                        ICollection<Dictionary> statistics =
                            this.ValueExtractor.GetArrayValuesCollectionFromDictionary<Dictionary>(job, "Statistics");
                        foreach (Dictionary statistic in statistics)
                        {
                            statisticDiscounts.Add(
                                this.ValueExtractor.GetValueFromDictionary<string>(statistic, "Name"),
                                this.ValueExtractor.GetValueFromDictionary<int>(statistic, "Discount"));
                        }
                    }

                    IDictionary<string, int> skillDiscounts = new System.Collections.Generic.Dictionary<string, int>();
                    if (job.Contains("Skills"))
                    {
                        ICollection<Dictionary> skills =
                            this.ValueExtractor.GetArrayValuesCollectionFromDictionary<Dictionary>(job, "Skills");
                        foreach (Dictionary skill in skills)
                        {
                            skillDiscounts.Add(
                                this.ValueExtractor.GetValueFromDictionary<string>(skill, "Name"),
                                this.ValueExtractor.GetValueFromDictionary<int>(skill, "Discount"));
                        }
                    }

                    IDictionary<IAbility, int> abilityCosts = new System.Collections.Generic.Dictionary<IAbility, int>();
                    if (job.Contains("Abilities"))
                    {
                        ICollection<Dictionary> abilities =
                            this.ValueExtractor.GetArrayValuesCollectionFromDictionary<Dictionary>(job, "Abilities");
                        foreach (Dictionary ability in abilities)
                        {
                            abilityCosts.Add(
                                this.AbilityHandler.Get(
                                    this.ValueExtractor.GetValueFromDictionary<string>(ability, "Name")),
                                this.ValueExtractor.GetValueFromDictionary<int>(ability, "Cost"));
                        }
                    }

                    Color abilityIconColour = Colors.White;
                    Color abilityBackgroundColour = Colors.Black;
                    if (job.Contains("Colours"))
                    {
                        Dictionary colours = this.ValueExtractor.GetValueFromDictionary<Dictionary>(job, "Colours");
                        abilityIconColour = new Color(this.ValueExtractor.GetValueFromDictionary<string>(colours, "Icon"));
                        abilityBackgroundColour =
                            new Color(this.ValueExtractor.GetValueFromDictionary<string>(colours, "Background"));
                    }
                                
                    jobTypes.Add(
                        new JobType(
                            name,
                            description,
                            statisticDiscounts,
                            skillDiscounts,
                            abilityCosts, 
                            abilityIconColour,
                            abilityBackgroundColour));
                }
            }
            
            return jobTypes;
        }

        public IEnumerable<IJob> Values => this.m_Jobs ?? (this.m_Jobs = this.Load().ToList());

        public void Dispose()
        {
            this.m_Jobs = null;
        }

        ~JobHandler()
        {
            this.Dispose();
        }
    }
}
