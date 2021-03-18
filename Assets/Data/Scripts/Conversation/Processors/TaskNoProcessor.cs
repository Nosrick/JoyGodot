using JoyLib.Code.Conversation.Conversations;

namespace JoyLib.Code.Entities.Conversation.Processors
{
    public class TaskNoProcessor : TopicData
    {
        public TaskNoProcessor() 
            : base(
                new ITopicCondition[0], 
                "TaskNo",
                new []{ "BaseTopics" }, 
                "I can't do that for you.", 
                0,
                null, 
                Speaker.INSTIGATOR)
        {
        }
    }
}