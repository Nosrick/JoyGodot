using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JoyLib.Code;
using JoyLib.Code.Entities;
using JoyLib.Code.Entities.Abilities;
using JoyLib.Code.Entities.AI.LOS.Providers;
using JoyLib.Code.Helpers;
using Moq;
using NUnit.Framework;

namespace Tests
{
    public class EntityTemplateHandlerTest
    {
        private IEntityTemplateHandler target;
        
        [SetUp]
        public void SetUp()
        {
            ActionLog actionLog = new ActionLog();
            GlobalConstants.ActionLog = actionLog;
            IEntitySkillHandler skillHandler = Mock.Of<IEntitySkillHandler>();
            IVisionProviderHandler visionProviderHandler = Mock.Of<IVisionProviderHandler>(
                handler => handler.Get(It.IsAny<string>()) == Mock.Of<IVision>());
            IAbilityHandler abilityHandler = Mock.Of<IAbilityHandler>();
            
            this.target = new EntityTemplateHandler(
                skillHandler,
                visionProviderHandler,
                abilityHandler);
        }

        [Test]
        public void LoadTypes_ShouldHaveValidData()
        {
            //given
            List<IEntityTemplate> entityTemplates = this.target.Values.ToList();

            //when

            //then
            Assert.That(entityTemplates, Is.Not.Empty);
            foreach(IEntityTemplate template in entityTemplates)
            {
                Assert.That(template.Statistics.Values, Is.Not.Empty);
                Assert.That(template.Slots, Is.Not.Empty);
                Assert.That(template.Tags, Is.Not.Empty);
                Assert.False(template.JoyType == "DEFAULT");
                Assert.False(template.CreatureType == "DEFAULT");
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
