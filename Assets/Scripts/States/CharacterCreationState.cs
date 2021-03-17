using JoyLib.Code.Unity.GUI;
using UnityEngine.InputSystem;

namespace JoyLib.Code.States
{
    public class CharacterCreationState : GameState
    {
        protected CharacterCreationScreen CharacterCreationScreen { get; set; }

        public CharacterCreationState()
        {
        }

        public override void Start()
        {
        }

        public override void Stop()
        {
        }

        public override void LoadContent()
        {
        }

        public override void SetUpUi()
        {
            base.SetUpUi();

            this.CharacterCreationScreen = this.GUIManager
                .Get(GUINames.CHARACTER_CREATION_PART_1)
                .GetComponent<CharacterCreationScreen>();
            this.CharacterCreationScreen.Initialise();
            this.GUIManager.OpenGUI(GUINames.CHARACTER_CREATION_PART_1, true);
        }

        public override void HandleInput(object data, InputActionChange action)
        {
        }

        public override void Update()
        {
        }

        public override GameState GetNextState()
        {
            /*
            IEntity player = this.CharacterCreationScreen.CreatePlayer();
            GlobalConstants.GameManager.Player = player;
            player.AddExperience(500);
            foreach (string jobName in player.Cultures.SelectMany(culture => culture.Jobs))
            {
                IJob job = GlobalConstants.GameManager.JobHandler.Get(jobName);
                job.AddExperience(300);
                player.AddJob(job);
            }
            
            return new WorldCreationState(player);
            */
            return null;
        }
    }
}