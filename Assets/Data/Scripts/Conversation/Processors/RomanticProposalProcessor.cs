using System;
using System.Collections.Generic;
using System.Linq;
using JoyGodot.Assets.Scripts.Conversation.Conversations;
using JoyGodot.Assets.Scripts.Cultures;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Relationships;
using JoyGodot.Assets.Scripts.JoyObject;

namespace JoyGodot.Assets.Data.Scripts.Conversation.Processors
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
                new []{"relationship", "query", "romance"}, 
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
                .Get(new[] {instigator.Guid, listener.Guid})
                .ToList();
            if (listener.Romance.WillRomance(listener, instigator, relationships)
            && instigator.Romance.WillRomance(instigator, listener, relationships))
            {
                ICulture cultureResult = this.Roller.SelectFromCollection(listener.Cultures);
                string relationshipType = this.Roller.SelectFromCollection(cultureResult.RelationshipTypes);
    
                IRelationship selectedRelationship = this.RelationshipHandler.RelationshipTypes.First(relationship =>
                    relationship.Name.Equals(relationshipType, StringComparison.OrdinalIgnoreCase));

                relationships = this.RelationshipHandler.GetAllForObject(instigator.Guid).ToList();

                bool unique = relationships.Any(relationship =>
                    relationship.Name.Equals(selectedRelationship.Name)
                    && relationship.UniqueTags.Intersect(selectedRelationship.UniqueTags).Any());

                relationships = this.RelationshipHandler.GetAllForObject(listener.Guid).ToList();
                
                unique |= relationships.Any(relationship =>
                    relationship.Name.Equals(selectedRelationship.Name)
                    && relationship.UniqueTags.Intersect(selectedRelationship.UniqueTags).Any());

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
                    new []{"relationship", "negative", "romance"},
                    0,
                    null,
                    Speaker.LISTENER)
            };
        }
    }
}