using Godot;
using JoyLib.Code;
using JoyLib.Code.States;
using JoyLib.Code.World;

namespace Code.States
{
    public class GameOverState : GameState
    {
        protected IWorldInstance Overworld { get; set; } 
        
        public GameOverState(IWorldInstance overworld)
        {
            this.Overworld = overworld;
        }
        
        public override void LoadContent()
        {
        }

        public override void Start()
        {
            IGameManager gameManager = GlobalConstants.GameManager;
            gameManager.EntityPool.RetireAll();
            gameManager.ItemPool.RetireAll();
            gameManager.FogPool.RetireAll();
            gameManager.FloorPool.RetireAll();
            gameManager.WallPool.RetireAll();
            
            
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

        public override GameState GetNextState()
        {
            return new MainMenuState();
        }
    }
}