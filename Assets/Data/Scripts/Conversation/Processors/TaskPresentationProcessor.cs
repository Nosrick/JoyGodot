using JoyGodot.Assets.Scripts.Conversation.Conversations;
using JoyGodot.Assets.Scripts.Quests;

namespace JoyGodot.Assets.Data.Scripts.Conversation.Processors
{
    public class TaskPresentationProcessor : TopicData
    {
        protected IQuest OfferedQuest { get; set; }
        
        public TaskPresentationProcessor(IQuest offeredQuest) 
            : base(
                new ITopicCondition[0], 
                "TaskPresentation",
                new []
                {
                    "TaskYes",
                    "TaskNo"
                }, 
                offeredQuest.ToString(), 
                0,
                null,
                Speaker.LISTENER)
        {
            this.OfferedQuest = offeredQuest;
        }

        protected override ITopic[] FetchNextTopics()
        {
            return new ITopic[]
            {
                new TaskYesProcessor(this.OfferedQuest),
                new TaskNoProcessor()
            };
        }
    }
}