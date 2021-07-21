using System;
using System.Collections.Generic;
using System.Linq;

using Godot.Collections;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.Items;
using JoyGodot.Assets.Scripts.Quests.Actions;
using JoyGodot.Assets.Scripts.Scripting;
using JoyGodot.Assets.Scripts.World;
using Array = Godot.Collections.Array;

namespace JoyGodot.Assets.Scripts.Quests
{
    [Serializable]
    public class Quest : IQuest
    {
        protected List<string> m_Tags;

        public Quest()
        {
            this.Actions = new List<IQuestAction>();
            this.Morality = QuestMorality.Neutral;
            this.RewardGUIDs = new List<Guid>();
            this.Instigator = Guid.Empty;
            this.Questor = Guid.Empty;
            this.CurrentStep = 0;
            this.ID = Guid.Empty;
            this.m_Tags = new List<string>();
        }
        
        public Quest(
            List<IQuestAction> steps,
            QuestMorality morality,
            IEnumerable<IItemInstance> rewards,
            Guid instigator,
            Guid questor,
            IEnumerable<string> tags)
        {
            this.Actions = steps;
            this.Morality = morality;
            this.RewardGUIDs = rewards.Select(instance => instance.Guid).ToList();
            this.Instigator = instigator;
            this.Questor = questor;
            this.CurrentStep = 0;
            this.ID = GlobalConstants.GameManager.GUIDManager.AssignGUID();
            GlobalConstants.ActionLog.Log("Rewards for quest " + this.ID);
            GlobalConstants.ActionLog.Log(rewards);
            GlobalConstants.GameManager.ItemHandler.AddQuestRewards(this.ID, rewards);
            this.Tags = new List<string>(tags);
        }

        ~Quest()
        {
            GlobalConstants.GameManager.GUIDManager.ReleaseGUID(this.ID);
            GlobalConstants.GameManager.ItemHandler.CleanUpRewards();
        }

        public bool AdvanceStep()
        {
            this.CurrentStep++;

            return this.IsComplete;
        }

        public bool FulfilsRequirements(IEntity questor, IJoyAction action)
        {
            return this.Actions[this.CurrentStep].ExecutedSuccessfully(action);
        }

        public void StartQuest(IEntity questor)
        {
            foreach (IQuestAction step in this.Actions)
            {
                step.ExecutePrerequisites(questor);
            }
        }

        public bool BelongsToThis(object searchTerm)
        {
            switch (searchTerm)
            {
                case IItemInstance itemInstance:
                {
                    return this.Actions[this.CurrentStep].Items.Contains(itemInstance.Guid);
                }
                case IEntity entity:
                {
                    return this.Actions[this.CurrentStep].Actors.Contains(entity.Guid);
                }
                case IWorldInstance worldInstance:
                {
                    return this.Actions[this.CurrentStep].Areas.Contains(worldInstance.Guid);
                }
                default:
                    return false;
            }
        }

        public bool CompleteQuest(IEntity questor, bool force = false)
        {
            if (force)
            {
                this.CurrentStep = this.Actions.Count;
            }
            
            if (this.IsComplete == false)
            {
                return false;
            }

            if (this.Rewards.Any() == false)
            {
                GlobalConstants.ActionLog.Log("Quest " + this.ID + " has no rewards!");
            }

            foreach (IItemInstance reward in this.Rewards)
            {
                if (reward is ItemInstance item)
                {
                    item.Instantiate();
                }
                GlobalConstants.ActionLog.Log("Adding " + reward + " to " + questor + " inventory as reward");
                questor.AddContents(reward);
            }

            return true;
        }
        
        public bool AddTag(string tag)
        {
            if (this.m_Tags.Any(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase)) != false)
            {
                return false;
            }
            
            this.m_Tags.Add(tag);
            return true;
        }

        public bool RemoveTag(string tag)
        {
            if (!this.Tags.Any(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
            
            this.m_Tags.Remove(tag);
            return true;
        }

        public bool HasTag(string tag)
        {
            return this.Tags.Any(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase));
        }

        public bool HasTags(IEnumerable<string> tags)
        {
            return tags.All(this.HasTag);
        }

        public override string ToString()
        {
            string fullString = "";
            string rewardString = this.Rewards.Count > 0 ? "I'll give you " : "";
            for (int i = 0; i < this.Rewards.Count; i++)
            {
                rewardString += this.Rewards[i].JoyName;
                if(this.Rewards[i].Contents.Any())
                {
                    rewardString += ", " + this.Rewards[i].ContentString;
                }
                if (this.Rewards.Count > 1)
                {
                    if (i == this.Rewards.Count - 2)
                        rewardString += "and ";
                    else if (i < this.Rewards.Count - 2)
                        rewardString += ", ";
                }
            }

            for (int j = 0; j < this.Actions.Count; j++)
            {
                fullString += this.Actions[j].AssembleDescription();
            }
            fullString += " " + rewardString + ".";
            return fullString;
        }

        public Dictionary Save()
        {
            Dictionary saveDict = new Dictionary
            {
                {"Guid", this.ID.ToString()},
                {"Questor", this.Questor.ToString()},
                {"Instigator", this.Instigator.ToString()},
                {"CurrentStep", this.CurrentStep},
                {"Rewards", new Array(this.Rewards.Select(instance => instance.Save()))},
                {"Actions", new Array(this.Actions.Select(step => step.Save()))}
            };

            return saveDict;
        }

        public void Load(Dictionary data)
        {
            var valueExtractor = GlobalConstants.GameManager.ItemDatabase.ValueExtractor;
            string guidString = valueExtractor.GetValueFromDictionary<string>(data, "Guid");
            this.ID = guidString is null ? Guid.Empty : new Guid(guidString);

            guidString = valueExtractor.GetValueFromDictionary<string>(data, "Questor");
            this.Questor = guidString is null ? Guid.Empty : new Guid(guidString);

            guidString = valueExtractor.GetValueFromDictionary<string>(data, "Instigator");
            this.Instigator = guidString is null ? Guid.Empty : new Guid(guidString);

            this.CurrentStep = valueExtractor.GetValueFromDictionary<int>(data, "CurrentStep");

            var rewardDicts = valueExtractor.GetArrayValuesCollectionFromDictionary<Dictionary>(data, "Rewards");

            List<IItemInstance> rewards = new List<IItemInstance>();
            foreach (Dictionary dict in rewardDicts)
            {
                IItemInstance item = new ItemInstance();
                item.Load(dict);
                rewards.Add(item);
            }
            GlobalConstants.GameManager.ItemHandler.AddQuestRewards(this.ID, rewards);

            this.RewardGUIDs = rewards.Select(instance => instance.Guid).ToList();

            this.Actions = new List<IQuestAction>();
            var actionDicts = valueExtractor.GetArrayValuesCollectionFromDictionary<Dictionary>(data, "Actions");
            foreach (Dictionary actionDict in actionDicts)
            {
                string type = valueExtractor.GetValueFromDictionary<string>(actionDict, "Type");
                if (type.IsNullOrEmpty())
                {
                    continue;
                }
                
                IQuestAction action = QuestActionFactory.Create(type);
                if (action is null)
                {
                    continue;
                }
                
                action.Load(actionDict);
                this.Actions.Add(action);
            }
        }

        public List<IQuestAction> Actions { get; protected set; }
        
        public QuestMorality Morality { get; protected set; }
        
        public List<Guid> RewardGUIDs { get; protected set; }

        public List<IItemInstance> Rewards =>
            GlobalConstants.GameManager.ItemHandler.GetQuestRewards(this.ID).ToList();
        
        public int CurrentStep { get; protected set;  }

        public Guid Instigator { get; protected set; }

        public Guid Questor { get; protected set; }

        public Guid ID { get; protected set; }

        public bool IsComplete => this.CurrentStep == this.Actions.Count;

        public IEnumerable<string> Tags
        {
            get => this.m_Tags;
            protected set => this.m_Tags = new List<string>(value);
        }
    }
}
