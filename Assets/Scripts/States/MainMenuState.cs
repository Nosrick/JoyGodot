using Godot;
using JoyGodot.Assets.Scripts.GUI;
using JoyGodot.Assets.Scripts.IO;
using JoyGodot.Assets.Scripts.World;

namespace JoyGodot.Assets.Scripts.States
{
    class MainMenuState : GameState
    {
        protected GameState m_NextState;

        protected WorldSerialiser m_WorldSerialiser = new WorldSerialiser(GlobalConstants.GameManager.ObjectIconHandler);

        public MainMenuState() :
            base()
        {
        }

        public override void LoadContent()
        {
        }

        public override void SetUpUi()
        {
            this.GUIManager.InstantiateUIScene(
                GD.Load<PackedScene>(
                    GlobalConstants.GODOT_ASSETS_FOLDER +
                    "Scenes/UI/MainMenu.tscn"));
            this.GUIManager.FindGUIs();
            this.GUIManager.OpenGUI(this, GUINames.MAIN_MENU);
        }

        public override void Start()
        {
        }

        public override void Update()
        {
        }

        public override void HandleInput(InputEvent @event)
        {
        }

        public void NewGame()
        {
            this.Done = true;
            this.m_NextState = new CharacterCreationState();
        }

        public void ContinueGame()
        {
            IWorldInstance overworld = this.m_WorldSerialiser.Deserialise("Everse");
            this.Done = true;

            IWorldInstance playerWorld = GlobalConstants.GameManager.EntityHandler.GetPlayer().MyWorld;
            this.m_NextState = new WorldInitialisationState(overworld, playerWorld);
        }

        public override IGameState GetNextState()
        {
            return this.m_NextState;
        }
    }
}