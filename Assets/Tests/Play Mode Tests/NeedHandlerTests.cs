using System.Collections.Generic;
using JoyGodot.Assets.Scripts;
using JoyGodot.Assets.Scripts.Entities.Needs;
using JoyGodot.Assets.Scripts.Entities.Relationships;
using JoyGodot.Assets.Scripts.Graphics;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.Managed_Assets;
using JoyGodot.Assets.Scripts.Scripting;
using Moq;
using NUnit.Framework;

namespace JoyGodot.Assets.Tests.Play_Mode_Tests
{
    public class NeedHandlerTests
    {
        private ScriptingEngine ScriptingEngine;
        
        private INeedHandler target;
    
        [SetUp]
        public void SetUp()
        {
            GlobalConstants.ActionLog = new ActionLog();
            this.ScriptingEngine = new ScriptingEngine();

            IGameManager gameManager = Mock.Of<IGameManager>(
                manager => manager.RelationshipHandler == Mock.Of<IEntityRelationshipHandler>()
                && manager.ObjectIconHandler == Mock.Of<IObjectIconHandler>(
                    handler => handler.GetManagedSprites(
                        It.IsAny<string>(), 
                        It.IsAny<string>(),
                        It.IsAny<string>()) 
                               == new[]
                               {
                                   new SpriteData
                                   {
                                       Name = "DEFAULT",
                                       Parts = new List<SpritePart>
                                       {
                                           new SpritePart
                                           {
                                               m_Frames = 1
                                           }
                                       },
                                       State = "DEFAULT"
                                   }
                               }));

            GlobalConstants.GameManager = gameManager;
            
            this.target = new NeedHandler();
        }
    
        [Test]
        public void Initialise_ShouldHave_ValidData()
        {
            //given
            
            //when
            
            //then
            Assert.That(this.target.Values, Is.Not.Empty);
            foreach (INeed need in this.target.Values)
            {
                Assert.That(need.Name, Is.Not.Empty);
                Assert.That(need.Name, Is.Not.EqualTo("DEFAULT"));
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
