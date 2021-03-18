using JoyLib.Code.Conversation.Conversations;
using JoyLib.Code.Rollers;

namespace JoyLib.Code.Entities.Conversation.Processors
{
    public class BondingDecision : TopicData
    {
        public BondingDecision(string decision)
            : base(
                new ITopicCondition[0], 
                "BondingDecision",
                new []{ "BaseTopics" },
                decision,
                0,
                null, 
                Speaker.LISTENER,
                new RNG())
        {}
    }
}