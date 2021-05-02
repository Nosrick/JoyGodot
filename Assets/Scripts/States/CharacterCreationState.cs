using System.Linq;
using Godot;
using JoyLib.Code.Cultures;
using JoyLib.Code.Entities;
using JoyLib.Code.Entities.Jobs;
using JoyLib.Code.Unity.GUI;
using JoyLib.Code.Unity.GUI.CharacterCreationState;

namespace JoyLib.Code.States
{
    public class CharacterCreationState : GameState
    {
        protected CharacterCreationHandler CharacterCreationHandler { get; set; }
        
        protected IEntity Player { get; set; }

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
            
            this.CharacterCreationHandler = this.GUIManager.Get("Character Creation") as CharacterCreationHandler;

            if (this.CharacterCreationHandler is null)
            {
                GD.Print("CHARACTER CREATION NOT FOUND");
                return;
            }
            
            this.CharacterCreationHandler.PlayerCreated += this.PlayerCreated;
        }

        public override void HandleInput(InputEvent @event)
        {
        }

        public override void Update()
        {
        }

        protected void PlayerCreated(IEntity player)
        {
            this.Player = player;
            GlobalConstants.GameManager.EntityHandler.SetPlayer(player);
            player.AddExperience(500);
            foreach (string jobName in player.Cultures.SelectMany(culture => culture.Jobs))
            {
                IJob job = GlobalConstants.GameManager.JobHandler.Get(jobName);
                job.AddExperience(300);
                player.AddJob(job);
            }

            this.Done = true;
        }

        public override GameState GetNextState()
        {
            return new WorldCreationState(this.Player);
        }
    }
}