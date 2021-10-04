using JoyGodot.Assets.Scripts;
using JoyGodot.Assets.Scripts.Conversation.Conversations;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Quests;

namespace JoyGodot.Assets.Data.Scripts.Conversation.Processors
{
    public class TaskProcessor : TopicData
    {
        protected IQuestProvider QuestProvider { get; set; }
        
        protected IQuest OfferedQuest { get; set; }
        
        public TaskProcessor() 
            : base(
                new ITopicCondition[0], 
                "TaskTopic", 
                new []
                {
                    "TaskPresentation"
                }, 
                "", 
                new []{"relationship", "query", "task"}, 
                0, 
                null, 
                Speaker.LISTENER)
        {
            this.Initialise();
        }

        protected void Initialise()
        {
            if (!(this.QuestProvider is null))
            {
                return;
            }

            this.QuestProvider = GlobalConstants.GameManager?.QuestProvider;
        }

        public override ITopic[] Interact(IEntity instigator, IEntity listener)
        {
            this.OfferedQuest = this.QuestProvider?.MakeRandomQuest(
                instigator,
                listener,
                instigator.MyWorld.GetOverworld());

            return this.FetchNextTopics();
        }

        protected override ITopic[] FetchNextTopics()
        {
            return new ITopic[]
            {
                new TaskPresentationProcessor(this.OfferedQuest)
            };
        }
    }
}