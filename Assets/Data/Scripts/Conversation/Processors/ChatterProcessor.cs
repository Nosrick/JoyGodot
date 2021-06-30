using JoyGodot.Assets.Scripts;
using JoyGodot.Assets.Scripts.Conversation.Conversations;
using JoyGodot.Assets.Scripts.Conversation.Subengines.Rumours;
using JoyGodot.Assets.Scripts.Rollers;

namespace JoyGodot.Assets.Data.Scripts.Conversation.Processors
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
                    this.RumourMill.GetRandom(this.ConversationEngine.Listener.MyWorld.GetOverworld()).Words,
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