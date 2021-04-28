using Godot;
using JoyLib.Code.Cultures;
using JoyLib.Code.Unity.GUI;

namespace JoyLib.Code.States
{
    public class CharacterCreationState : GameState
    {
        //protected CharacterCreationScreen CharacterCreationScreen { get; set; }

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
            PackedScene scene = GD.Load<PackedScene>(
                GlobalConstants.GODOT_ASSETS_FOLDER +
                "Scenes/UI/Character Creation.tscn");
            
            GlobalConstants.GameManager.GUIManager.InstantiateUIScene(scene);
            base.SetUpUi();
            
            ICulture culture = GlobalConstants.GameManager.Roller.SelectFromCollection(GlobalConstants.GameManager.CultureHandler.Values);
            this.GUIManager.SetUIColours(
                culture.BackgroundColours,
                culture.CursorColours,
                culture.FontColours);

            /*
            this.CharacterCreationScreen = this.GUIManager
                .Get(GUINames.CHARACTER_CREATION_PART_1)
                .GetComponent<CharacterCreationScreen>();
            this.CharacterCreationScreen.Initialise();
            */
        }

        public override void HandleInput(InputEvent @event)
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