using System;
using System.Collections.Generic;
using System.Linq;
using JoyLib.Code.Entities;
using JoyLib.Code.Entities.Needs;
using JoyLib.Code.Entities.Statistics;

namespace JoyLib.Code.Conversation.Subengines.Rumours
{
    public class SkillParameterProcessor : IParameterProcessor
    {
        protected IEntitySkillHandler SkillHandler
        {
            get;
            set;
        }

        protected INeedHandler NeedHandler
        {
            get;
            set;
        }

        protected IDictionary<string, IEntitySkill> DefaultSkillBlock
        {
            get;
            set;
        }

        public SkillParameterProcessor()
        {
            this.SkillHandler = GlobalConstants.GameManager.SkillHandler;
            this.NeedHandler = GlobalConstants.GameManager.NeedHandler;
            this.DefaultSkillBlock = this.SkillHandler.GetDefaultSkillBlock();
        }
        
        public bool CanParse(string parameter)
        {
            if (parameter.Equals("skills", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return this.DefaultSkillBlock.ContainsKey(parameter);
        }

        public string Parse(string parameter, IJoyObject participant)
        {
            if (!(participant is IEntity entity))
            {
                return "";
            }

            if (parameter.Equals("skills", StringComparison.OrdinalIgnoreCase)
                || entity.Skills.ContainsKey(parameter))
            {
                IEnumerable<Tuple<string, object>> values = entity.GetData(new string[] {parameter});

                return values.OrderByDescending(tuple => tuple.Item2)
                    .First()
                    .Item1;
            }

            return "";
        }
    }
}