using System.Linq;
using Godot;
using JoyLib.Code.Cultures;
using JoyLib.Code.IO;
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
            ICulture[] cultures = GlobalConstants.GameManager.CultureHandler.Cultures.ToArray();
            int result = GlobalConstants.GameManager.Roller.Roll(0, cultures.Length);
            ICulture randomCulture = cultures[result];
            this.GUIManager.SetUIColours(
                randomCulture.BackgroundColours,
                randomCulture.CursorColours,
                randomCulture.FontColours,
                false);
            base.SetUpUi();
            this.GUIManager.RecolourGUIs();
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

        private void NewGame()
        {
            this.Done = true;
            this.m_NextState = new CharacterCreationState();
        }

        private void ContinueGame()
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