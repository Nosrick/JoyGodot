using JoyLib.Code.Unity.GUI;
using UnityEngine.InputSystem;

namespace JoyLib.Code.States
{
    public interface IGameState
    {
        bool Done { get; }
        IGUIManager GUIManager { get; }
        void LoadContent();
        void SetUpUi();
        void Start();
        void Stop();
        void Update();
        void HandleInput(object data, InputActionChange change);
        GameState GetNextState();
    }
}