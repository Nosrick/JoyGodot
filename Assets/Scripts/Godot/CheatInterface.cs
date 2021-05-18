using System.Linq;
using Godot;
using JoyLib.Code.Entities;
using JoyLib.Code.Helpers;
using JoyLib.Code.Unity.GUI;

namespace JoyLib.Code.Godot
{
    public class CheatInterface : GUIData
    {
        protected IEntity Player { get; set; }

        public override void _Ready()
        {
            base._Ready();
            
            this.Player = GlobalConstants.GameManager.Player;
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
    }
}