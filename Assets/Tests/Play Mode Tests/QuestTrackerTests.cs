using System;
using JoyGodot.Assets.Scripts;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Needs;
using JoyGodot.Assets.Scripts.Entities.Statistics;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.Items;
using JoyGodot.Assets.Scripts.Quests;
using JoyGodot.Assets.Scripts.Rollers;
using JoyGodot.Assets.Scripts.Scripting;
using JoyGodot.Assets.Scripts.World;
using Moq;
using NUnit.Framework;

namespace JoyGodot.Assets.Tests.Play_Mode_Tests
{
    public class QuestTrackerTests
    {
        private IQuestTracker target;

        private IQuestProvider questProvider;
        
        private ScriptingEngine scriptingEngine;

        private INeedHandler NeedHandler;
        private IEntitySkillHandler SkillHandler;
        private ILiveItemHandler ItemHandler;

        private WorldInstance world;
        
        private IEntity left;
        private IEntity right;
        
        [SetUp]
        public void SetUp()
        {
            ActionLog actionLog = new ActionLog();
            GlobalConstants.ActionLog = actionLog;
            GlobalConstants.ScriptingEngine = new ScriptingEngine();

            this.ItemHandler = new LiveItemHandler(new RNG());
            IGameManager gameManager = Mock.Of<IGameManager>(
                manager => manager.ItemHandler == this.ItemHandler);

            GlobalConstants.GameManager = gameManager;

            this.target = new QuestTracker(this.ItemHandler);
        }
        
        [SetUp]
        public void SetUpEntities()
        {
            this.left = Mock.Of<IEntity>(
                entity => entity.Guid == Guid.NewGuid()
                && entity.JoyName == "TEST1"
                && entity.PlayerControlled == true);

            this.right = Mock.Of<IEntity>(
                entity => entity.JoyName == "TEST2"
                && entity.Guid == Guid.NewGuid());
        }

        [Test]
        public void QuestTracker_Should_AddQuest()
        {
            //given
            IQuest quest = Mock.Of<IQuest>();
            
            //when
            this.target.AddQuest(this.left.Guid, quest);

            //then
            Assert.That(this.target.GetQuestsForEntity(this.left.Guid), Is.Not.Empty);
        }

        [Test]
        public void QuestTracker_Should_AdvanceOrCompleteQuest()
        {
            //given
            IJoyAction action = Mock.Of<IJoyAction>();
            IQuest quest = Mock.Of<IQuest>(
                q => q.AdvanceStep()
                && q.FulfilsRequirements(this.left, action) == true
                && q.CompleteQuest(this.left, false) == true
                && q.IsComplete);

            this.target.AddQuest(this.left.Guid, quest);
            quest.StartQuest(this.left);
            
            //when
            this.target.PerformQuestAction(this.left, quest, action);

            //then
            Assert.That(this.target.GetQuestsForEntity(this.left.Guid), Is.Empty);
        }

        [TearDown]
        public void TearDown()
        {
            GlobalConstants.GameManager = null;
            GlobalConstants.ActionLog = null;
            GlobalConstants.ScriptingEngine = null;
        }
    }
}