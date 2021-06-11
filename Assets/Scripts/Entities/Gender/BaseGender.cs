using System;
using Godot.Collections;

namespace JoyLib.Code.Entities.Gender
{
    [Serializable]
    public class BaseGender : IGender
    {
         
        public string Possessive { get; protected set; }
         
        public string PersonalSubject { get; protected set; }
         
        public string PersonalObject { get; protected set; }
         
        public string Reflexive { get; protected set; }

         
        public string PossessivePlural { get; protected set; }

         
        public string ReflexivePlural { get; protected set; }
         
        public string Name { get; protected set; }

         
        public string IsOrAre { get; protected set; }

        public BaseGender()
        {}
        
        public BaseGender(
            string name,
            string possessive,
            string personalSubject,
            string personalObject,
            string reflexive,
            string possessivePlural,
            string reflexivePlural,
            string isOrAre)
        {
            this.Name = name;
            this.Possessive = possessive;
            this.PossessivePlural = possessivePlural;
            this.PersonalSubject = personalSubject;
            this.PersonalObject = personalObject;
            this.Reflexive = reflexive;
            this.ReflexivePlural = reflexivePlural;
            this.IsOrAre = isOrAre;
        }

        public Dictionary Save()
        {
            Dictionary saveDict = new Dictionary
            {
                {"Name", this.Name},
                {"Possessive", this.Possessive},
                {"PossessivePlural", this.PossessivePlural},
                {"PersonalSubject", this.PersonalSubject},
                {"PersonalObject", this.PersonalObject},
                {"Reflexive", this.Reflexive},
                {"ReflexivePlural", this.ReflexivePlural},
                {"IsOrAre", this.IsOrAre}
            };
            
            return saveDict;
        }

        public void Load(Dictionary data)
        {
            var valueExtractor = GlobalConstants.GameManager.GenderHandler.ValueExtractor;

            this.Name = valueExtractor.GetValueFromDictionary<string>(data, "Name");
            this.Possessive = valueExtractor.GetValueFromDictionary<string>(data, "Possessive");
            this.PossessivePlural = valueExtractor.GetValueFromDictionary<string>(data, "PossessivePlural");
            this.PersonalSubject = valueExtractor.GetValueFromDictionary<string>(data, "PersonalSubject");
            this.PersonalObject = valueExtractor.GetValueFromDictionary<string>(data, "PersonalObject");
            this.Reflexive = valueExtractor.GetValueFromDictionary<string>(data, "Reflexive");
            this.ReflexivePlural = valueExtractor.GetValueFromDictionary<string>(data, "ReflexivePlural");
            this.IsOrAre = valueExtractor.GetValueFromDictionary<string>(data, "IsOrAre");
        }
    }
}