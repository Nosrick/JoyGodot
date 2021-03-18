using JoyLib.Code.Conversation.Conversations;
using JoyLib.Code.Helpers;
using JoyLib.Code.Rollers;
using JoyLib.Code.World;

namespace JoyLib.Code.Entities.Conversation.Processors
{
    public class LocalAreaInfoProcessor : TopicData
    {
        protected IEntity Listener
        {
            get;
            set;
        }
        
        public LocalAreaInfoProcessor() 
            : base(
                new ITopicCondition[0], 
                "LocalAreaInfo",
                new []{ "Thanks" },
                "",
                0,
                null, 
                Speaker.LISTENER,
                new RNG())
        {
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
                    new []{ "Thanks" }, this.GetAreaInfo(this.Listener),
                    0,
                    null, 
                    Speaker.LISTENER,
                    new RNG()) 
            };
        }

        protected string GetAreaInfo(IEntity listener)
        {
            string message = "";

            IWorldInstance listenerWorld = listener.MyWorld;
            if (listenerWorld.HasTag("interior"))
            {
                int result = this.Roller.Roll(0, 100);
                if (result <= 50)
                {
                    int numberOfLevels = 1;
                    numberOfLevels = WorldConversationDataHelper.GetNumberOfFloors(numberOfLevels, listenerWorld);

                    string plural = numberOfLevels > 1 ? "floors" : "floor";
                    message = "This place has " + numberOfLevels + plural + " to it.";
                }
                if (result > 50)
                {
                    int exactNumber = WorldConversationDataHelper.GetNumberOfCreatures(listener.CreatureType, listenerWorld);
                    int roughNumber = 0;
                    if (exactNumber < 10)
                    {
                        roughNumber = exactNumber;
                    }
                    else if (exactNumber % 10 < 6)
                    {
                        roughNumber = exactNumber - (exactNumber % 10);
                    }
                    else
                    {
                        roughNumber = exactNumber + (exactNumber % 10);
                    }

                    string plural = roughNumber > 1 ? "s" : "";
                    message = "There are around " + roughNumber + " " + listener.CreatureType + plural + " here.";
                }
            }
            else
            {
                message = "I don't know much about this place, sorry.";
            }

            return message;
        }
    }
}