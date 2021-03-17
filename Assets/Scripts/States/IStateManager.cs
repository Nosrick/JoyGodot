using JoyLib.Code.States;
using UnityEngine.InputSystem;

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
        void OnMove(object data, InputActionChange change);
    }
}