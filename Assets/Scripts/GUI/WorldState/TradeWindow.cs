﻿using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Godot;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Relationships;
using JoyGodot.Assets.Scripts.GUI.Inventory_System;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.Items;
using JoyGodot.Assets.Scripts.JoyObject;

namespace JoyGodot.Assets.Scripts.GUI.WorldState
{
    public class TradeWindow : GUIData
    {
        public IItemContainer Left
        {
            get => this.m_Left;
            set => this.m_Left = value;
        }

        protected IItemContainer m_Left;
        public IItemContainer Right
        {
            get => this.m_Right;
            set => this.m_Right = value;
        }

        protected IItemContainer m_Right;

        protected Label LeftValue { get; set; }
        protected Label RightValue { get; set; }

        protected ItemContainer LeftInventory { get; set; }
        protected ItemContainer LeftOffering { get; set; }

        protected ItemContainer RightInventory { get; set; }
        protected ItemContainer RightOffering { get; set; }

        public IEntityRelationshipHandler RelationshipHandler { get; set; }

        public override void _Ready()
        {
            base._Ready();

            this.LeftValue = this.FindNode("LeftValue") as Label;
            this.RightValue = this.FindNode("RightValue") as Label;
            
            this.LeftInventory = this.FindNode("LeftInventory") as ItemContainer;
            this.LeftOffering = this.FindNode("LeftOffering") as ItemContainer;
            
            this.RightInventory = this.FindNode("RightInventory") as ItemContainer;
            this.RightOffering = this.FindNode("RightOffering") as ItemContainer;

            this.RelationshipHandler = GlobalConstants.GameManager.RelationshipHandler;
        }

        public override void DisconnectEvents()
        {
            base.DisconnectEvents();
            
            this.LeftOffering.ContainerOwner.ItemAdded -= this.Tally;
            this.LeftOffering.ContainerOwner.ItemRemoved -= this.Tally;
            this.RightOffering.ContainerOwner.ItemAdded -= this.Tally;
            this.RightOffering.ContainerOwner.ItemRemoved -= this.Tally;
        }

        public override bool Close(object sender)
        {
            this.LeftInventory.ContainerOwner.AddContents(this.LeftOffering.Contents);
            this.LeftOffering.RemoveAllItems();

            this.RightInventory.ContainerOwner.AddContents(this.RightOffering.Contents);
            this.RightOffering.RemoveAllItems();
            
            GlobalConstants.GameManager.GUIManager.CloseGUI(this, GUINames.TOOLTIP);
            return base.Close(sender);
        }

        public void SetActors(IItemContainer left, IItemContainer right)
        {
            this.Left = left;
            this.Right = right;
            
            this.SetActors();
        }

        protected void SetActors()
        {
            this.LeftInventory.ContainerOwner = this.Left;
            this.LeftOffering.ContainerOwner = new VirtualStorage();

            for (int i = 0; i < 8 - this.LeftOffering.NumberOfSlots; i++)
            {
                this.LeftOffering.AddSlot(true);
            }
            this.LeftOffering.TitleText = "You are offering";

            this.RightInventory.ContainerOwner = this.Right;
            this.RightOffering.ContainerOwner = new VirtualStorage();

            for (int i = 0; i < 8 - this.RightOffering.NumberOfSlots; i++)
            {
                this.RightOffering.AddSlot(true);
            }

            if (this.Right is IEntity right)
            {
                this.RightOffering.TitleText = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(
                    right.Gender.PersonalSubject) +
                    " " + right.Gender.IsOrAre +
                         " offering";
            }
            else
            {
                this.RightOffering.TitleText = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(this.Right.JoyName + "'s offering");
            }
            
            this.RightInventory.TitleText = this.Right.JoyName;
            
            this.LeftOffering.ContainerOwner.ItemAdded -= this.Tally;
            this.LeftOffering.ContainerOwner.ItemRemoved -= this.Tally;
            this.RightOffering.ContainerOwner.ItemAdded -= this.Tally;
            this.RightOffering.ContainerOwner.ItemRemoved -= this.Tally;

            this.LeftOffering.ContainerOwner.ItemAdded += this.Tally;
            this.LeftOffering.ContainerOwner.ItemRemoved += this.Tally;
            this.RightOffering.ContainerOwner.ItemAdded += this.Tally;
            this.RightOffering.ContainerOwner.ItemRemoved += this.Tally;

            this.Tally();
        }

        protected bool Trade()
        {
            int leftValue = this.LeftOffering.Contents.Sum(item => item.Value);

            int rightValue = this.RightOffering.Contents.Sum(item => item.Value);

            int relationshipValue = 0;
            relationshipValue = this.RelationshipHandler.GetHighestRelationshipValue(this.Left.Guid, this.Right.Guid);
            
            int difference = leftValue - rightValue;
            IEnumerable<IRelationship> relationships =
                this.RelationshipHandler?.Get(new[] {this.Left.Guid, this.Right.Guid});
            foreach (IRelationship relationship in relationships)
            {
                relationship.ModifyValueOfParticipant(this.Left.Guid, this.Right.Guid, difference);
            }

            if (!(leftValue + relationshipValue >= rightValue))
            {
                return false;
            }

            if (this.Left is IJoyObject leftObject && this.Right is IJoyObject rightObject)
            {
                GlobalConstants.ScriptingEngine.FetchAction("tradeaction").Execute(
                    new[] {leftObject, rightObject},
                    new[] {"trade", "give", "item"},
                    new Dictionary<string, object>
                    {
                        {"leftOffering", this.LeftOffering.Contents},
                        {"rightOffering", this.RightOffering.Contents}
                    });
            }

            this.LeftOffering.RemoveAllItems();
            this.RightOffering.RemoveAllItems();

            this.LeftInventory.ContainerOwner = this.Left;
            this.LeftInventory.OnEnable();

            this.RightInventory.ContainerOwner = this.Right;
            this.RightInventory.OnEnable();

            this.Tally();

            return true;
        }

        public void TradeButton()
        {
            bool result = this.Trade();
            if (!result)
            {
                GlobalConstants.ActionLog.Log(
                    "Trade failed between " + 
                    this.Left.JoyName + 
                    " and " + 
                    this.Right.JoyName, 
                    LogLevel.Warning);
            }
        }

        protected void Tally()
        {
            int leftValue = this.LeftOffering.Contents.Sum(item => item.Value);

            int rightValue = this.RightOffering.Contents.Sum(item => item.Value);

            this.LeftValue.Text = "Your value: " + leftValue;
            this.RightValue.Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(this.Right.JoyName) +
                                   "'s value: " + rightValue;
        }

        protected bool Tally(IItemContainer container, IItemInstance itemInstance)
        {
            this.Tally();
            return true;
        }
    }
}