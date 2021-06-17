using JoyGodot.Assets.Scripts.IO;
using JoyGodot.Assets.Scripts.States;
using JoyGodot.Assets.Scripts.World;

namespace JoyGodot.Assets.Scripts.GUI.MainMenuState
{
    public class MainMenuHandler : GUIData
    {
        public override void _Ready()
        {
            GlobalConstants.GameManager.GUIManager.SetupManagedComponents(this);
        }

        public void NewGame()
        {
            GlobalConstants.GameManager.GUIManager.CloseAllGUIs();
            GlobalConstants.GameManager.SetNextState(new States.CharacterCreationState());
        }

        public void LoadGame()
        {
            WorldSerialiser worldSerialiser = new WorldSerialiser(GlobalConstants.GameManager.ObjectIconHandler);
            IWorldInstance overworld = worldSerialiser.Deserialise("Everse");
            GlobalConstants.GameManager.GUIManager.CloseAllGUIs();
            GlobalConstants.GameManager.SetNextState(new WorldInitialisationState(overworld, overworld.GetPlayerWorld(overworld)));
        }

        public void Settings()
        {
            GlobalConstants.GameManager.GUIManager.OpenGUI(this, GUINames.SETTINGS);
        }

        public void QuitGame()
        {
            this.GetTree().Quit(0);
        }
    }
}