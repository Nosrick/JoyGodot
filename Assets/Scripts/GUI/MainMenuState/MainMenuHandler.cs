using Godot;
using JoyLib.Code.States;

namespace JoyLib.Code.Unity.GUI.MainMenuState
{
    public class MainMenuHandler : Control
    {
        public void NewGame()
        {
            GlobalConstants.GameManager.SetNextState(new CharacterCreationState());
        }
    }
}