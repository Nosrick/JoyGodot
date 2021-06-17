using System;
using System.Collections.Generic;
using JoyGodot.Assets.Scripts;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Gender;
using JoyGodot.Assets.Scripts.Entities.Relationships;
using JoyGodot.Assets.Scripts.Entities.Sexes;
using JoyGodot.Assets.Scripts.Entities.Sexuality;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Scripting;
using Moq;
using NUnit.Framework;

namespace JoyGodot.Assets.Tests.Play_Mode_Tests
{
    public class SexualityTest
    {
        private ScriptingEngine scriptingEngine;

        private IEntityRelationshipHandler RelationshipHandler;

        private IEntitySexualityHandler target;

        private IEntity heteroMaleHuman;
        private IEntity heterofemaleHuman;

        private IEntity homoMaleHumanLeft;
        private IEntity homoMaleHumanRight;

        private IEntity homofemaleHumanLeft;
        private IEntity homofemaleHumanRight;

        private IEntity biMaleHuman;
        private IEntity bifemaleHuman;

        private IEntity asexualMaleHuman;

        private ISexuality heterosexual;
        private ISexuality homosexual;
        private ISexuality bisexual;
        private ISexuality asexual;

        [SetUp]
        public void SetUp()
        {
            ActionLog actionLog = new ActionLog();
            GlobalConstants.ActionLog = actionLog;
            this.scriptingEngine = new ScriptingEngine();

            this.target = new EntitySexualityHandler();
            this.RelationshipHandler = new EntityRelationshipHandler();
        }

        [SetUp]
        public void SetUpHumans()
        {
            IBioSex femaleSex = Mock.Of<IBioSex>(
                sex => sex.Name == "female"
                       && sex.CanBirth == true);
            IBioSex maleSex = Mock.Of<IBioSex>(
                sex => sex.Name == "male"
                       && sex.CanBirth == false);

            IGender femaleGender = Mock.Of<IGender>(gender => gender.Name == "female");
            IGender maleGender = Mock.Of<IGender>(gender => gender.Name == "male");

            this.heterosexual = this.target.Get("heterosexual");
            this.homosexual = this.target.Get("homosexual");
            this.bisexual = this.target.Get("pansexual");
            this.asexual = this.target.Get("asexual");

            this.heterofemaleHuman = Mock.Of<IEntity>(
                entity => entity.Gender == femaleGender
                          && entity.Sexuality == this.heterosexual
                          && entity.Guid == Guid.NewGuid());

            this.heteroMaleHuman = Mock.Of<IEntity>(
                entity => entity.Gender == maleGender
                          && entity.Sexuality == this.heterosexual
                          && entity.Guid == Guid.NewGuid());

            this.homoMaleHumanLeft = Mock.Of<IEntity>(
                entity => entity.Gender == maleGender
                          && entity.Sexuality == this.homosexual
                          && entity.Guid == Guid.NewGuid());

            this.homoMaleHumanRight = Mock.Of<IEntity>(
                entity => entity.Gender == maleGender
                          && entity.Sexuality == this.homosexual
                          && entity.Guid == Guid.NewGuid());

            this.homofemaleHumanLeft = Mock.Of<IEntity>(
                entity => entity.Gender == femaleGender
                          && entity.Sexuality == this.homosexual
                          && entity.Guid == Guid.NewGuid());

            this.homofemaleHumanRight = Mock.Of<IEntity>(
                entity => entity.Gender == femaleGender
                          && entity.Sexuality == this.homosexual
                          && entity.Guid == Guid.NewGuid());

            this.biMaleHuman = Mock.Of<IEntity>(
                entity => entity.Gender == maleGender
                          && entity.Sexuality == this.bisexual
                          && entity.Guid == Guid.NewGuid());

            this.bifemaleHuman = Mock.Of<IEntity>(
                entity => entity.Gender == femaleGender
                          && entity.Sexuality == this.bisexual
                          && entity.Guid == Guid.NewGuid());

            this.asexualMaleHuman = Mock.Of<IEntity>(
                entity => entity.Gender == maleGender
                          && entity.Sexuality == this.asexual
                          && entity.Guid == Guid.NewGuid());


            IEntity[] heteroCouple = new IEntity[] {this.heterofemaleHuman, this.heteroMaleHuman};
            IEntity[] homofemaleCouple = new IEntity[] {this.homofemaleHumanLeft, this.homofemaleHumanRight};
            IEntity[] homoMaleCouple = new IEntity[] {this.homoMaleHumanLeft, this.homoMaleHumanRight};
            IEntity[] biCoupleLeft = new IEntity[] {this.bifemaleHuman, this.homofemaleHumanLeft};
            IEntity[] biCoupleRight = new IEntity[] {this.bifemaleHuman, this.biMaleHuman};
            IEntity[] asexualCouple = new IEntity[] {this.asexualMaleHuman, this.bifemaleHuman};

            this.RelationshipHandler.CreateRelationshipWithValue(heteroCouple, new[] {"sexual"}, 500);
            this.RelationshipHandler.CreateRelationshipWithValue(homofemaleCouple, new[] {"sexual"}, 500);
            this.RelationshipHandler.CreateRelationshipWithValue(homoMaleCouple, new[] {"sexual"}, 500);
            this.RelationshipHandler.CreateRelationshipWithValue(biCoupleLeft, new[] {"sexual"}, 500);
            this.RelationshipHandler.CreateRelationshipWithValue(biCoupleRight, new[] {"sexual"}, 500);
            this.RelationshipHandler.CreateRelationshipWithValue(asexualCouple, new[] {"sexual"}, 500);
        }

        [Test]
        public void Heterosexual_WillMateWith_AcceptsHeteroPartners()
        {
            IJoyObject[] participants = new[] {this.heterofemaleHuman, this.heteroMaleHuman};
            IEnumerable<IRelationship> relationships = this.RelationshipHandler.Get(participants);
            Assert.IsTrue(this.heterosexual.WillMateWith(this.heterofemaleHuman, this.heteroMaleHuman, relationships));
        }

        [Test]
        public void Heterosexual_WillMateWith_RejectsHomoPartners()
        {
            IJoyObject[] participants = new[] {this.heterofemaleHuman, this.homofemaleHumanLeft};
            IEnumerable<IRelationship> relationships = this.RelationshipHandler.Get(participants);
            Assert.IsFalse(this.heterosexual.WillMateWith(this.heterofemaleHuman, this.homofemaleHumanLeft, relationships));
        }

        [Test]
        public void Homosexual_WillMateWith_AcceptsHomoPartners()
        {
            IJoyObject[] participants = new[] {this.homoMaleHumanLeft, this.homoMaleHumanRight};
            IEnumerable<IRelationship> relationships = this.RelationshipHandler.Get(participants);
            Assert.IsTrue(this.homosexual.WillMateWith(this.homoMaleHumanLeft, this.homoMaleHumanRight, relationships));
        }

        [Test]
        public void Homosexual_WillMateWith_RejectsHeteroPartners()
        {
            IJoyObject[] participants = new[] {this.homofemaleHumanLeft, this.homofemaleHumanRight};
            IEnumerable<IRelationship> relationships = this.RelationshipHandler.Get(participants);
            Assert.IsFalse(this.homosexual.WillMateWith(this.homoMaleHumanLeft, this.homofemaleHumanRight, relationships));
        }

        [Test]
        public void Bisexual_WillMateWith_WillAcceptHomoPartners()
        {
            IJoyObject[] participants = new[] {this.bifemaleHuman, this.homofemaleHumanLeft};
            IEnumerable<IRelationship> relationships = this.RelationshipHandler.Get(participants);
            Assert.IsTrue(this.bisexual.WillMateWith(this.bifemaleHuman, this.homofemaleHumanLeft, relationships));
        }

        [Test]
        public void Bisexual_WillMateWith_WillAcceptHeteroPartners()
        {
            IJoyObject[] participants = new[] {this.bifemaleHuman, this.biMaleHuman};
            IEnumerable<IRelationship> relationships = this.RelationshipHandler.Get(participants);
            Assert.IsTrue(this.bisexual.WillMateWith(this.bifemaleHuman, this.biMaleHuman, relationships));
        }

        [Test]
        public void Asexual_WillMateWith_RejectsPartner()
        {
            IJoyObject[] participants = new IJoyObject[] {this.asexualMaleHuman, this.bifemaleHuman};
            IEnumerable<IRelationship> relationships = this.RelationshipHandler.Get(participants);
            Assert.IsFalse(this.asexual.WillMateWith(this.asexualMaleHuman, this.bifemaleHuman, relationships));
        }

        [TearDown]
        public void TearDown()
        {
            GlobalConstants.GameManager = null;
            GlobalConstants.ActionLog.Dispose();
        }
    }
}