using System.Linq;
using Godot;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.GUI;
using JoyGodot.Assets.Scripts.Quests;

namespace JoyGodot.Assets.Scripts.Godot
{
    public class CheatInterface : GUIData
    {
        protected IEntity Player { get; set; }
        
        protected int Pressed { get; set; }

        public override void _Ready()
        {
            base._Ready();
            
            this.Player = GlobalConstants.GameManager.Player;
        }

        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("tooltip_show"))
            {
                this.Pressed += 1;

                if (this.Pressed >= 3)
                {
                    this.Show();
                }
            }
            else if(@event.IsActionPressed("close all windows"))
            {
                this.Pressed = 0;
                this.Hide();
            }
        }

        public void FillNeeds()
        {
            if (this.Player is null)
            {
                return;
            }

            foreach (var need in this.Player.Needs.Values)
            {
                need.SetValue(need.HappinessThreshold);
            }

            this.Player.HappinessIsDirty = true;
        }

        public void EmptyNeeds()
        {
            if (this.Player is null)
            {
                return;
            }

            foreach (var need in this.Player.Needs.Values)
            {
                need.SetValue(0);
            }

            this.Player.HappinessIsDirty = true;
        }

        public void EmptyOneNeed()
        {
            if (this.Player is null)
            {
                return;
            }
            
            var need = this.Player.Needs.Values
                .OrderByDescending(n => n.Value)
                .First();
            need.SetValue(0);
            
            this.Player.HappinessIsDirty = true;
        }

        public void AddQuest()
        {
            if (this.Player is null)
            {
                return;
            }

            var random = this.Player.MyWorld.GetRandomSentient();

            IQuest quest = GlobalConstants.GameManager.QuestProvider.MakeRandomQuest(
                this.Player, 
                random, 
                this.Player.MyWorld);

            GlobalConstants.GameManager.QuestTracker.AddQuest(this.Player.Guid, quest);
        }

        public void CompleteQuest()
        {
            if (this.Player is null)
            {
                return;
            }

            var quest = GlobalConstants.GameManager.QuestTracker
                .GetQuestsForEntity(this.Player.Guid)
                .FirstOrDefault();

            if (quest is null)
            {
                return;
            }
            
            GlobalConstants.GameManager.QuestTracker.CompleteQuest(this.Player, quest, true);
        }
    }
}