using JoyLib.Code.Conversation.Conversations;
using JoyLib.Code.Conversation.Subengines.Rumours;
using JoyLib.Code.Rollers;

namespace JoyLib.Code.Entities.Conversation.Processors
{
    public class ChatterProcessor : TopicData
    {
        protected IRumourMill RumourMill
        {
            get;
            set;
        }
        
        public ChatterProcessor() 
            : base(
                new ITopicCondition[0], 
                "ChatterTopic", 
                new[] { "Thanks" }, 
                "", 
                0, 
                null, 
                Speaker.LISTENER,
                new RNG())
        {
            this.Initialise();
        }

        protected void Initialise()
        {
            if (this.RumourMill is null)
            {
                this.RumourMill = GlobalConstants.GameManager?.RumourMill;
            }
        }

        protected override ITopic[] FetchNextTopics()
        {
            this.Initialise();
            
            return new ITopic[]
            {
                new TopicData(
                    new ITopicCondition[0],
                    "ChatterTopic",
                    new string[] {"Thanks"},
                    this.RumourMill.GetRandom(ConversationEngine.Listener.MyWorld.GetOverworld()).Words,
                    0,
                    null,
                    Speaker.LISTENER,
                    new RNG(),
                    "",
                    this.ConversationEngine,
                    this.RelationshipHandler)
            };
        }
    }
}