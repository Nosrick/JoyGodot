using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using JoyGodot.Assets.Scripts;
using JoyGodot.Assets.Scripts.Conversation;
using JoyGodot.Assets.Scripts.Conversation.Conversations;
using JoyGodot.Assets.Scripts.Conversation.Subengines.Rumours;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Needs;
using JoyGodot.Assets.Scripts.Entities.Relationships;
using JoyGodot.Assets.Scripts.Entities.Statistics;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Rollers;
using JoyGodot.Assets.Scripts.Scripting;
using JoyGodot.Assets.Scripts.World;
using Moq;
using NUnit.Framework;

namespace JoyGodot.Assets.Tests.Play_Mode_Tests
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
            
            this.prefab = GD.Load<Node2D>(GlobalConstants.GODOT_ASSETS_FOLDER + "Scenes/JoyObject.tscn");

            this.scriptingEngine = new ScriptingEngine();

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

            this.world = Mock.Of<IWorldInstance>(
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

            this.instigator = Mock.Of<IEntity>(
                entity => entity.PlayerControlled == true
                          && entity.MyWorld == this.world
                          && entity.Needs == needs
                          && entity.Statistics == stats
                          && entity.Sentient == true
                          && entity.Guid == Guid.NewGuid());

            this.listener = Mock.Of<IEntity>(entity => entity.MyWorld == this.world
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
            ITopic[] topics = this.target.AllTopics;

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

            this.target.SetActors(this.instigator, this.listener);

            ICollection<ITopic> baseTopics = this.target.Converse();
            bool ended = false;
            foreach (ITopic topic in baseTopics)
            {
                ended = this.AdvanceToEnd(topic, baseTopics);
            }

            Assert.That(ended, Is.True);
        }

        private bool AdvanceToEnd(ITopic topic, ICollection<ITopic> baseTopics)
        {
            ICollection<ITopic> nextTopics = this.target.Converse(topic);
            if (nextTopics.Intersect(baseTopics).Count() == baseTopics.Count)
            {
                return true;
            }

            foreach (ITopic next in nextTopics)
            {
                this.AdvanceToEnd(next, baseTopics);
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