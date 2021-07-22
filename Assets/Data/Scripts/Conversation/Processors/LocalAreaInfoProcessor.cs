using JoyGodot.Assets.Scripts;
using JoyGodot.Assets.Scripts.Conversation.Conversations;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.Rollers;
using JoyGodot.Assets.Scripts.World;
using JoyGodot.Assets.Scripts.World.WorldInfo;

namespace JoyGodot.Assets.Data.Scripts.Conversation.Processors
{
    public class LocalAreaInfoProcessor : TopicData
    {
        protected IEntity Listener
        {
            get;
            set;
        }
        
        protected static ILocalAreaInfoHandler InfoHandler { get; set; }
        
        public LocalAreaInfoProcessor() 
            : base(
                new ITopicCondition[0], 
                "BaseTopics", 
                new [] { "Thanks" }, 
                "Can you tell me anything about this place?", 
                0, 
                null,
                Speaker.INSTIGATOR)
        {
            InfoHandler ??= GlobalConstants.GameManager.LocalAreaInfoHandler;
        }

        public override ITopic[] Interact(IEntity instigator, IEntity listener)
        {
            this.Listener = listener;
            return base.Interact(instigator, listener);
        }

        protected override ITopic[] FetchNextTopics()
        {
            return new ITopic[]
            {
                new TopicData(
                    new ITopicCondition[0], 
                    "LocalAreaInfo",
                    new []{ "Thanks" }, 
                    this.GetAreaInfo(this.Listener),
                    0,
                    null, 
                    Speaker.LISTENER,
                    this.Roller) 
            };
        }

        protected string GetAreaInfo(IEntity listener)
        {
            return InfoHandler.GetRandomLocalAreaInfo(listener.MyWorld);
        }
    }
}