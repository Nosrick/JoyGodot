using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Godot;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Items;
using JoyGodot.Assets.Scripts.Entities.Relationships;
using JoyGodot.Assets.Scripts.Events;
using JoyGodot.Assets.Scripts.GUI.Inventory_System;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Scripting;

namespace JoyGodot.Assets.Scripts.GUI.WorldState
{
    public class TradeWindow : GUIData
    {
        public IEntity Left { get; protected set; }
        public IEntity Right { get; protected set; }

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
            
            this.LeftOffering.OnAddItem -= this.Tally;
            this.LeftOffering.OnRemoveItem -= this.Tally;
            this.RightOffering.OnAddItem -= this.Tally;
            this.RightOffering.OnRemoveItem -= this.Tally;

            this.LeftOffering.OnAddItem += this.Tally;
            this.LeftOffering.OnRemoveItem += this.Tally;
            this.RightOffering.OnAddItem += this.Tally;
            this.RightOffering.OnRemoveItem += this.Tally;

            this.RelationshipHandler = GlobalConstants.GameManager.RelationshipHandler;
        }

        public override bool Close(object sender)
        {
            foreach (IItemInstance item in this.LeftOffering.Contents)
            {
                this.LeftInventory.StackOrAdd(item);
            }

            this.LeftOffering.RemoveAllItems();

            foreach (IItemInstance item in this.RightOffering.Contents)
            {
                this.RightOffering.StackOrAdd(item);
            }

            this.RightOffering.RemoveAllItems();
            
            GlobalConstants.GameManager.GUIManager.CloseGUI(this, GUINames.TOOLTIP);
            return base.Close(sender);
        }

        public void SetActors(IEntity left, IEntity right)
        {
            this.Left = left;
            this.Right = right;

            this.LeftInventory.JoyObjectOwner = this.Left;
            this.LeftOffering.JoyObjectOwner = new VirtualStorage();

            this.RightInventory.JoyObjectOwner = this.Right;
            this.RightOffering.JoyObjectOwner = new VirtualStorage();

            this.RightOffering.TitleText = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(this.Right.Gender.PersonalSubject) +
                                  " " + this.Right.Gender.IsOrAre + " offering";
            this.RightInventory.TitleText = this.Right.JoyName;

            this.Tally();
        }

        protected bool Trade()
        {
            int leftValue = this.LeftOffering.Contents.Sum(item => item.Value);

            int rightValue = this.RightOffering.Contents.Sum(item => item.Value);

            int? relationshipValue = this.RelationshipHandler?.GetHighestRelationshipValue(this.Right, this.Left);

            if (relationshipValue is null)
            {
                return false;
            }

            if (!(leftValue + relationshipValue >= rightValue))
            {
                return false;
            }

            int difference = leftValue - rightValue;

            IEnumerable<IRelationship> relationships =
                this.RelationshipHandler?.Get(new IJoyObject[] {this.Left, this.Right});
            foreach (IRelationship relationship in relationships)
            {
                relationship.ModifyValueOfParticipant(this.Left.Guid, this.Right.Guid, difference);
            }

            ScriptingEngine.Instance.FetchAction("tradeaction").Execute(
                new IJoyObject[] {this.Left, this.Right},
                new[] {"trade", "give", "item"},
                new Dictionary<string, object>
                {
                    {"leftOffering", this.LeftOffering.Contents},
                    {"rightOffering", this.RightOffering.Contents}
                });

            this.LeftOffering.RemoveAllItems();
            this.RightOffering.RemoveAllItems();

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
            this.RightValue.Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(this.Right.Gender.Possessive) +
                                   " value: " + rightValue;
        }

        protected void Tally(IItemContainer container, ItemChangedEventArgs args)
        {
            this.Tally();
        }
    }
}