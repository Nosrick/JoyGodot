﻿using JoyGodot.Assets.Scripts;
using JoyGodot.Assets.Scripts.Conversation.Conversations;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Quests;

namespace JoyGodot.Assets.Data.Scripts.Conversation.Processors
{
    public class TaskYesProcessor : TopicData
    {
        protected IQuestTracker QuestTracker { get; set; }
        protected IEntity Player { get; set; }
        
        protected IQuest OfferedQuest { get; set; }
        
        protected bool Initialised { get; set; }
        
        public TaskYesProcessor(IQuest offeredQuest) 
            : base(
                new ITopicCondition[0], 
                "TaskYes", 
                new []{ "ListenerThanks" }, 
                "I can do that.", 
                new []{"relationship", "task", "positive"}, 
                0, 
                null, 
                Speaker.INSTIGATOR)
        {
            this.Initialise();
            this.OfferedQuest = offeredQuest;
        }

        protected void Initialise()
        {
            if (!(this.QuestTracker is null) || this.Initialised)
            {
                return;
            }

            IGameManager gameManager = GlobalConstants.GameManager;
            this.QuestTracker = gameManager.QuestTracker;
            this.Player = gameManager.Player;

            this.Initialised = true;
        }

        public override ITopic[] Interact(IEntity instigator, IEntity listener)
        {
            this.Initialise();
            base.Interact(instigator, listener);
            this.QuestTracker.AddQuest(
                instigator.Guid,
                this.OfferedQuest);

            this.OfferedQuest.StartQuest(instigator);

            this.Words = this.OfferedQuest.ToString();

            return this.FetchNextTopics();
        }
    }
}