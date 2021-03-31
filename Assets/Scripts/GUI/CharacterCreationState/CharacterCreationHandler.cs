using Godot;
using JoyLib.Code.Cultures;
using JoyLib.Code.Rollers;

namespace JoyLib.Code.Unity.GUI.CharacterCreationState
{
    public class CharacterCreationHandler : GUIData
    {
        protected LineEdit PlayerName { get; set; }
        protected ICultureHandler CultureHandler { get; set; }
        
        protected IRollable Roller { get; set; }

        public override void _Ready()
        {
            this.PlayerName = this.FindNode("Player Name Input") as LineEdit;

            this.CultureHandler = GlobalConstants.GameManager.CultureHandler;
            this.Roller = GlobalConstants.GameManager.Roller;
        }

        public void RandomiseName()
        {
            this.PlayerName.Text = this.Roller.SelectFromCollection(
                this.CultureHandler.Values)
                .GetRandomName("male");
        }
    }
}