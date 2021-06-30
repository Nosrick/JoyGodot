using System;
using JoyGodot.Assets.Scripts.Conversation.Conversations;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Relationships;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Rollers;

namespace JoyGodot.Assets.Data.Scripts.Conversation.Processors
{
    public class RomanceYes : TopicData
    {
        protected IRelationship SelectedRelationship { get; set; }
        
        public RomanceYes(IRelationship relationship)
        : base(
            new ITopicCondition[0],
            "RomanceYes",
            new[] { "BaseTopics" },
            "It certainly is.",
            0,
            null,
            Speaker.LISTENER,
            new RNG())
        {
            this.SelectedRelationship = relationship;
        }

        public override ITopic[] Interact(IEntity instigator, IEntity listener)
        {
            var newRelationship = this.SelectedRelationship.Create(new[] {listener.Guid, instigator.Guid});
            this.RelationshipHandler.Add(newRelationship);
            
            return base.Interact(instigator, listener);
        }
    }
}