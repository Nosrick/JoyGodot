using Godot;
using JoyGodot.Assets.Scripts.Cultures;
using JoyGodot.Assets.Scripts.GUI;

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

        public override void SetUpUi()
        {
            GlobalConstants.GameManager.GUIManager.InstantiateUIScene(
                GD.Load<PackedScene>(
                    GlobalConstants.GODOT_ASSETS_FOLDER +
                    "Scenes/UI/LoadingScreen.tscn"));
            base.SetUpUi();

            GlobalConstants.GameManager.GUIManager.OpenGUI(this, GUINames.LOADING_SCREEN);
        }

        public override void Update()
        {
            if (GlobalConstants.GameManager?.Initialised == true)
            {
                this.Done = true;
                
                ICulture randomCulture = GlobalConstants.GameManager.Roller.SelectFromCollection(
                    GlobalConstants.GameManager.CultureHandler.Values);
                this.GUIManager.SetUIColours(
                    randomCulture.BackgroundColours,
                    randomCulture.CursorColours,
                    randomCulture.FontColours);
            }
        }

        public override void HandleInput(InputEvent @event)
        {
        }

        public override IGameState GetNextState()
        {
            //return new CharacterCreationState();
            return new MainMenuState();
        }
    }
}