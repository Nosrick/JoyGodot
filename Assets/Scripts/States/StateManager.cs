using Godot;
using JoyLib.Code.States;

namespace Joy.Code.Managers
{
    public class StateManager : IStateManager
    {
        protected IGameState m_ActiveState;
        
        protected bool Active { get; set; }

        public StateManager()
        {
        }

        public void ChangeState(IGameState newState)
        {
            this.Active = false;
            this.m_ActiveState?.Stop();
            this.m_ActiveState = null;
            this.m_ActiveState = newState;
            this.Active = true;
            this.m_ActiveState.Start();
            this.m_ActiveState.LoadContent();
            this.m_ActiveState.SetUpUi();
        }

        public void LoadContent()
        {
            this.m_ActiveState.LoadContent();
        }

        public void Start()
        {
            this.Active = true;
            this.m_ActiveState = new MainMenuState();
            this.m_ActiveState.Start();
        }

        public void Update()
        {
            if (this.m_ActiveState is null || this.Active == false)
            {
                return;
            }
            
            this.m_ActiveState.Update();

            if(this.m_ActiveState.Done)
            {
                this.ChangeState(this.m_ActiveState.GetNextState());
            }
        }

        public void Process(InputEvent @event)
        {
            if (this.Active == false)
            {
                return;
            }
            this.m_ActiveState?.HandleInput(@event);
        }

        public void NextState()
        {
            IGameState newState = this.m_ActiveState.GetNextState();
            this.ChangeState(newState);
        }

        public void Stop()
        {
            this.Active = false;
            this.m_ActiveState?.Stop();
        }
    }
}