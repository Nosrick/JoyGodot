using System.Collections.Generic;
using JoyGodot.Assets.Scripts;
using JoyGodot.Assets.Scripts.Conversation.Conversations;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Statistics;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Scripting;

namespace JoyGodot.Assets.Data.Scripts.Conversation.Processors
{
    public class SexDecision : TopicData
    {
        protected bool Happening { get; set; }

        public SexDecision(string decision, bool happening)
            : base(
                new ITopicCondition[0],
                "SexDecision",
                new string[0],
                decision, 
                new []{"relationship", "query", "sexual"},
                0,
                null,
                Speaker.LISTENER)
        {
            this.Happening = happening;
        }

        public override ITopic[] Interact(IEntity instigator, IEntity listener)
        {
            if (this.Happening == false)
            {
                return base.Interact(instigator, listener);
            }
            
            base.Interact(instigator, listener);
            
            IJoyAction fulfillNeed = instigator.FetchAction("fulfillneedaction");

            int listenerSatisfaction = (
                instigator.Statistics[EntityStatistic.INTELLECT].Value
                + instigator.Statistics[EntityStatistic.ENDURANCE].Value
                + instigator.Statistics[EntityStatistic.PERSONALITY].Value) / 3;
            
            int instigatorSatisfaction = (
                listener.Statistics[EntityStatistic.INTELLECT].Value
                + listener.Statistics[EntityStatistic.ENDURANCE].Value
                + listener.Statistics[EntityStatistic.PERSONALITY].Value) / 3;
            
            fulfillNeed.Execute(
                new IJoyObject[] {instigator},
                new[] {"sex", "need"},
                new Dictionary<string, object>
                {
                    {"need", "sex"},
                    {"value", instigatorSatisfaction},
                    {"counter", 10}
                });
            fulfillNeed.Execute(
                new IJoyObject[] {listener},
                new[] {"sex", "need"},
                new Dictionary<string, object>
                {
                    {"need", "sex"},
                    {"value", listenerSatisfaction},
                    {"counter", 10}
                });

            return this.FetchNextTopics();
        }

        protected override ITopic[] FetchNextTopics()
        {
            if (this.Happening)
            {
                return new ITopic[0];
            }
            else
            {
                return new ITopic[]
                {
                    new TopicData(
                        new ITopicCondition[0],
                        "SexReject",
                        new string[0],
                        this.Words, 
                        new []{"relationship", "negative", "sexual"},
                        0,
                        null,
                        Speaker.LISTENER)
                };
            }
        }
    }
}