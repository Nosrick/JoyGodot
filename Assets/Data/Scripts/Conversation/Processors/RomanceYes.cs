using JoyLib.Code.Conversation.Conversations;
using JoyLib.Code.Entities.Relationships;
using JoyLib.Code.Rollers;

namespace JoyLib.Code.Entities.Conversation.Processors
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
            var newRelationship = this.SelectedRelationship.Create(new IJoyObject[] {listener, instigator});
            this.RelationshipHandler.Add(newRelationship);
            
            return base.Interact(instigator, listener);
        }
    }
}