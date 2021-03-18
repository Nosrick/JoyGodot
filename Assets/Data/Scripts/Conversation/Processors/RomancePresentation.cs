using JoyLib.Code.Conversation.Conversations;
using JoyLib.Code.Entities.Relationships;
using JoyLib.Code.Rollers;

namespace JoyLib.Code.Entities.Conversation.Processors
{
    public class RomancePresentation : TopicData
    {
        protected IRelationship SelectedRelationship { get; set; }
        
        public RomancePresentation(IRelationship relationship)
        : base(
            new ITopicCondition[0], 
            "RomancePresentation",
            new []
            {
                "RomanceYes",
                "RomanceNo"
            },
            "Is a <1> relationship okay?",
            0,
            null,
            Speaker.LISTENER,
            new RNG())
        {
            this.SelectedRelationship = relationship;
            this.Words = this.Words.Replace("<1>", this.SelectedRelationship.Name);
        }

        protected override ITopic[] FetchNextTopics()
        {
            return new ITopic[]
            {
                new RomanceYes(this.SelectedRelationship),
                new RomanceNo()
            };
        }
    }
}