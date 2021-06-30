using System;
using JoyGodot.Assets.Scripts;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Relationships;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.Scripting;
using Moq;
using NUnit.Framework;

namespace JoyGodot.Assets.Tests.Play_Mode_Tests
{
    public class EntityRelationshipHandlerTests
    {
        private ScriptingEngine scriptingEngine;

        private IEntityRelationshipHandler target;
        
        private IEntity left;
        private IEntity right;
        
        [SetUp]
        public void SetUp()
        {
            GlobalConstants.ActionLog = new ActionLog();
            this.scriptingEngine = new ScriptingEngine();

            ILiveEntityHandler entityHandler = new LiveEntityHandler();
            IGameManager gameManager = Mock.Of<IGameManager>(
                manager => manager.EntityHandler == entityHandler);
            GlobalConstants.GameManager = gameManager;

            this.target = new EntityRelationshipHandler();
        }

        [SetUp]
        public void SetUpEntities()
        {
            this.left = Mock.Of<IEntity>(entity => entity.Guid == Guid.NewGuid());
            this.right = Mock.Of<IEntity>(entity => entity.Guid == Guid.NewGuid());
        }

        [Test]
        public void CreateRelationship_ShouldHave_ZeroValue()
        {
            //given
            IRelationship relationship = this.target.CreateRelationship(new[] {this.left, this.right}, new []{"friendship"});
            
            //when

            //then
            Assert.That(relationship.GetRelationshipValue(this.left.Guid, this.right.Guid), Is.EqualTo(0));
        }

        [Test]
        public void CreateRelationshipWithValue_ShouldHave_NonZeroValue()
        {
            //given
            IRelationship relationship = this.target.CreateRelationshipWithValue(
                new[] {this.left, this.right},
                new [] {"friendship"},
                50);
            
            //when
            
            //then
            Assert.That(relationship.GetRelationshipValue(this.left.Guid, this.right.Guid), Is.EqualTo(50));
        }

        [TearDown]
        public void TearDown()
        {
            GlobalConstants.GameManager = null;
            GlobalConstants.ActionLog.Dispose();
        }
    }
}