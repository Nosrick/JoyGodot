using Godot;
using JoyLib.Code;
using JoyLib.Code.States;
using JoyLib.Code.Unity.GUI;

namespace JoyGodot.Assets.Scripts.States
{
    public class LoadingState : GameState
    {
        public override void LoadContent()
        {
            
        }

        public override void Start()
        {
        }

        public override void Stop()
        {
        }

        public override void SetUpUi()
        {
            GlobalConstants.GameManager.GUIManager.InstantiateUIScene(
                GD.Load<PackedScene>(
                    GlobalConstants.GODOT_ASSETS_FOLDER +
                    "Scenes/UI/LoadingScreen.tscn"));
            base.SetUpUi();

            GlobalConstants.GameManager.GUIManager.OpenGUI(GUINames.LOADING_SCREEN);
        }

        public override void Update()
        {
            if (GlobalConstants.GameManager?.Initialised == true)
            {
                this.Done = true;
            }
        }

        public override void HandleInput(InputEvent @event)
        {
        }

        public override GameState GetNextState()
        {
            //return new CharacterCreationState();
            return new MainMenuState();
        }
    }
}