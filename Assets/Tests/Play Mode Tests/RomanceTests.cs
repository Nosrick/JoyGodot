using System;
using System.Collections.Generic;
using JoyGodot.Assets.Scripts;
using JoyGodot.Assets.Scripts.Cultures;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Gender;
using JoyGodot.Assets.Scripts.Entities.Relationships;
using JoyGodot.Assets.Scripts.Entities.Romance;
using JoyGodot.Assets.Scripts.Entities.Sexuality;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Scripting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Moq;
using NUnit.Framework;

namespace JoyGodot.Assets.Tests.Play_Mode_Tests
{
    public class RomanceTests
    {
        private ScriptingEngine scriptingEngine;

        private IEntityRomanceHandler target;
        private IEntityRelationshipHandler RelationshipHandler;
        
        private IEntity heteroMaleHuman;
        private IEntity heterofemaleHuman;

        private IEntity homoMaleHumanLeft;
        private IEntity homoMaleHumanRight;

        private IEntity homofemaleHumanLeft;
        private IEntity homofemaleHumanRight;

        private IEntity biMaleHuman;
        private IEntity bifemaleHuman;

        private IEntity aroMaleHuman;

        private ISexuality asexual;

        private IRomance heteroromantic;
        private IRomance homoromantic;
        private IRomance biromantic;
        private IRomance aromantic;

        [SetUp]
        public void SetUp()
        {
            ActionLog actionLog = new ActionLog();
            GlobalConstants.ActionLog = actionLog;
            GlobalConstants.ScriptingEngine = new ScriptingEngine();

            this.RelationshipHandler = new EntityRelationshipHandler();

            ICultureHandler cultureHandler = Mock.Of<ICultureHandler>(
                handler => handler.Cultures == new ICulture[]
                {
                    Mock.Of<ICulture>(culture => culture.RomanceTypes ==
                                                 new string[]
                                                 {
                                                     "heteroromantic",
                                                     "homoromantic",
                                                     "panromantic",
                                                     "aromantic"
                                                 })
                });

            GlobalConstants.GameManager = Mock.Of<IGameManager>(
                manager => manager.RelationshipHandler == this.RelationshipHandler
                && manager.CultureHandler == cultureHandler);

            this.target = new EntityRomanceHandler();

            this.heteroromantic = this.target.Get("heteroromantic");
            this.homoromantic = this.target.Get("homoromantic");
            this.biromantic = this.target.Get("panromantic");
            this.aromantic = this.target.Get("aromantic");
        }

        [SetUp]
        public void SetUpHumans()
        {
            IGender femaleGender = Mock.Of<IGender>(gender => gender.Name == "female");
            IGender maleGender = Mock.Of<IGender>(gender => gender.Name == "male");

            this.heterofemaleHuman = Mock.Of<IEntity>(
                human => human.Gender == femaleGender
                && human.Romance == this.heteroromantic
                && human.Guid == Guid.NewGuid());

            this.heteroMaleHuman = Mock.Of<IEntity>(
                human => human.Gender == maleGender
                         && human.Romance == this.heteroromantic
                         && human.Guid == Guid.NewGuid());

            this.homoMaleHumanLeft = Mock.Of<IEntity>(
                human => human.Gender == maleGender
                         && human.Romance == this.homoromantic
                         && human.Guid == Guid.NewGuid());

            this.homoMaleHumanRight = Mock.Of<IEntity>(
                human => human.Gender == maleGender
                         && human.Romance == this.homoromantic
                         && human.Guid == Guid.NewGuid());

            this.homofemaleHumanLeft = Mock.Of<IEntity>(
                human => human.Gender == femaleGender
                         && human.Romance == this.homoromantic
                         && human.Guid == Guid.NewGuid());

            this.homofemaleHumanRight = Mock.Of<IEntity>(
                human => human.Gender == femaleGender
                         && human.Romance == this.homoromantic
                         && human.Guid == Guid.NewGuid());

            this.biMaleHuman = Mock.Of<IEntity>(
                human => human.Gender == maleGender
                         && human.Romance == this.biromantic
                         && human.Guid == Guid.NewGuid());

            this.bifemaleHuman = Mock.Of<IEntity>(
                human => human.Gender == femaleGender
                         && human.Romance == this.biromantic
                         && human.Guid == Guid.NewGuid());

            this.aroMaleHuman = Mock.Of<IEntity>(
                human => human.Gender == maleGender
                         && human.Romance == this.aromantic
                         && human.Guid == Guid.NewGuid());


            Guid[] heteroCouple = new[] {this.heterofemaleHuman.Guid, this.heteroMaleHuman.Guid };
            Guid[] homofemaleCouple = new Guid[] {this.homofemaleHumanLeft.Guid, this.homofemaleHumanRight.Guid };
            Guid[] homoMaleCouple = new Guid[] {this.homoMaleHumanLeft.Guid, this.homoMaleHumanRight.Guid };
            Guid[] biCoupleLeft = new Guid[] {this.bifemaleHuman.Guid, this.homofemaleHumanLeft.Guid };
            Guid[] biCoupleRight = new Guid[] {this.bifemaleHuman.Guid, this.biMaleHuman.Guid };
            Guid[] asexualCouple = new Guid[] {this.aroMaleHuman.Guid, this.bifemaleHuman.Guid};

            this.RelationshipHandler.CreateRelationshipWithValue(heteroCouple, new[]{ "monoamorous" }, 500);
            this.RelationshipHandler.CreateRelationshipWithValue(homofemaleCouple, new[]{ "monoamorous" }, 500);
            this.RelationshipHandler.CreateRelationshipWithValue(homoMaleCouple, new[]{ "monoamorous" }, 500);
            this.RelationshipHandler.CreateRelationshipWithValue(biCoupleLeft, new[]{ "monoamorous" }, 500);
            this.RelationshipHandler.CreateRelationshipWithValue(biCoupleRight, new[]{ "monoamorous" }, 500);
            this.RelationshipHandler.CreateRelationshipWithValue(asexualCouple, new[]{ "monoamorous" }, 500);
        }

        [Test]
        public void Heteroromantic_Compatible_AcceptsHeteroPartners()
        {
            Guid[] participants = new [] {this.heterofemaleHuman.Guid, this.heteroMaleHuman.Guid };
            IEnumerable<IRelationship> relationships = this.RelationshipHandler.Get(participants);
            Assert.IsTrue(this.heteroromantic.WillRomance(this.heterofemaleHuman, this.heteroMaleHuman, relationships));
        }

        [Test]
        public void Heteroromantic_Compatible_RejectsHomoPartners()
        {
            Guid[] participants = new [] {this.heterofemaleHuman.Guid, this.homofemaleHumanLeft.Guid };
            IEnumerable<IRelationship> relationships = this.RelationshipHandler.Get(participants);
            Assert.IsFalse(this.heteroromantic.WillRomance(this.heterofemaleHuman, this.homofemaleHumanLeft, relationships));
        }

        [Test]
        public void Homoromantic_Compatible_AcceptsHomoPartners()
        {
            Guid[] participants = new [] {this.homoMaleHumanLeft.Guid, this.homoMaleHumanRight.Guid };
            IEnumerable<IRelationship> relationships = this.RelationshipHandler.Get(participants);
            Assert.IsTrue(this.homoromantic.WillRomance(this.homoMaleHumanLeft, this.homoMaleHumanRight, relationships));
        }

        [Test]
        public void Homoromantic_Compatible_RejectsHeteroPartners()
        {
            Guid[] participants = new[] {this.homofemaleHumanLeft.Guid, this.homofemaleHumanRight.Guid };
            IEnumerable<IRelationship> relationships = this.RelationshipHandler.Get(participants);
            Assert.IsFalse(this.homoromantic.WillRomance(this.homoMaleHumanLeft, this.homofemaleHumanRight, relationships));
        }

        [Test]
        public void Biromantic_Compatible_WillAcceptHomoPartners()
        {
            Guid[] participants = new[] {this.bifemaleHuman.Guid, this.homofemaleHumanLeft.Guid };
            IEnumerable<IRelationship> relationships = this.RelationshipHandler.Get(participants);
            Assert.IsTrue(this.biromantic.WillRomance(this.bifemaleHuman, this.homofemaleHumanLeft, relationships));
        }

        [Test]
        public void Biromantic_Compatible_WillAcceptHeteroPartners()
        {
            Guid[] participants = new[] {this.bifemaleHuman.Guid, this.biMaleHuman.Guid };
            IEnumerable<IRelationship> relationships = this.RelationshipHandler.Get(participants);
            Assert.IsTrue(this.biromantic.WillRomance(this.bifemaleHuman, this.biMaleHuman, relationships));
        }

        [Test]
        public void Aromantic_Compatible_WillRejectPartner()
        {
            Guid[] participants = new[] {this.aroMaleHuman.Guid, this.bifemaleHuman.Guid };
            IEnumerable<IRelationship> relationships = this.RelationshipHandler.Get(participants);
            Assert.IsFalse(this.aromantic.WillRomance(this.aroMaleHuman, this.bifemaleHuman, relationships));
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