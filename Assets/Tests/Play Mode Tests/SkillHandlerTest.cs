using System.Collections.Generic;
using JoyGodot.Assets.Scripts;
using JoyGodot.Assets.Scripts.Entities.Statistics;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.Scripting;
using NUnit.Framework;

namespace JoyGodot.Assets.Tests.Play_Mode_Tests
{
    public class SkillHandlerTest
    {
        private IEntitySkillHandler target;
    
        [SetUp]
        public void Initialise()
        {
            ActionLog actionLog = new ActionLog();
            GlobalConstants.ActionLog = actionLog;
            GlobalConstants.ScriptingEngine = new ScriptingEngine();
            
            this.target = new EntitySkillHandler();
        }
    
        [Test]
        public void GetDefaultSkillBlock_ShouldHave_ValidData()
        {
            IDictionary<string, IEntitySkill> skills = this.target.GetDefaultSkillBlock();
    
            foreach (IEntitySkill skill in skills.Values)
            {
                Assert.That(skill.Name, Is.Not.Empty);
                Assert.That(skill.Value, Is.Zero);
                Assert.That(skill.SuccessThreshold, Is.EqualTo(GlobalConstants.DEFAULT_SUCCESS_THRESHOLD));
            }

            
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

