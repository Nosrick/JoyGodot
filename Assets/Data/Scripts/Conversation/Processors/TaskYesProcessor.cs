using JoyLib.Code.Conversation.Conversations;
using JoyLib.Code.Quests;

namespace JoyLib.Code.Entities.Conversation.Processors
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
            this.Player = gameManager.EntityHandler.GetPlayer();

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