using System.Collections.Generic;
using JoyLib.Code.Conversation.Conversations;
using JoyLib.Code.Entities.Relationships;
using JoyLib.Code.Rollers;

namespace JoyLib.Code.Entities.Conversation.Processors
{
    public class BondingProposalProcessor : TopicData
    {
        public BondingProposalProcessor()
            : base(
                new ITopicCondition[0],
                        "BondingProcessor",
                new []
                {
                    "BondingDecision"
                },
                "words",
                0,
                null,
                Speaker.INSTIGATOR,
                new RNG())
        {}

        protected override ITopic[] FetchNextTopics()
        {
            IEntity listener = ConversationEngine.Listener;
            IEntity instigator = ConversationEngine.Instigator;
            IEnumerable<IRelationship> relationships =
                RelationshipHandler.Get(new IJoyObject[] {instigator, listener}, new[] {"romantic"});
            int highestValue = int.MinValue;
            IRelationship chosenRelationship = null;

            foreach (IRelationship relationship in relationships)
            {
                int value = relationship.GetRelationshipValue(instigator.Guid, listener.Guid);
                if (value > highestValue)
                {
                    highestValue = value;
                    chosenRelationship = relationship;
                }
            }

            string decision = "";
            if (highestValue > listener.Romance.BondingThreshold && chosenRelationship is null == false)
            {
                decision = "Yes, I will!";
                chosenRelationship.AddTag("bonded");
            }
            else
            {
                decision = "I'm sorry, no.";
            }

            return new ITopic[]
            {
                new BondingDecision(decision)
            };
        }
    }
}