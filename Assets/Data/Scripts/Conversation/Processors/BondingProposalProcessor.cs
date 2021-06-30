using System;
using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Conversation.Conversations;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Relationships;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Rollers;

namespace JoyGodot.Assets.Data.Scripts.Conversation.Processors
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
            IEntity listener = this.ConversationEngine.Listener;
            IEntity instigator = this.ConversationEngine.Instigator;
            IEnumerable<IRelationship> relationships =
                this.RelationshipHandler.Get(
                    new[] {instigator.Guid, listener.Guid}, 
                    new[] {"romantic"});
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