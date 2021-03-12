using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JoyLib.Code.Entities.Abilities;
using JoyLib.Code.Entities.AI.LOS.Providers;
using JoyLib.Code.Entities.Statistics;

namespace JoyLib.Code.Entities
{
    public class EntityTemplateHandler : IEntityTemplateHandler
    {
        protected List<IEntityTemplate> m_Templates;
        protected IEntitySkillHandler SkillHandler { get; set; }
        protected IVisionProviderHandler VisionProviderHandler { get; set; }
        protected IAbilityHandler AbilityHandler { get; set; }

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
                    Directory.GetCurrentDirectory() + GlobalConstants.DATA_FOLDER + "Entities", 
                    "*.json",
                    SearchOption.AllDirectories);

            foreach (string file in files)
            {
                /*
                using (StreamReader reader = new StreamReader(file))
                {
                    using (JsonTextReader jsonReader = new JsonTextReader(reader))
                    {
                        try
                        {
                            JObject jToken = JObject.Load(jsonReader);

                            if (jToken.IsNullOrEmpty())
                            {
                                continue;
                            }

                            foreach (JToken child in jToken["Entities"])
                            {
                                string creatureType = (string) child["CreatureType"];
                                string type = (string) child["Type"];
                                string visionType = (string) child["VisionType"];
                                IVision vision = this.VisionProviderHandler.Get(visionType);
                                int size = (int) (child["Size"] ?? 0);

                                IDictionary<string, IEntityStatistic> statistics = child["Statistics"] is null
                                    ? new Dictionary<string, IEntityStatistic>()
                                    : child["Statistics"].Select(
                                            token => new EntityStatistic(
                                                (string) token["Name"],
                                                (int) token["Value"],
                                                (int) ((token["Threshold"]) ??
                                                       GlobalConstants.DEFAULT_SUCCESS_THRESHOLD)))
                                        .ToDictionary(statistic => statistic.Name, statistic => (IEntityStatistic) statistic);

                                IDictionary<string, IEntitySkill> skills = child["Skills"] is null
                                    ? new Dictionary<string, IEntitySkill>()
                                    : child["Skills"].Select(
                                            token => new EntitySkill(
                                                (string) token["Name"],
                                                (int) (token["Value"] ?? 0),
                                                (int) (token["Threshold"] ?? GlobalConstants.DEFAULT_SUCCESS_THRESHOLD)))
                                        .ToDictionary(skill => skill.Name, skill => (IEntitySkill) skill);

                                IEnumerable<string> tags = child["Tags"] is null
                                    ? new List<string>()
                                    : child["Tags"].Select(token => (string) token);

                                IEnumerable<string> slots = child["Slots"] is null
                                    ? new List<string>()
                                    : child["Slots"].Select(token => (string) token);

                                IEnumerable<string> needs = child["Needs"] is null
                                    ? new List<string>()
                                    : child["Needs"].Select(token => (string) token);

                                IEnumerable<IAbility> abilities = child["Abilities"] is null
                                    ? new List<IAbility>()
                                    : child["Abilities"].Select(token =>
                                        this.AbilityHandler.Get((string) token));
                                
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
                        catch (Exception e)
                        {
                            GlobalConstants.ActionLog.AddText("Failed to load entities in " + file);
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
            }

            return entities;
        }

        public IEntityTemplate Get(string type)
        {
            if(this.Values.Any(x => x.CreatureType == type))
            {
                return this.Values.First(x => x.CreatureType == type);
            }

            throw new InvalidOperationException("Could not find entity template of type " + type);
        }

        public bool Add(IEntityTemplate value)
        {
            throw new NotImplementedException();
        }

        public bool Destroy(string key)
        {
            throw new NotImplementedException();
        }

        public IEntityTemplate GetRandom()
        {
            //int result = GlobalConstants.GameManager.Roller.Roll(0, this.m_Templates.Count);
            return this.m_Templates[0];
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
