using JoyLib.Code;
using JoyLib.Code.IO;
using JoyLib.Code.States;
using JoyLib.Code.Unity.GUI;

namespace JoyGodot.Assets.Scripts.GUI.WorldState
{
    public class PauseScreen : GUIData
    {
        protected WorldSerialiser WorldSerialiser { get; set; }

        public override void _Ready()
        {
            base._Ready();

            this.WorldSerialiser = new WorldSerialiser(GlobalConstants.GameManager.ObjectIconHandler);
        }

        public void Settings()
        {
            this.GUIManager.OpenGUI(this, GUINames.SETTINGS);
        }
        
        public void SaveContinue()
        {
            this.WorldSerialiser.Serialise(GlobalConstants.GameManager.Player.MyWorld.GetOverworld());
            this.ButtonClose();
        }

        public void SaveExit()
        {
            this.WorldSerialiser.Serialise(GlobalConstants.GameManager.Player.MyWorld.GetOverworld());
            GlobalConstants.GameManager.SetNextState(new MainMenuState());
        }

        public void ExitNoSave()
        {
            GlobalConstants.GameManager.SetNextState(new MainMenuState());
        }
    }
}