﻿using System;
using System.Collections.Generic;
using System.Linq;
using JoyLib.Code.Conversation.Conversations;
using JoyLib.Code.Cultures;
using JoyLib.Code.Entities.Relationships;

namespace JoyLib.Code.Entities.Conversation.Processors
{
    public class RomanticProposalProcessor : TopicData
    {
        public RomanticProposalProcessor() 
            : base(
                new ITopicCondition[0], 
                "RomanticProposal", 
                new []
                {
                    "RomancePresentation"
                }, 
                "words", 
                0, 
                null,
                Speaker.INSTIGATOR)
        {
        }

        protected override ITopic[] FetchNextTopics()
        {
            IEntity listener = this.ConversationEngine.Listener;
            IEntity instigator = this.ConversationEngine.Instigator;
            List<IRelationship> relationships = this.RelationshipHandler
                .Get(new IJoyObject[] {instigator, listener})
                .ToList();
            if (listener.Romance.WillRomance(listener, instigator, relationships)
            && instigator.Romance.WillRomance(instigator, listener, relationships))
            {
                ICulture cultureResult = this.Roller.SelectFromCollection(listener.Cultures);
                string relationshipType = this.Roller.SelectFromCollection(cultureResult.RelationshipTypes);
    
                IRelationship selectedRelationship = this.RelationshipHandler.RelationshipTypes.First(relationship =>
                    relationship.Name.Equals(relationshipType, StringComparison.OrdinalIgnoreCase));

                relationships = this.RelationshipHandler.GetAllForObject(instigator).ToList();

                bool unique = relationships.Any(relationship =>
                    relationship.Name.Equals(selectedRelationship.Name)
                    && (relationship.Unique || selectedRelationship.Unique));

                relationships = this.RelationshipHandler.GetAllForObject(listener).ToList();
                
                unique |= relationships.Any(relationship =>
                    relationship.Name.Equals(selectedRelationship.Name)
                    && (relationship.Unique || selectedRelationship.Unique));

                if (unique == false)
                {
                    return new ITopic[]
                    {
                        new RomancePresentation(selectedRelationship)
                    };
                }
            }
            return new ITopic[]
            {
                new TopicData(
                    new ITopicCondition[0],
                    "RomanceTurnDown",
                    new[] {"BaseTopics"},
                    "Uh, no thanks.",
                    0,
                    null,
                    Speaker.LISTENER)
            };
        }
    }
}