using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JoyLib.Code.Helpers;

namespace JoyLib.Code.Entities.Gender
{
    public class GenderHandler : IGenderHandler
    {
        protected HashSet<IGender> Genders { get; set; }
        
        public JSONValueExtractor ValueExtractor { get; protected set; }

        public IEnumerable<IGender> Values => this.Genders;

        public GenderHandler()
        {
            this.ValueExtractor = new JSONValueExtractor();
            this.Initialise();
        }

        protected void Initialise()
        {
            if (this.Genders is null)
            {
                this.Genders = new HashSet<IGender>(this.Load());
            }
        }

        public IEnumerable<IGender> Load()
        {
            HashSet<IGender> genders = new HashSet<IGender>();
                string[] files =
                    Directory.GetFiles(Directory.GetCurrentDirectory() + GlobalConstants.DATA_FOLDER + "/Genders",
                        "*.json", SearchOption.AllDirectories);

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

                                foreach (JToken child in jToken["Genders"])
                                {
                                    string name = (string) child["Name"];
                                    string possessive = (string) child["Possessive"];
                                    string personalSubject = (string) child["PersonalSubject"];
                                    string personalObject = (string) child["PersonalObject"];
                                    string reflexive = (string) child["Reflexive"];
                                    string possessivePlural = (string) child["PossessivePlural"];
                                    string reflexivePlural = (string) child["ReflexivePlural"];
                                    string isOrAre = (string) child["IsOrAre"];

                                    genders.Add(new BaseGender(
                                        name,
                                        possessive,
                                        personalSubject,
                                        personalObject,
                                        reflexive,
                                        possessivePlural,
                                        reflexivePlural,
                                        isOrAre));
                                }
                            }
                            catch (Exception e)
                            {
                                GlobalConstants.ActionLog.AddText("Error loading genders from " + file);
                                GlobalConstants.ActionLog.StackTrace(e);
                            }
                            finally
                            {
                                jsonReader.Close();
                                reader.Close();
                            }
                        }
                    }*/
                }

                return genders;
        }

        public IGender Get(string name)
        {
            if (this.Genders is null)
            {
                this.Initialise();
            }

            return this.Genders.First(gender => gender.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public bool Add(IGender value)
        {
            return !this.Genders.Contains(value) && this.Genders.Add(value);
        }
        
        public bool Destroy(string key)
        {
            var toRemove =
                this.Genders.FirstOrDefault(gender => gender.Name.Equals(key, StringComparison.OrdinalIgnoreCase));
            return !(toRemove is null) && this.Genders.Remove(toRemove);
        }

        public void Dispose()
        {
            this.Genders = null;
        }
    }
}