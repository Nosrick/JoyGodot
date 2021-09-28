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


            Guid[] heteroCouple = new Guid[] {this.heterofemaleHuman.Guid, this.heteroMaleHuman.Guid};
            Guid[] homofemaleCouple = new Guid[] {this.homofemaleHumanLeft.Guid, this.homofemaleHumanRight.Guid};
            Guid[] homoMaleCouple = new Guid[] {this.homoMaleHumanLeft.Guid, this.homoMaleHumanRight.Guid};
            Guid[] biCoupleLeft = new Guid[] {this.bifemaleHuman.Guid, this.homofemaleHumanLeft.Guid};
            Guid[] biCoupleRight = new Guid[] {this.bifemaleHuman.Guid, this.biMaleHuman.Guid};
            Guid[] asexualCouple = new Guid[] {this.asexualMaleHuman.Guid, this.bifemaleHuman.Guid};

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
            Guid[] participants = new[] {this.heterofemaleHuman.Guid, this.heteroMaleHuman.Guid};
            IEnumerable<IRelationship> relationships = this.RelationshipHandler.Get(participants);
            Assert.IsTrue(this.heterosexual.WillMateWith(this.heterofemaleHuman, this.heteroMaleHuman, relationships));
        }

        [Test]
        public void Heterosexual_WillMateWith_RejectsHomoPartners()
        {
            Guid[] participants = new[] {this.heterofemaleHuman.Guid, this.homofemaleHumanLeft.Guid};
            IEnumerable<IRelationship> relationships = this.RelationshipHandler.Get(participants);
            Assert.IsFalse(this.heterosexual.WillMateWith(this.heterofemaleHuman, this.homofemaleHumanLeft, relationships));
        }

        [Test]
        public void Homosexual_WillMateWith_AcceptsHomoPartners()
        {
            Guid[] participants = new[] {this.homoMaleHumanLeft.Guid, this.homoMaleHumanRight.Guid};
            IEnumerable<IRelationship> relationships = this.RelationshipHandler.Get(participants);
            Assert.IsTrue(this.homosexual.WillMateWith(this.homoMaleHumanLeft, this.homoMaleHumanRight, relationships));
        }

        [Test]
        public void Homosexual_WillMateWith_RejectsHeteroPartners()
        {
            Guid[] participants = new[] {this.homofemaleHumanLeft.Guid, this.homofemaleHumanRight.Guid};
            IEnumerable<IRelationship> relationships = this.RelationshipHandler.Get(participants);
            Assert.IsFalse(this.homosexual.WillMateWith(this.homoMaleHumanLeft, this.homofemaleHumanRight, relationships));
        }

        [Test]
        public void Bisexual_WillMateWith_WillAcceptHomoPartners()
        {
            Guid[] participants = new[] {this.bifemaleHuman.Guid, this.homofemaleHumanLeft.Guid};
            IEnumerable<IRelationship> relationships = this.RelationshipHandler.Get(participants);
            Assert.IsTrue(this.bisexual.WillMateWith(this.bifemaleHuman, this.homofemaleHumanLeft, relationships));
        }

        [Test]
        public void Bisexual_WillMateWith_WillAcceptHeteroPartners()
        {
            Guid[] participants = new[] {this.bifemaleHuman.Guid, this.biMaleHuman.Guid};
            IEnumerable<IRelationship> relationships = this.RelationshipHandler.Get(participants);
            Assert.IsTrue(this.bisexual.WillMateWith(this.bifemaleHuman, this.biMaleHuman, relationships));
        }

        [Test]
        public void Asexual_WillMateWith_RejectsPartner()
        {
            Guid[] participants = new Guid[] {this.asexualMaleHuman.Guid, this.bifemaleHuman.Guid};
            IEnumerable<IRelationship> relationships = this.RelationshipHandler.Get(participants);
            Assert.IsFalse(this.asexual.WillMateWith(this.asexualMaleHuman, this.bifemaleHuman, relationships));
        }

        [TearDown]
        public void TearDown()
        {
            GlobalConstants.GameManager = null;
            GlobalConstants.ActionLog = null;
        }
    }
}