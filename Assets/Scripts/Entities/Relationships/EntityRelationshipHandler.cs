using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Castle.Core.Internal;
using Godot;
using Godot.Collections;
using JoyLib.Code.Collections;
using JoyLib.Code.Helpers;
using JoyLib.Code.Scripting;
using Array = Godot.Collections.Array;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace JoyLib.Code.Entities.Relationships
{
    public class EntityRelationshipHandler : IEntityRelationshipHandler
    {
        protected IDictionary<string, IRelationship> m_RelationshipTypes;
        protected NonUniqueDictionary<long, IRelationship> m_Relationships;

        public JSONValueExtractor ValueExtractor { get; protected set; }
        
        public IEnumerable<IRelationship> Values => this.m_Relationships.Values;

        public EntityRelationshipHandler()
        {
            this.ValueExtractor = new JSONValueExtractor();
            this.m_Relationships = new NonUniqueDictionary<long, IRelationship>();
            this.m_RelationshipTypes =
                this.Load().ToDictionary(relationship => relationship.Name, relationship => relationship);
        }

        public IRelationship Get(long name)
        {
            return this.m_Relationships[name].First();
        }

        public IEnumerable<IRelationship> Load()
        {
            List<IRelationship> relationships = new List<IRelationship>();
            
            string[] files =
                Directory.GetFiles(
                    Directory.GetCurrentDirectory() + 
                    GlobalConstants.ASSETS_FOLDER + 
                    GlobalConstants.DATA_FOLDER + 
                    "/Relationships",
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

                ICollection<Dictionary> relationshipCollection =
                    this.ValueExtractor.GetArrayValuesCollectionFromDictionary<Dictionary>(dictionary, "Relationships");

                foreach (Dictionary relationship in relationshipCollection)
                {
                    string name = this.ValueExtractor.GetValueFromDictionary<string>(relationship, "Name");
                    string displayName = this.ValueExtractor.GetValueFromDictionary<string>(relationship, "DisplayName");
                    IEnumerable<string> uniqueTags = relationship.Contains("UniqueTags")
                        ? this.ValueExtractor.GetArrayValuesCollectionFromDictionary<string>(relationship, "UniqueTags")
                        : new string[0];
                    int maxParticipants = relationship.Contains("MaxParticipants")
                        ? this.ValueExtractor.GetValueFromDictionary<int>(relationship, "MaxParticipants")
                        : -1;
                    IEnumerable<string> tags = relationship.Contains("Tags")
                        ? this.ValueExtractor.GetArrayValuesCollectionFromDictionary<string>(relationship, "Tags")
                        : new string[0];
                                
                    relationships.Add(
                        new BaseRelationship(
                            name,
                            displayName,
                            maxParticipants,
                            uniqueTags,
                            null,
                            null,
                            tags));
                }
            }

            relationships.AddRange(ScriptingEngine.Instance.FetchAndInitialiseChildren<IRelationship>());

            return relationships;
        }

        public bool Add(IRelationship relationship)
        {
            this.m_Relationships.Add(relationship.GenerateHashFromInstance(), relationship);
            return true;
        }

        public bool Destroy(long ID)
        {
            return this.m_Relationships.RemoveByKey(ID) > 0;
        }

        public IRelationship CreateRelationship(IEnumerable<IJoyObject> participants, IEnumerable<string> tags)
        {
            long hash = BaseRelationship.GenerateHash(participants.Select(o => o.Guid));
            if (this.m_Relationships.ContainsKey(hash))
            {
                List<IRelationship> relationships = this.m_Relationships[hash];
                int matching = 0;
                IRelationship returnRelationship = null;
                foreach (IRelationship relationship in relationships)
                {
                    int matches = relationship.Tags.Intersect(tags, StringComparer.OrdinalIgnoreCase).Count();
                    if (matches <= matching)
                    {
                        continue;
                    }
                    matching = matches;
                    returnRelationship = relationship;
                }

                if (returnRelationship is null == false)
                {
                    return returnRelationship;
                }
            }
            
            if(this.m_RelationshipTypes.Any(t => tags.Intersect(t.Value.Tags, StringComparer.OrdinalIgnoreCase).Any()))
            {
                int matching = 0;
                IRelationship found = null;
                foreach (IRelationship relationship in this.m_RelationshipTypes.Values)
                {
                    int matches = relationship.Tags.Intersect(tags, StringComparer.OrdinalIgnoreCase).Count();
                    if (matches <= matching)
                    {
                        continue;
                    }
                    matching = matches;
                    found = relationship;
                }

                if (found is null)
                {
                    return null;
                }
                
                IRelationship newRelationship = found.Create(participants);

                //newRelationship.ModifyValueOfAllParticipants(0);

                this.m_Relationships.Add(newRelationship.GenerateHashFromInstance(), newRelationship);
                return newRelationship;
            }

            throw new InvalidOperationException("Relationship type " + tags.Print() + " not found.");
        }

        public IRelationship CreateRelationshipWithValue(IEnumerable<IJoyObject> participants, IEnumerable<string> tags,
            int value)
        {
            IRelationship relationship = this.CreateRelationship(participants, tags);
            relationship.ModifyValueOfAllParticipants(value);

            return relationship;
        }

        public IEnumerable<IRelationship> Get(IEnumerable<IJoyObject> participants, IEnumerable<string> tags = null,
            bool createNewIfNone = false)
        {
            IEnumerable<Guid> GUIDs = participants.Select(p => p.Guid);
            long hash = BaseRelationship.GenerateHash(GUIDs);

            List<IRelationship> relationships = new List<IRelationship>();
            
            foreach(Tuple<long, IRelationship> pair in this.m_Relationships)
            {
                if(pair.Item1 != hash)
                {
                    continue;
                }
                
                if (tags.IsNullOrEmpty() == false && tags.Intersect(pair.Item2.Tags).Any())
                {
                    relationships.Add(pair.Item2);
                }
                else if (tags.IsNullOrEmpty())
                {
                    relationships.Add(pair.Item2);
                }
            }

            if (relationships.Count == 0 && createNewIfNone)
            {
                List<string> newTags = new List<string>(tags);
                if (newTags.IsNullOrEmpty())
                {
                    newTags.Add("friendship");
                }
                relationships.Add(this.CreateRelationship(participants, newTags));
            }

            return relationships.ToArray();
        }

        public int GetHighestRelationshipValue(IJoyObject speaker, IJoyObject listener, IEnumerable<string> tags = null)
        {
            try
            {
                return this.GetBestRelationship(speaker, listener, tags)
                    .GetRelationshipValue(speaker.Guid, listener.Guid);
            }
            catch (Exception e)
            {
                GlobalConstants.ActionLog.Log("No relationship found for " + speaker + " and " + listener + ".", LogLevel.Warning);
                return 0;
            }
        }

        public IRelationship GetBestRelationship(IJoyObject speaker, IJoyObject listener,
            IEnumerable<string> tags = null)
        {
            IJoyObject[] participants = {speaker, listener};
            IEnumerable<IRelationship> relationships = this.Get(participants, tags, false);

            int highestValue = int.MinValue;
            IRelationship bestMatch = null;
            foreach (IRelationship relationship in relationships)
            {
                int value = relationship.GetRelationshipValue(speaker.Guid, listener.Guid);
                if (value > highestValue)
                {
                    highestValue = value;
                    bestMatch = relationship;
                }
            }

            if (bestMatch is null)
            {
                throw new InvalidOperationException("No relationship between " + speaker.JoyName + " and " + listener.JoyName + ".");
            }

            return bestMatch;
        }

        public IEnumerable<IRelationship> GetAllForObject(IJoyObject actor)
        {
            return this.m_Relationships.Where(tuple => tuple.Item2.GetParticipant(actor.Guid) is null == false)
                .Select(tuple => tuple.Item2)
                .ToArray();
        }

        public bool IsFamily(IJoyObject speaker, IJoyObject listener)
        {
            IJoyObject[] participants = { speaker, listener };
            IEnumerable<IRelationship> relationships = this.Get(participants, new[] {"family"});

            return relationships.Any();
        }

        public List<IRelationship> RelationshipTypes => this.m_RelationshipTypes.Values.ToList();
        public void Dispose()
        {
            this.m_RelationshipTypes = null;
            this.m_Relationships = null;
        }

        ~EntityRelationshipHandler()
        {
            this.Dispose();
        }

        public Dictionary Save()
        {
            Dictionary saveDict = new Dictionary();

            Dictionary valueDict = new Dictionary();
            foreach (long key in this.m_Relationships.Keys)
            {
                Array relationships = new Array(this.m_Relationships[key].Select(r => r.Save()));
                valueDict.Add(key, relationships);
            }
            
            saveDict.Add("Relationships", valueDict);

            return saveDict;
        }

        public void Load(Dictionary data)
        {
            Dictionary tempDict = this.ValueExtractor.GetValueFromDictionary<Dictionary>(data, "Relationships");
            foreach (DictionaryEntry entry in tempDict)
            {
                ICollection<Dictionary> relationshipDicts = this.ValueExtractor.GetCollectionFromArray<Dictionary>(entry.Value as Array);
                foreach (Dictionary relationshipDict in relationshipDicts)
                {
                    var name = this.ValueExtractor.GetValueFromDictionary<string>(relationshipDict, "Name");
                    IRelationship relationship =
                        this.RelationshipTypes
                            .FirstOrDefault(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                    relationship?.Load(relationshipDict);

                    if (relationship is null)
                    {
                        continue;
                    }
                
                    this.m_Relationships.Add(long.Parse(entry.Key.ToString()), relationship);
                }
            }
        }
    }
}
