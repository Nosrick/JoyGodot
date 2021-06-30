using Godot;
using JoyGodot.Assets.Scripts.World;

namespace JoyGodot.Assets.Scripts.States
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
            gameManager.FloorTileMap.Clear();
            gameManager.WallTileMap.Clear();
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