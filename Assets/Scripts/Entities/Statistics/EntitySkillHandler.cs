using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JoyLib.Code.Entities.Statistics;
using JoyLib.Code.Helpers;

namespace JoyLib.Code.Entities
{
    public class EntitySkillHandler : IEntitySkillHandler
    {
        public IEnumerable<string> SkillsNames => this.Skills.Keys;

        public IEnumerable<IEntitySkill> Values => this.Skills.Values;
        
        protected IDictionary<string, IEntitySkill> Skills { get; set; }
        protected IDictionary<string, IEntitySkill> DefaultSkills { get; set; }
        
        public EntitySkillHandler()
        {
            this.Skills = this.Load().ToDictionary(skill => skill.Name, skill => skill);
        }

        public IDictionary<string, IEntitySkill> GetDefaultSkillBlock()
        {
            return this.DefaultSkills.Copy();
        }

        public IEntitySkill Get(string name)
        {
            return this.Skills.TryGetValue(name, out IEntitySkill skill) ? skill.Copy() : null;
        }

        public bool Add(IEntitySkill value)
        {
            if (this.Skills.ContainsKey(value.Name))
            {
                return false;
            }

            this.Skills.Add(value.Name, value);
            return true;
        }

        public bool Destroy(string key)
        {
            if (this.Skills.ContainsKey(key) == false)
            {
                return false;
            }

            this.Skills[key] = null;
            this.Skills.Remove(key);
            return true;
        }

        public IEnumerable<IEntitySkill> Load()
        {
            List<IEntitySkill> skills = new List<IEntitySkill>();

            string file = (Directory.GetCurrentDirectory() + GlobalConstants.DATA_FOLDER + "Skills/" + "DefaultSkills.json");

            /*
            using (StreamReader reader = new StreamReader(file))
            {
                using (JsonTextReader jsonReader = new JsonTextReader(reader))
                {
                    try
                    {
                        JObject jToken = JObject.Load(jsonReader);

                        if (jToken["Skills"].IsNullOrEmpty() == false)
                        {
                            foreach (JToken child in jToken["Skills"])
                            {
                                skills.Add(new EntitySkill(
                                    (string) child,
                                    0,
                                    GlobalConstants.DEFAULT_SUCCESS_THRESHOLD));
                            }
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

            this.DefaultSkills = skills.ToDictionary(skill => skill.Name, skill => skill);

            return skills;
        }

        public void Dispose()
        {
            string[] keys = this.Skills.Keys.ToArray();
            foreach (string key in keys)
            {
                this.Skills[key] = null;
            }

            this.Skills = null;
        }

        ~EntitySkillHandler()
        {
            this.Dispose();
        }
    }
}
