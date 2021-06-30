using System.Collections.Generic;
using System.Linq;
using JoyGodot.Assets.Scripts;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Abilities;
using JoyGodot.Assets.Scripts.Entities.AI.LOS.Providers;
using JoyGodot.Assets.Scripts.Entities.Statistics;
using JoyGodot.Assets.Scripts.Helpers;
using Moq;
using NUnit.Framework;

namespace JoyGodot.Assets.Tests.Play_Mode_Tests
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
