using System;
using System.Collections.Generic;
using System.Linq;
using JoyGodot.Assets.Scripts.Conversation.Conversations;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Statistics;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Rollers;
using JoyGodot.Assets.Scripts.Scripting;

namespace JoyGodot.Assets.Data.Scripts.Conversation.Processors
{
    public class RomanceNo : TopicData
    {
        public RomanceNo()
            : base(
                new ITopicCondition[0], 
                "RomanceNo",
                new []{ "BaseTopics" },
                "No thanks.",
                0,
                null, 
                Speaker.INSTIGATOR,
                new RNG())
        {}

        public override ITopic[] Interact(IEntity instigator, IEntity listener)
        {
            IJoyAction influence = this.CachedActions.First(action =>
                action.Name.Equals("modifyrelationshippointsaction", StringComparison.OrdinalIgnoreCase));

            influence.Execute(
                new IJoyObject[]
                {
                    listener,
                    instigator
                },
                new[] {"friendship"},
                new Dictionary<string, object>
                {
                    {"value", -instigator.Statistics[EntityStatistic.PERSONALITY].Value}
                });
            
            return this.FetchNextTopics();
        }
    }
}