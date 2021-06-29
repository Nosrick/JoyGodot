using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using Godot.Collections;
using JoyGodot.Assets.Scripts.Entities.Abilities;
using JoyGodot.Assets.Scripts.Entities.AI.LOS.Providers;
using JoyGodot.Assets.Scripts.Entities.Statistics;
using JoyGodot.Assets.Scripts.Helpers;
using Array = Godot.Collections.Array;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace JoyGodot.Assets.Scripts.Entities
{
    public class EntityTemplateHandler : IEntityTemplateHandler
    {
        protected List<IEntityTemplate> m_Templates;
        protected IEntitySkillHandler SkillHandler { get; set; }
        protected IVisionProviderHandler VisionProviderHandler { get; set; }
        protected IAbilityHandler AbilityHandler { get; set; }

        public JSONValueExtractor ValueExtractor { get; protected set; }

        public IEnumerable<IEntityTemplate> Values
        {
            get
            {
                if (this.m_Templates is null)
                {
                    this.m_Templates = this.Load().ToList();
                }

                return this.m_Templates;
            }
        }

        public EntityTemplateHandler(
            IEntitySkillHandler skillHandler,
            IVisionProviderHandler visionProviderHandler,
            IAbilityHandler abilityHandler)
        {
            this.ValueExtractor = new JSONValueExtractor();
            this.AbilityHandler = abilityHandler;
            this.VisionProviderHandler = visionProviderHandler;
            this.SkillHandler = skillHandler;
            this.m_Templates = this.Load().ToList();
        }

        public IEnumerable<IEntityTemplate> Load()
        {
            List<IEntityTemplate> entities = new List<IEntityTemplate>();

            string[] files =
                Directory.GetFiles(
                    Directory.GetCurrentDirectory() +
                    GlobalConstants.ASSETS_FOLDER + 
                    GlobalConstants.DATA_FOLDER + 
                    "Entities", 
                    "*.json",
                    SearchOption.AllDirectories);

            foreach (string file in files)
            {
                JSONParseResult result = JSON.Parse(File.ReadAllText(file));

                if (result.Error != Error.Ok)
                {
                    GlobalConstants.ActionLog.Log("Could not load entity templates from " + file, LogLevel.Warning);
                    GlobalConstants.ActionLog.Log(result.ErrorString, LogLevel.Warning);
                    GlobalConstants.ActionLog.Log("On line: " + result.ErrorLine, LogLevel.Warning);
                    continue;
                }

                if (!(result.Result is Dictionary dictionary))
                {
                    GlobalConstants.ActionLog.Log("Could not parse JSON to Dictionary from " + file, LogLevel.Warning);
                    continue;
                }

                Array templateArray = this.ValueExtractor.GetValueFromDictionary<Array>(dictionary, "Entities");

                foreach (Dictionary templateDict in templateArray)
                {
                    string creatureType =
                        this.ValueExtractor.GetValueFromDictionary<string>(templateDict, "CreatureType");
                    string type = this.ValueExtractor.GetValueFromDictionary<string>(templateDict, "Type");
                    string visionType = this.ValueExtractor.GetValueFromDictionary<string>(templateDict, "VisionType") ?? "diurnal vision";
                    IVision vision = this.VisionProviderHandler.Get(visionType);

                    IDictionary<string, IEntityStatistic> statistics =
                        new System.Collections.Generic.Dictionary<string, IEntityStatistic>();
                    ICollection<Dictionary> statisticsCollection =
                        this.ValueExtractor.GetCollectionFromArray<Dictionary>(
                            this.ValueExtractor.GetValueFromDictionary<Array>(templateDict, "Statistics"));
                    foreach (Dictionary innerDict in statisticsCollection)
                    {
                        string statName = this.ValueExtractor.GetValueFromDictionary<string>(innerDict, "Name");
                        int statValue = this.ValueExtractor.GetValueFromDictionary<int>(innerDict, "Value");
                        int threshold = innerDict.Contains("Threshold") 
                        ? this.ValueExtractor.GetValueFromDictionary<int>(innerDict, "Threshold")
                        : GlobalConstants.DEFAULT_SUCCESS_THRESHOLD;

                        IEntityStatistic statistic = GlobalConstants.GameManager.StatisticHandler.Get(statName);
                        statistic.SetValue(statValue);
                        statistic.SetThreshold(threshold);
                        
                        statistics.Add(
                            statName,
                            statistic);
                    }

                    IDictionary<string, IEntitySkill> skills =
                        new System.Collections.Generic.Dictionary<string, IEntitySkill>();
                    if (templateDict.Contains("Skills"))
                    {
                        ICollection<Dictionary> skillCollection =
                            this.ValueExtractor.GetCollectionFromArray<Dictionary>(
                                this.ValueExtractor.GetValueFromDictionary<Array>(templateDict, "Skills"));
                        foreach (Dictionary innerDict in skillCollection)
                        {
                            string skillName = this.ValueExtractor.GetValueFromDictionary<string>(innerDict, "Name");
                            int skillValue = this.ValueExtractor.GetValueFromDictionary<int>(innerDict, "Value");
                            int threshold = innerDict.Contains("Threshold")
                                ? this.ValueExtractor.GetValueFromDictionary<int>(innerDict, "Threshold")
                                : GlobalConstants.DEFAULT_SUCCESS_THRESHOLD;

                            IEntitySkill skill = GlobalConstants.GameManager.SkillHandler.Get(skillName);
                            skill.SetValue(skillValue);
                            skill.SetThreshold(threshold);
                            skills.Add(
                                skillName,
                                skill);
                        }
                    }

                    ICollection<string> tags = this.ValueExtractor.GetCollectionFromArray<string>(
                        this.ValueExtractor.GetValueFromDictionary<Array>(templateDict, "Tags"));
                    
                    ICollection<string> slots = this.ValueExtractor.GetCollectionFromArray<string>(
                        this.ValueExtractor.GetValueFromDictionary<Array>(templateDict, "Slots"));
                    
                    ICollection<string> needs = this.ValueExtractor.GetCollectionFromArray<string>(
                        this.ValueExtractor.GetValueFromDictionary<Array>(templateDict, "Needs"));

                    ICollection<IAbility> abilities = new List<IAbility>();
                    if (templateDict.Contains("Abilities"))
                    {
                        ICollection<string> abilityNames = this.ValueExtractor.GetCollectionFromArray<string>(
                            this.ValueExtractor.GetValueFromDictionary<Array>(templateDict, "Abilities"));

                        foreach (string name in abilityNames)
                        {
                            abilities.Add(this.AbilityHandler.Get(name));
                        }
                    }

                    int size = this.ValueExtractor.GetValueFromDictionary<int>(templateDict, "Size");
                    
                    entities.Add(
                        new EntityTemplate(
                            statistics,
                            skills,
                            needs.ToArray(),
                            abilities.ToArray(),
                            slots.ToArray(),
                            size,
                            vision,
                            creatureType,
                            type,
                            tags.ToArray()));
                }
            }

            return entities;
        }

        public IEntityTemplate Get(string type)
        {
            var value = this.Values.FirstOrDefault(x => x.CreatureType.Equals(type, StringComparison.OrdinalIgnoreCase));

            if (value is null)
            {
                throw new InvalidOperationException("Could not find entity template of type " + type);
            }

            return value;
        }

        public bool Add(IEntityTemplate value)
        {
            this.m_Templates.Add(value);
            return true;
        }

        public bool Destroy(string key)
        {
            IEntityTemplate found = this.m_Templates.FirstOrDefault(template =>
                template.CreatureType.Equals(key, StringComparison.OrdinalIgnoreCase));
            
            return !(found is null) && this.m_Templates.Remove(found);
        }

        public IEntityTemplate GetRandom()
        {
            return GlobalConstants.GameManager.Roller.SelectFromCollection(this.m_Templates);
        }

        public void Dispose()
        {
            this.m_Templates = null;
        }

        ~EntityTemplateHandler()
        {
            this.Dispose();
        }
    }
}
