using Godot;
using JoyLib.Code.States;

namespace Joy.Code.Managers
{
    public interface IStateManager
    {
        void ChangeState(IGameState newState);
        void LoadContent();
        void Start();
        void Stop();
        void Update();
        void NextState();
        void Process(InputEvent @event);
    }
}