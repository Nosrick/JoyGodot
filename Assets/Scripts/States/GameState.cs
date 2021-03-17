using Godot;
using JoyLib.Code.Unity.GUI;

namespace JoyLib.Code.States
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
            //Thread.Sleep(200);

            this.GUIManager.Clear();
            this.GUIManager.FindGUIs();

            this.GUIManager.CloseAllOtherGUIs(GUINames.CURSOR);
        }

        public abstract void Start();

        public abstract void Stop();

        //ALWAYS call base.Update() from derived classes
        public abstract void Update();

        public abstract void HandleInput(InputEvent @event);

        public abstract GameState GetNextState();

        public bool Done
        {
            get;
            protected set;
        }

        public IGUIManager GUIManager { get; protected set; }
    }
}