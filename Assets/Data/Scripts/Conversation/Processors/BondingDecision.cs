using JoyGodot.Assets.Scripts.Conversation.Conversations;
using JoyGodot.Assets.Scripts.Rollers;

namespace JoyGodot.Assets.Data.Scripts.Conversation.Processors
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