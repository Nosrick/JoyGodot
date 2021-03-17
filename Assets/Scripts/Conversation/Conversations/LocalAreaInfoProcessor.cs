using System;
using System.Linq;
using JoyLib.Code.Entities;

namespace JoyLib.Code.Conversation.Conversations
{
    public class LocalAreaInfoProcessor : TopicData
    {
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
        }

        public override ITopic[] Interact(IEntity instigator, IEntity listener)
        {
            base.Interact(instigator, listener);

            int exactNumber = listener.MyWorld.Entities.Count(entity =>
                entity.CreatureType.Equals(listener.CreatureType, StringComparison.OrdinalIgnoreCase));
            int roughNumber = 0;
            if (exactNumber > 10)
            {
                if (exactNumber % 10 < 6)
                {
                    roughNumber = exactNumber - (exactNumber % 10);
                }
                else
                {
                    roughNumber = exactNumber + (exactNumber % 10);
                }                
            }
            else
            {
                roughNumber = exactNumber;
            }
            string words = "I think there are about " + roughNumber + " " + listener.CreatureType + " here.";
            
            return new ITopic[]
            {
                new TopicData(
                    new ITopicCondition[0],
                    "LocalAreaInfo",
                    new[]
                    {
                        "Thanks"
                    },
                    words,
                    0,
                    null,
                    Speaker.LISTENER)
            };
        }
    }
}