using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;
using JoyLib.Code;
using JoyLib.Code.Conversation;
using JoyLib.Code.Conversation.Conversations;
using JoyLib.Code.Entities;
using JoyLib.Code.Entities.Needs;
using JoyLib.Code.Entities.Relationships;
using JoyLib.Code.Entities.Statistics;
using JoyLib.Code.Helpers;
using JoyLib.Code.Rollers;
using JoyLib.Code.Scripting;
using JoyLib.Code.World;
using Moq;
using NUnit.Framework;

namespace Tests
{
    public class ConversationEngineTests
    {
        private ScriptingEngine scriptingEngine;

        private IConversationEngine target;

        private Node2D prefab;

        private IEntity instigator;
        private IEntity listener;

        private IWorldInstance world;

        [SetUp]
        public void SetUp()
        {
            GlobalConstants.ActionLog = new ActionLog();
            
            prefab = GD.Load<Node2D>(GlobalConstants.GODOT_ASSETS_FOLDER + "Scenes/JoyObject.tscn");

            scriptingEngine = new ScriptingEngine();

            ILiveEntityHandler entityHandler = Mock.Of<ILiveEntityHandler>();

            IRelationship friendship = Mock.Of<IRelationship>();
            
            IEntityRelationshipHandler relationshipHandler = Mock.Of<IEntityRelationshipHandler>(
                handler => handler.Get(It.IsAny<IJoyObject[]>(), It.IsAny<string[]>(), It.IsAny<bool>())
                           == new IRelationship[] {friendship});

            IEntity questObject = Mock.Of<IEntity>(
                entity => entity.JoyName == "NAME1" 
                    && entity.MyWorld == Mock.Of<IWorldInstance>(
                    w => w.GetRandomSentient() == Mock.Of<IEntity>(
                        e => e.JoyName == "NAME2")));

            world = Mock.Of<IWorldInstance>(
                w => w.GetRandomSentientWorldWide() == questObject
                     && w.GetRandomSentient() == questObject
                     && w.GetWorlds(It.IsAny<IWorldInstance>()) == new List<IWorldInstance>
                     {
                         Mock.Of<IWorldInstance>(mock => mock.Name == "TEST2")
                     }
                     && w.GetOverworld() == Mock.Of<IWorldInstance>(
                         mock => mock.Name == "EVERSE"
                         && mock.GetRandomSentient() == questObject
                         && mock.GetRandomSentientWorldWide() == questObject)
                     && w.Name == "TEST");

            INeed friendshipMock = Mock.Of<INeed>(
                need => need.Fulfill(It.IsAny<int>()) == 1
                        && need.Name == "friendship");
            IDictionary<string, INeed> needs = new Dictionary<string, INeed>();
            needs.Add("friendship", friendshipMock);
            
            IRollableValue<int> mockPersonality = Mock.Of<IEntityStatistic>(
                value => value.Value == 4
                         && value.Name == "personality");
            IDictionary<string, IEntityStatistic> stats = new Dictionary<string, IEntityStatistic>();
            stats.Add(
                "personality", 
                new EntityStatistic(
                    "personality",
                    4,
                    GlobalConstants.DEFAULT_SUCCESS_THRESHOLD));

            instigator = Mock.Of<IEntity>(
                entity => entity.PlayerControlled == true
                          && entity.MyWorld == world
                          && entity.Needs == needs
                          && entity.Statistics == stats
                          && entity.Sentient == true
                          && entity.Guid == Guid.NewGuid());

            listener = Mock.Of<IEntity>(entity => entity.MyWorld == world
                                                  && entity.Needs == needs
                                                  && entity.Statistics == stats
                                                  && entity.Sentient == true
                                                  && entity.Guid == Guid.NewGuid());

            GlobalConstants.GameManager = Mock.Of<IGameManager>(
                manager => manager.Player == this.instigator
                           && manager.ConversationEngine == new ConversationEngine(relationshipHandler, Guid.NewGuid())
                           && manager.RelationshipHandler == relationshipHandler
                           && manager.RumourMill == new ConcreteRumourMill(new RNG()));

            this.target = GlobalConstants.GameManager.ConversationEngine;
            
            friendship.AddParticipant(this.listener.Guid);
            friendship.AddParticipant(this.instigator.Guid);
        }

        [Test]
        public void LoadData_ShouldNotBeEmpty()
        {
            //given

            //when
            ITopic[] topics = target.AllTopics;

            //then
            Assert.That(topics, Is.Not.Empty);
            foreach (ITopic topic in topics)
            {
                Assert.That(topic.Words.Length, Is.GreaterThan(0));
                Assert.That(topic.ID.Length, Is.GreaterThan(0));
            }
        }

        [Test]
        public void Converse_ShouldCompleteConversation()
        {
            int depth = 0;

            target.SetActors(instigator, listener);

            ITopic[] baseTopics = target.Converse();
            bool ended = false;
            foreach (ITopic topic in baseTopics)
            {
                ended = AdvanceToEnd(topic, baseTopics);
            }

            Assert.That(ended, Is.True);
        }

        private bool AdvanceToEnd(ITopic topic, ITopic[] baseTopics)
        {
            ITopic[] nextTopics = target.Converse(topic.ID);
            if (nextTopics.Intersect(baseTopics).Count() == baseTopics.Length)
            {
                return true;
            }

            foreach (ITopic next in nextTopics)
            {
                AdvanceToEnd(next, baseTopics);
            }

            return true;
        }

        [TearDown]
        public void TearDown()
        {
            GlobalConstants.GameManager = null;
            GlobalConstants.ActionLog.Dispose();
        }
    }
}