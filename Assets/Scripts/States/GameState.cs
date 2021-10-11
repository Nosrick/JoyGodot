using Godot;
using JoyGodot.Assets.Scripts.GUI;

namespace JoyGodot.Assets.Scripts.States
{
    public abstract class GameState : IGameState
    {
        protected GameState()
        {
            this.GUIManager = GlobalConstants.GameManager.GUIManager;
        }

        public abstract void LoadContent();

        public virtual void SetUpUi()
        {
            this.GUIManager.FindGUIs();
        }

        public abstract void Start();

        public virtual void Stop()
        {
            this.GUIManager.Clear();
        }

        public abstract void Update();

        public abstract void HandleInput(InputEvent @event);

        public abstract IGameState GetNextState();

        public bool Done
        {
            get;
            protected set;
        }

        public IGUIManager GUIManager { get; protected set; }
    }
}