using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Conversation.Conversations;
using JoyGodot.Assets.Scripts.Rollers;

namespace JoyGodot.Assets.Data.Scripts.Conversation.Processors
{
    public class BondingDecision : TopicData
    {
        public BondingDecision(
            string decision,
            IEnumerable<string> tags)
            : base(
                new ITopicCondition[0], 
                "BondingDecision",
                new []{ "BaseTopics" },
                decision, 
                tags,
                0,
                null, 
                Speaker.LISTENER,
                new RNG())
        {}
    }
}