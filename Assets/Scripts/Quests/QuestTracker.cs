using System;
using System.Collections.Generic;
using System.Linq;
using Godot.Collections;
using JoyLib.Code.Entities;
using JoyLib.Code.Entities.Items;
using JoyLib.Code.Scripting;
using Array = Godot.Collections.Array;

namespace JoyLib.Code.Quests
{
    public class QuestTracker : IQuestTracker
    {
        protected System.Collections.Generic.Dictionary<Guid, List<IQuest>> EntityQuests { get; set; }
        
        protected ILiveItemHandler ItemHandler { get; set; }

        public List<IQuest> AllQuests => this.EntityQuests.Values.SelectMany(list => list).ToList();

        public QuestTracker(ILiveItemHandler itemHandler = null)
        {
            this.ItemHandler = itemHandler ?? GlobalConstants.GameManager.ItemHandler;
            this.Initialise();
        }
        
        public void Initialise()
        {
            if (this.EntityQuests is null)
            {
                this.EntityQuests = new System.Collections.Generic.Dictionary<Guid, List<IQuest>>();
            }
        }

        public List<IQuest> GetQuestsForEntity(Guid GUID)
        {
            if (this.EntityQuests.ContainsKey(GUID))
            {
                return this.EntityQuests[GUID];
            }
                
            return new List<IQuest>();
        }

        public IQuest GetPrimaryQuestForEntity(Guid GUID)
        {
            if (this.EntityQuests.ContainsKey(GUID) && this.EntityQuests[GUID].Count > 0)
            {
                return this.EntityQuests[GUID][0];
            }
            
            throw new InvalidOperationException("No quests found for " + GUID + ".");
        }

        public void AddQuest(Guid GUID, IQuest quest)
        {
            if (this.EntityQuests.ContainsKey(GUID))
            {
                this.EntityQuests[GUID].Add(quest);
            }
            else
            {
                this.EntityQuests.Add(GUID, new List<IQuest> {quest});
            }

            GlobalConstants.ActionLog.Log("Quest added to player: " + quest.ID);
            GlobalConstants.ActionLog.Log(quest);
        }

        public void CompleteQuest(IEntity questor, IQuest quest)
        {
            GlobalConstants.ActionLog.Log(questor + " completed " + quest.ID);
            quest.CompleteQuest(questor);
            this.EntityQuests[questor.Guid].Remove(quest);
            GlobalConstants.GameManager.ItemHandler.CleanUpRewards();
        }

        public void AbandonQuest(IEntity questor, IQuest quest)
        {
            this.EntityQuests[questor.Guid].Remove(quest);
        }

        public void FailQuest(IEntity questor, IQuest quest)
        {
            this.EntityQuests[questor.Guid].Remove(quest);
        }

        public void PerformQuestAction(IEntity questor, IQuest quest, IJoyAction completedAction)
        {
            if (quest.FulfilsRequirements(questor, completedAction) && quest.AdvanceStep())
            {
                this.CompleteQuest(questor, quest);
            }
        }

        public void PerformQuestAction(IEntity questor, IJoyAction completedAction)
        {
            List<IQuest> copy = new List<IQuest>(this.GetQuestsForEntity(questor.Guid));
            foreach (IQuest quest in copy)
            {
                this.PerformQuestAction(questor, quest, completedAction);
            }
        }

        public Dictionary Save()
        {
            Dictionary saveDict = new Dictionary
            {
                {"Quests", new Array(this.AllQuests.Select(quest => quest.Save()))}
            };
            
            return saveDict;
        }

        public void Load(Dictionary data)
        {
            var valueExtractor = GlobalConstants.GameManager.ItemHandler.ValueExtractor;

            var questsCollection = valueExtractor.GetArrayValuesCollectionFromDictionary<Dictionary>(data, "Quests");

            foreach (Dictionary questDict in questsCollection)
            {
                IQuest quest = new Quest();
                quest.Load(questDict);
                if (this.EntityQuests.ContainsKey(quest.Questor))
                {
                    this.EntityQuests[quest.Questor].Add(quest);
                }
                else
                {
                    this.EntityQuests.Add(quest.Questor, new List<IQuest> { quest });
                }
            }
        }
    }
}
