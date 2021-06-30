using System;
using JoyGodot.Assets.Scripts.Conversation.Subengines.Rumours.Parameters;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Gender;
using JoyGodot.Assets.Scripts.JoyObject;

namespace JoyGodot.Assets.Data.Scripts.Rumours.Parameters
{
    public class GenderParameterProcessor : IParameterProcessor
    {
        public bool CanParse(string parameter)
        {
            return parameter.IndexOf("gender", StringComparison.OrdinalIgnoreCase) > -1;
        }

        public string Parse(string parameter, IJoyObject participant)
        {
            if (!this.CanParse(parameter))
            {
                return "";
            }

            if (!(participant is Entity entity))
            {
                return "";
            }

            string[] split = parameter.Split('/');
            string pronoun = split[1];

            IGender gender = entity.Gender;

            switch (pronoun.ToLower())
            {
                case "possessive":
                    return gender.Possessive;
                
                case "personalsubject":
                    return gender.PersonalSubject;
                
                case "personalobject":
                    return gender.PersonalObject;
                
                case "reflexive":
                    return gender.Reflexive;
                
                case "possessiveplural":
                    return gender.PossessivePlural;
                
                case "reflexiveplural":
                    return gender.ReflexivePlural;
                
                default:
                    return gender.PersonalObject;
            }
        }
    }
}