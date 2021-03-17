using JoyLib.Code.World;
using UnityEngine.InputSystem;

namespace JoyLib.Code.States
{
    public class WorldDestructionState : GameState
    {
        protected IWorldInstance m_OverWorld;
        protected IWorldInstance m_NextWorld;

        public WorldDestructionState(IWorldInstance overworld, IWorldInstance nextWorld)
        {
            this.m_OverWorld = overworld;
            this.m_NextWorld = nextWorld;
        }

        public override void LoadContent()
        {
        }

        public override void Start()
        {
            this.DestroyWorld();
        }

        public override void SetUpUi()
        {
        }

        public override void Stop()
        {
        }

        public override void Update()
        {
        }

        public override void HandleInput(object data, InputActionChange action)
        {
        }

        protected void DestroyWorld()
        {
            IGameManager gameManager = GlobalConstants.GameManager;
            gameManager.EntityPool.RetireAll();
            gameManager.ItemPool.RetireAll();
            gameManager.FogPool.RetireAll();
            gameManager.FloorPool.RetireAll();
            gameManager.WallPool.RetireAll();

            this.Done = true;
        }

        public override GameState GetNextState()
        {
            return new WorldInitialisationState(this.m_OverWorld, this.m_NextWorld);
        }
    }
}
