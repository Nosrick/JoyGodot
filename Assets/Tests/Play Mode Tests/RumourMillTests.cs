using System.Collections.Generic;
using Godot;
using JoyGodot.Assets.Scripts;
using JoyGodot.Assets.Scripts.Conversation.Subengines.Rumours;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Gender;
using JoyGodot.Assets.Scripts.Entities.Needs;
using JoyGodot.Assets.Scripts.Entities.Relationships;
using JoyGodot.Assets.Scripts.Entities.Statistics;
using JoyGodot.Assets.Scripts.Graphics;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Scripting;
using JoyGodot.Assets.Scripts.World;
using Moq;
using NUnit.Framework;

namespace JoyGodot.Assets.Tests.Play_Mode_Tests
{
    public class RumourMillTests
    {
        private IRumourMill target;
        
        private ScriptingEngine scriptingEngine;
        
        private IEntityRelationshipHandler RelationshipHandler;
        
        private IEntity left;
        private IEntity right;

        [SetUp]
        public void SetUp()
        {
            ActionLog actionLog = new ActionLog();
            GlobalConstants.ActionLog = actionLog;
            this.scriptingEngine = new ScriptingEngine();
            
            this.target = new ConcreteRumourMill();

            IWorldInstance world = Mock.Of<IWorldInstance>();

            IGameManager gameManager = Mock.Of<IGameManager>(
                manager => manager.NeedHandler == Mock.Of<INeedHandler>(
                               handler => handler.GetManyRandomised(It.IsAny<IEnumerable<string>>())
                               == new List<INeed>())
                && manager.SkillHandler == Mock.Of<IEntitySkillHandler>(
                    handler => handler.GetDefaultSkillBlock()
                    == new Dictionary<string, IEntitySkill>
                    {
                        {
                            "light blades", 
                            new EntitySkill(
                                "light blades", 
                                5, 
                                7,
                                new List<string>())
                        }
                    })
                && manager.RelationshipHandler == Mock.Of<IEntityRelationshipHandler>()
                           && manager.ObjectIconHandler == Mock.Of<IObjectIconHandler>());

            GlobalConstants.GameManager = gameManager;

            IGender gender = Mock.Of<IGender>(
                g => g.PersonalSubject == "her");

            IDictionary<string, IEntitySkill> skills = gameManager.SkillHandler.GetDefaultSkillBlock();

            this.left = Mock.Of<IEntity>(
                entity => entity.PlayerControlled == true
                && entity.JoyName == "TEST1"
                && entity.Skills == skills);

            this.right = Mock.Of<IEntity>(
                entity => entity.JoyName == "TEST2"
                          && entity.Skills == skills);
        }

        [Test]
        public void RumourMill_ShouldHave_NonZeroRumourTypeCount()
        {
            //given
            
            //when
            
            //then
            Assert.That(this.target.RumourTypes, Is.Not.Empty);
        }

        [Test]
        public void RumourMill_ShouldMake_ValidRumours()
        {
            //given
            IRumour[] rumours = this.target.GenerateOneRumourOfEachType(new IJoyObject[] {this.left, this.right});

            //then
            foreach (IRumour rumour in rumours)
            {
                Assert.That(rumour.Words, Does.Not.Contain("<"));
                Assert.That(rumour.Words, Does.Not.Contain("PARAMETER NUMBER MISMATCH"));
                GD.Print(rumour.Words);
            }
        }

        [TearDown]
        public void TearDown()
        {
            GlobalConstants.GameManager = null;
            GlobalConstants.ActionLog = null;
        }
    }
}