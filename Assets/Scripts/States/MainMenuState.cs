using System.Linq;
using Godot;
using JoyLib.Code.Cultures;
using JoyLib.Code.IO;
using JoyLib.Code.Unity.GUI;
using JoyLib.Code.World;

namespace JoyLib.Code.States
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
            GlobalConstants.GameManager.GUIManager.InstantiateUIScene(
                GD.Load<PackedScene>(
                    GlobalConstants.GODOT_ASSETS_FOLDER +
                    "Scenes/UI/MainMenu.tscn"));
            base.SetUpUi();
            this.GUIManager.OpenGUI(GUINames.MAIN_MENU);
        }

        public override void Start()
        {
        }

        public override void Stop()
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

            IWorldInstance playerWorld = overworld.Player.MyWorld;
            this.m_NextState = new WorldState(overworld, playerWorld);
        }

        public override GameState GetNextState()
        {
            return this.m_NextState;
        }
    }
}