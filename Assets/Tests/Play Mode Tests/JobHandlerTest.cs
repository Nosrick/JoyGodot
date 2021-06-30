using System.Linq;
using JoyGodot.Assets.Scripts;
using JoyGodot.Assets.Scripts.Entities.Abilities;
using JoyGodot.Assets.Scripts.Entities.Jobs;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.Rollers;
using NUnit.Framework;

namespace JoyGodot.Assets.Tests.Play_Mode_Tests
{
    public class JobHandlerTest
    {
        private IJobHandler target;
        private IAbilityHandler abilityHandler;

        private ActionLog log;

        [SetUp]
        public void SetUp()
        {
            GlobalConstants.ActionLog = new ActionLog();
            this.abilityHandler = new AbilityHandler();
            this.target = new JobHandler(this.abilityHandler, new RNG());
        }

        [Test]
        public void LoadTypes_ShouldHaveValidData()
        {
            //given

            //when
            IJob[] jobs = this.target.Values.ToArray();

            //then
            Assert.That(jobs, Is.Not.Empty);
            foreach(IJob job in jobs)
            {
                Assert.That(job.SkillDiscounts, Is.Not.Empty);
                Assert.That(job.StatisticDiscounts, Is.Not.Empty);
                Assert.IsNotEmpty(job.Name);
            }
        }

        [TearDown]
        public void TearDown()
        {
            GlobalConstants.ActionLog.Dispose();
        }
    }
}