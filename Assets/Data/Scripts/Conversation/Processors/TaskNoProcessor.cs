using JoyGodot.Assets.Scripts.Conversation.Conversations;

namespace JoyGodot.Assets.Data.Scripts.Conversation.Processors
{
    public class TaskNoProcessor : TopicData
    {
        public TaskNoProcessor() 
            : base(
                new ITopicCondition[0], 
                "TaskNo",
                new []{ "BaseTopics" }, 
                "I can't do that for you.", 
                new []{"relationship", "negative"}, 
                0,
                null, 
                Speaker.INSTIGATOR)
        {
        }
    }
}