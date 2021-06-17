using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using Godot.Collections;
using JoyGodot.Assets.Scripts.Helpers;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace JoyGodot.Assets.Scripts.Entities.Gender
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
                    Directory.GetFiles(
                        Directory.GetCurrentDirectory() +
                        GlobalConstants.ASSETS_FOLDER + 
                        GlobalConstants.DATA_FOLDER + 
                        "/Genders",
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
                        GlobalConstants.ActionLog.Log("Could not parse JSON in " + file + " into a Dictionary.", LogLevel.Warning);
                        continue;
                    }

                    ICollection<Dictionary> gendersCollection =
                        this.ValueExtractor.GetArrayValuesCollectionFromDictionary<Dictionary>(dictionary, "Genders");

                    foreach (Dictionary gender in gendersCollection)
                    {
                        string name = this.ValueExtractor.GetValueFromDictionary<string>(gender, "Name");
                        string possessive = this.ValueExtractor.GetValueFromDictionary<string>(gender, "Possessive");
                        string personalSubject = this.ValueExtractor.GetValueFromDictionary<string>(gender, "PersonalSubject");
                        string personalObject = this.ValueExtractor.GetValueFromDictionary<string>(gender, "PersonalObject");
                        string reflexive = this.ValueExtractor.GetValueFromDictionary<string>(gender, "Reflexive");
                        string possessivePlural = this.ValueExtractor.GetValueFromDictionary<string>(gender, "PossessivePlural");
                        string reflexivePlural = this.ValueExtractor.GetValueFromDictionary<string>(gender, "ReflexivePlural");
                        string isOrAre = this.ValueExtractor.GetValueFromDictionary<string>(gender, "IsOrAre");

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