using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Godot.Collections;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.JoyObject;
using Array = Godot.Collections.Array;

namespace JoyGodot.Assets.Scripts.Entities.Relationships
{
    [Serializable]
    public class BaseRelationship : IRelationship
    {
        protected HashSet<string> m_Tags;
        public string Name { get; protected set; }
        public string DisplayName { get; protected set; }
        public HashSet<string> UniqueTags { get; protected set; }
        public int MaxParticipants { get; protected set; }

        //Yeesh, this is messy
        //But this is a key value pair for how each participant feels about the other in the relationship
        protected IDictionary<Guid, IDictionary<Guid, int>> m_Values;

        protected List<Guid> m_Participants;

        public IEnumerable<string> Tags
        {
            get => this.m_Tags;
            protected set => this.m_Tags = new HashSet<string>(value);
        }

        public BaseRelationship()
        {
            this.Name = "DEFAULT";
            this.DisplayName = "DEFAULT";
            this.UniqueTags = new HashSet<string>();
            this.MaxParticipants = 1;
            this.m_Participants = new List<Guid>();
            this.m_Values = new System.Collections.Generic.Dictionary<Guid, IDictionary<Guid, int>>();
            this.m_Tags = new HashSet<string>();
        }

        public BaseRelationship(
            string name,
            string displayName,
            int maxParticipants,
            IEnumerable<string> uniqueTags = null,
            IEnumerable<Guid> participants = null,
            IDictionary<Guid, IDictionary<Guid, int>> values = null,
            IEnumerable<string> tags = null)
        {
            this.Name = name;
            this.DisplayName = displayName;
            this.UniqueTags = uniqueTags.IsNullOrEmpty() ? new HashSet<string>() : new HashSet<string>(uniqueTags);
            this.MaxParticipants = maxParticipants;
            this.m_Participants = new List<Guid>();
            this.m_Values = values ?? new SortedDictionary<Guid, IDictionary<Guid, int>>();
            this.Tags = tags ?? new HashSet<string>();
            if (participants.IsNullOrEmpty() == false)
            {
                this.AddParticipants(participants);
            }
        }

        public long GenerateHashFromInstance()
        {
            return GenerateHash(this.m_Participants);
        }


        public virtual bool AddParticipant(Guid newParticipant)
        {
            if (this.m_Participants.Contains(newParticipant) == false
                && (this.MaxParticipants < 0
                    || this.m_Participants.Count + 1 <= this.MaxParticipants))
            {
                this.m_Participants.Add(newParticipant);

                this.m_Values.Add(newParticipant, new System.Collections.Generic.Dictionary<Guid, int>());

                foreach (KeyValuePair<Guid, IDictionary<Guid, int>> pair in this.m_Values)
                {
                    if (pair.Key == newParticipant)
                    {
                        foreach (Guid guid in this.m_Participants)
                        {
                            this.m_Values[newParticipant].Add(guid, 0);
                        }
                    }
                    else
                    {
                        this.m_Values[pair.Key].Add(newParticipant, 0);
                    }
                }

                return true;
            }

            return false;
        }

        public bool AddParticipants(IEnumerable<Guid> participants)
        {
            return participants.Aggregate(true, (current, participant) => current & this.AddParticipant(participant));
        }

        public bool HasTag(string tag)
        {
            return this.m_Tags.Any(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase));
        }

        public bool AddTag(string tag)
        {
            if (this.HasTag(tag))
            {
                return false;
            }

            this.m_Tags.Add(tag);
            return true;
        }

        public bool RemoveTag(string tag)
        {
            if (this.HasTag(tag))
            {
                this.m_Tags.Remove(tag);
                return true;
            }

            return false;
        }

        public IJoyObject GetParticipant(Guid GUID)
        {
            /*
            return this.m_Participants.Contains(GUID) 
                ? GlobalConstants.GameManager.EntityHandler.Get(GUID) 
                : null;
                */
            return null;
        }

        public IEnumerable<IJoyObject> GetParticipants()
        {
            List<IEntity> participants = new List<IEntity>();
            foreach (Guid participant in this.m_Participants)
            {
                //participants.Add(GlobalConstants.GameManager.EntityHandler.Get(participant));
            }

            return participants;
        }

        public int ModifyValueOfParticipant(Guid actor, Guid observer, int value)
        {
            if (this.m_Values.ContainsKey(observer))
            {
                if (this.m_Values[observer].ContainsKey(actor))
                {
                    this.m_Values[observer][actor] += value;
                    return this.m_Values[observer][actor];
                }
            }

            return 0;
        }

        public int GetHighestRelationshipValue(Guid GUID)
        {
            return this.m_Values.Where(pair => pair.Key.Equals(GUID))
                .Max(pair => pair.Value.Max(valuePair => valuePair.Value));
        }

        public IDictionary<Guid, int> GetValuesOfParticipant(Guid GUID)
        {
            if (this.m_Values.ContainsKey(GUID))
            {
                return this.m_Values[GUID];
            }

            return null;
        }

        public int ModifyValueOfOtherParticipants(Guid actor, int value)
        {
            List<Guid> participantKeys = this.m_Values.Keys.ToList();
            foreach (Guid guid in participantKeys)
            {
                if (guid != actor)
                {
                    this.m_Values[guid][actor] += value;
                }
            }

            return value;
        }

        public int ModifyValueOfAllParticipants(int value)
        {
            List<Guid> participantKeys = this.m_Values.Keys.ToList();
            foreach (Guid guid in participantKeys)
            {
                if (this.m_Values[guid].Keys.Count == 0)
                {
                    foreach (Guid participant in this.m_Participants)
                    {
                        if (guid != participant)
                        {
                            this.m_Values[guid].Add(participant, 0);
                        }
                    }
                }

                List<Guid> involvedKeys = this.m_Values[guid].Keys.ToList();

                foreach (Guid involvedGUID in involvedKeys)
                {
                    this.m_Values[guid][involvedGUID] += value;
                }
            }

            return value;
        }

        public bool RemoveParticipant(Guid currentGUID)
        {
            if (this.m_Participants.Contains(currentGUID))
            {
                this.m_Participants.Remove(currentGUID);
                this.m_Values.Remove(currentGUID);
                foreach (System.Collections.Generic.Dictionary<Guid, int> relationship in this.m_Values.Values)
                {
                    if (relationship.ContainsKey(currentGUID))
                    {
                        relationship.Remove(currentGUID);
                    }
                }

                return true;
            }

            return false;
        }

        public static long GenerateHash(IEnumerable<Guid> participants)
        {
            long hash = 0;

            long s1 = 1;
            long s2 = 0;

            int hashMagic = 65521;

            List<Guid> sortedList = new List<Guid>(participants);
            sortedList.Sort();
            foreach (Guid GUID in sortedList)
            {
                s1 = (s1 + GUID.GetHashCode()) % hashMagic;
                s2 = (s2 + s1) % hashMagic;
            }

            hash = (s2 << 16) | s1;
            return hash;
        }

        public int GetRelationshipValue(Guid left, Guid right)
        {
            if (this.m_Values.ContainsKey(left))
            {
                if (this.m_Values[left].ContainsKey(right))
                {
                    return this.m_Values[left][right];
                }
            }

            return 0;
        }

        public IRelationship Create(
            IEnumerable<Guid> participants)
        {
            return new BaseRelationship(
                this.Name,
                this.DisplayName,
                this.MaxParticipants,
                this.UniqueTags,
                participants,
                null,
                this.Tags);
        }

        public IRelationship CreateWithValue(IEnumerable<Guid> participants,
            int value)
        {
            IRelationship relationship = this.Create(participants);

            relationship.ModifyValueOfAllParticipants(value);

            return relationship;
        }

        public Dictionary Save()
        {
            Dictionary saveDict = new Dictionary
            {
                {"Name", this.Name},
                {"DisplayName", this.DisplayName},
                {"Tags", new Array(this.Tags)},
                {"UniqueTags", new Array(this.UniqueTags)},
                {"MaxParticipants", this.MaxParticipants},
                {"Participants", new Array(this.m_Participants.Select(guid => guid.ToString()))}
            };

            Dictionary values = new Dictionary();

            foreach (KeyValuePair<Guid, IDictionary<Guid, int>> dictPair in this.m_Values)
            {
                Dictionary innerValues = new Dictionary();
                foreach (KeyValuePair<Guid, int> pair in dictPair.Value)
                {
                    innerValues.Add(pair.Key.ToString(), pair.Value);
                }

                values.Add(dictPair.Key.ToString(), innerValues);
            }

            saveDict.Add("Values", values);

            return saveDict;
        }

        public void Load(Dictionary data)
        {
            var valueExtractor = GlobalConstants.GameManager.RelationshipHandler.ValueExtractor;
            this.Name = valueExtractor.GetValueFromDictionary<string>(data, "Name");
            this.DisplayName = valueExtractor.GetValueFromDictionary<string>(data, "DisplayName");
            this.Tags = valueExtractor.GetArrayValuesCollectionFromDictionary<string>(data, "Tags");
            this.UniqueTags =
                new HashSet<string>(
                    valueExtractor.GetArrayValuesCollectionFromDictionary<string>(
                        data,
                        "UniqueTags"));
            this.MaxParticipants = valueExtractor.GetValueFromDictionary<int>(data, "MaxParticipants");
            this.m_Participants = valueExtractor
                .GetArrayValuesCollectionFromDictionary<string>(
                    data,
                    "Participants")
                .Select(s => new Guid(s))
                .ToList();

            Dictionary outerValues = valueExtractor.GetValueFromDictionary<Dictionary>(data, "Values");
            foreach (DictionaryEntry outerValue in outerValues)
            {
                IDictionary<Guid, int> values = new System.Collections.Generic.Dictionary<Guid, int>();
                Guid key = new Guid(outerValue.Key.ToString());
                foreach (DictionaryEntry innerValue in outerValue.Value as Dictionary)
                {
                    Guid participant = new Guid(innerValue.Key.ToString());
                    int value = int.Parse(innerValue.Value.ToString());
                    values.Add(participant, value);
                }

                this.m_Values.Add(key, values);
            }
        }
    }
}