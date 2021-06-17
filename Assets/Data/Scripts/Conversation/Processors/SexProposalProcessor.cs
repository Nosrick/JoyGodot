using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using JoyGodot.Assets.Scripts.Conversation.Conversations;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Relationships;
using JoyGodot.Assets.Scripts.Entities.Statistics;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Scripting;

namespace JoyGodot.Assets.Data.Scripts.Conversation.Processors
{
    public class SexProposalProcessor : TopicData
    {
        protected bool Happening { get; set; }
        
        public SexProposalProcessor()
            : base(
                new ITopicCondition[0],
                "SexProposal",
                new string[0],
                "words",
                0,
                null,
                Speaker.INSTIGATOR)
        {}

        public override ITopic[] Interact(IEntity instigator, IEntity listener)
        {
            this.Happening = false;

            IJoyObject[] participants = {instigator, listener};
            
            List<IRelationship> relationships = this.RelationshipHandler.Get(participants, new[] {"sexual"}, false).ToList();

            if (relationships.IsNullOrEmpty()
                && listener.Sexuality.Compatible(listener, instigator)
                && instigator.Sexuality.Compatible(instigator, listener))
            {
                relationships.Add(this.RelationshipHandler.CreateRelationship(participants, new string[] {"sexual"}));
            }
            
            if (listener.Sexuality.WillMateWith(listener, instigator, relationships) == false
            || instigator.Sexuality.WillMateWith(instigator, listener, relationships) == false)
            {
                return base.Interact(instigator, listener);
            }
            
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
                new IJoyObject[] {instigator, listener},
                new[] {"sex", "need"},
                new Dictionary<string, object>
                {
                    {"need", "sex"},
                    {"value", instigatorSatisfaction},
                    {"counter", 10},
                    {"overwrite", true}
                });
            fulfillNeed.Execute(
                new IJoyObject[] {listener, instigator},
                new[] {"sex", "need"},
                new Dictionary<string, object>
                {
                    {"need", "sex"},
                    {"value", listenerSatisfaction},
                    {"counter", 10},
                    {"overwrite", true}
                });
            
            base.Interact(instigator, listener);

            this.Happening = true;

            return this.FetchNextTopics();
        }

        protected override ITopic[] FetchNextTopics()
        {
            if (!this.Happening)
            {
                return new ITopic[]
                {
                    new TopicData(
                        new ITopicCondition[0],
                        "SexRejection",
                        new string[] {"BaseTopics"},
                        "No thank you.",
                        0,
                        null,
                        Speaker.LISTENER)
                };
            }
            return new ITopic[]
            {
                new TopicData(
                    new ITopicCondition[0],
                    "SexAcceptance",
                    new string[0],
                    "words",
                    0,
                    null,
                    Speaker.LISTENER)
            };
        }
    }
}