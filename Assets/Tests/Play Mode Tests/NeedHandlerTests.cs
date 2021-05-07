using System.Collections;
using System.Collections.Generic;
using JoyGodot.addons.Managed_Assets;
using JoyLib.Code;
using JoyLib.Code.Entities.Needs;
using JoyLib.Code.Entities.Relationships;
using JoyLib.Code.Graphics;
using JoyLib.Code.Helpers;
using JoyLib.Code.Scripting;
using Moq;
using NUnit.Framework;

namespace Tests
{
    public class NeedHandlerTests
    {
        private ScriptingEngine ScriptingEngine;
        
        private INeedHandler target;
    
        [SetUp]
        public void SetUp()
        {
            GlobalConstants.ActionLog = new ActionLog();
            ScriptingEngine = new ScriptingEngine();

            IGameManager gameManager = Mock.Of<IGameManager>(
                manager => manager.RelationshipHandler == Mock.Of<IEntityRelationshipHandler>()
                && manager.ObjectIconHandler == Mock.Of<IObjectIconHandler>(
                    handler => handler.GetSprites(
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
            
            target = new NeedHandler();
        }
    
        [Test]
        public void Initialise_ShouldHave_ValidData()
        {
            //given
            
            //when
            
            //then
            Assert.That(target.Values, Is.Not.Empty);
            foreach (INeed need in target.Values)
            {
                Assert.That(need.Name, Is.Not.Empty);
                Assert.That(need.Name, Is.Not.EqualTo("DEFAULT"));
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
