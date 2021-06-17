using System;
using System.Collections.Generic;
using System.Linq;
using JoyGodot.Assets.Scripts;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Items;
using JoyGodot.Assets.Scripts.Entities.Relationships;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Managers;
using JoyGodot.Assets.Scripts.Quests;
using JoyGodot.Assets.Scripts.Rollers;
using JoyGodot.Assets.Scripts.Scripting;
using JoyGodot.Assets.Scripts.World;
using Moq;
using NUnit.Framework;

namespace JoyGodot.Assets.Tests.Play_Mode_Tests
{
    public class QuestProviderTests
    {
        private IQuestTracker questTracker;

        private IQuestProvider target;

        private IGameManager gameManager;

        private ScriptingEngine scriptingEngine;

        private IWorldInstance overworld;
        private IWorldInstance world;

        private IEntity left;
        private IEntity right;

        [SetUp]
        public void SetUp()
        {
            ActionLog actionLog = new ActionLog();
            GlobalConstants.ActionLog = actionLog;
            this.scriptingEngine = new ScriptingEngine();

            IItemInstance item = Mock.Of<IItemInstance>();

            this.overworld = Mock.Of<IWorldInstance>(
                w => w.Name == "overworld"
                     && w.GetWorlds(It.IsAny<IWorldInstance>()) == new List<IWorldInstance>
                     {
                         Mock.Of<IWorldInstance>(
                             instance => instance.Guid == Guid.NewGuid())
                     }
                     && w.GetRandomSentientWorldWide() == Mock.Of<IEntity>(
                         entity => entity.Guid == Guid.NewGuid()
                                   && entity.MyWorld == this.world
                                   && entity.Contents == new List<IItemInstance> {item}));

            this.world = Mock.Of<IWorldInstance>(
                w => w.GetOverworld() == this.overworld
                     && w.Name == "TEST"
                     && w.Guid == Guid.NewGuid());

            this.left = Mock.Of<IEntity>(
                entity => entity.Guid == Guid.NewGuid()
                          && entity.PlayerControlled == true
                          && entity.MyWorld == this.world
                          && entity.HasDataKey(It.IsAny<string>()) == false);

            this.right = Mock.Of<IEntity>(
                entity => entity.Guid == Guid.NewGuid()
                          && entity.MyWorld == this.world
                          && entity.Contents == new List<IItemInstance> {item});

            IRelationship friendship = Mock.Of<IRelationship>(
                relationship => relationship.GetRelationshipValue(It.IsAny<Guid>(), It.IsAny<Guid>()) == 0);

            IEntityRelationshipHandler relationshipHandler = Mock.Of<IEntityRelationshipHandler>(
                handler => handler.Get(It.IsAny<IJoyObject[]>(), It.IsAny<string[]>(), It.IsAny<bool>())
                           == new[] {friendship});
            ILiveItemHandler itemHandler = Mock.Of<ILiveItemHandler>(
                handler => handler.GetQuestRewards(It.IsAny<Guid>()) == new List<IItemInstance> { item });
            IItemFactory itemFactory = Mock.Of<IItemFactory>(
                factory => factory.CreateRandomItemOfType(
                               It.IsAny<string[]>(),
                               It.IsAny<bool>()) == item
                           && factory.CreateSpecificType(
                               It.IsAny<string>(),
                               It.IsAny<string[]>(),
                               It.IsAny<bool>()) == item
                           && factory.CreateRandomWeightedItem(
                               It.IsAny<bool>(),
                               It.IsAny<bool>()) == item);

            this.gameManager = Mock.Of<IGameManager>(
                manager => manager.ItemFactory == itemFactory
                           && manager.Player == this.left
                           && manager.ItemHandler == itemHandler
                           && manager.GUIDManager == new GUIDManager());

            GlobalConstants.GameManager = this.gameManager;

            this.target = new QuestProvider(
                relationshipHandler,
                itemHandler,
                itemFactory,
                new RNG());
            this.questTracker = new QuestTracker();
        }

        [Test]
        public void QuestProvider_ShouldHave_NonZeroQuests()
        {
            //given

            //when

            //then
            Assert.That(this.target.Actions, Is.Not.Empty);
        }

        [Test]
        public void QuestProvider_ShouldGenerate_ValidQuest()
        {
            //given

            //when
            IQuest[] quests = this.target.MakeOneOfEachType(this.left, this.right, this.overworld).ToArray();

            //then
            foreach (IQuest quest in quests)
            {
                Assert.That(quest.Rewards, Is.Not.Empty);
                Assert.That(quest.Actions, Is.Not.Empty);
            }
        }

        [TearDown]
        public void TearDown()
        {
            GlobalConstants.GameManager = null;
            GlobalConstants.ActionLog.Dispose();
        }
    }
}