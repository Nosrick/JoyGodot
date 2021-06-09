using Godot;

namespace JoyLib.Code.Unity.GUI.MainMenuState
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