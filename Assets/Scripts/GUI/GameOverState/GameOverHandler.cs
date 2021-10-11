namespace JoyGodot.Assets.Scripts.GUI.GameOverState
{
    public class GameOverHandler : GUIData
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

        public void QuitToMenu()
        {
            GlobalConstants.GameManager.GUIManager.CloseAllGUIs();
            GlobalConstants.GameManager.SetNextState(new States.MainMenuState());
        }
    }
}