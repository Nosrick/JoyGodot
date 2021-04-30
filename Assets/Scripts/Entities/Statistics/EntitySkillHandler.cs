using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using JoyLib.Code.Entities.Statistics;
using JoyLib.Code.Helpers;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace JoyLib.Code.Entities
{
    public class EntitySkillHandler : IEntitySkillHandler
    {
        public IEnumerable<string> SkillsNames => this.Skills.Keys;

        public IEnumerable<IEntitySkill> Values => this.Skills.Values;
        
        public JSONValueExtractor ValueExtractor { get; protected set; }
        
        protected IDictionary<string, IEntitySkill> Skills { get; set; }
        protected IDictionary<string, IEntitySkill> DefaultSkills { get; set; }
        
        public EntitySkillHandler()
        {
            this.ValueExtractor = new JSONValueExtractor();
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

            string file = (Directory.GetCurrentDirectory() + 
                           GlobalConstants.ASSETS_FOLDER + 
                           GlobalConstants.DATA_FOLDER + 
                           "Skills/" + "DefaultSkills.json");
            
            JSONParseResult result = JSON.Parse(File.ReadAllText(file));
            
            if (result.Error != Error.Ok)
            {
                this.ValueExtractor.PrintFileParsingError(result, file);
                return skills;
            }

            if (!(result.Result is Dictionary dictionary))
            {
                GlobalConstants.ActionLog.Log("Failed to parse JSON from " + file + " into a Dictionary.", LogLevel.Warning);
                return skills;
            }

            ICollection<string> skillNames =
                this.ValueExtractor.GetArrayValuesCollectionFromDictionary<string>(dictionary, "Skills");

            skills.AddRange(skillNames.Select(statName => new EntitySkill(statName, 0, GlobalConstants.DEFAULT_SUCCESS_THRESHOLD)));

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
