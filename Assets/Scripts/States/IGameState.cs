using Godot;
using JoyLib.Code.Unity.GUI;

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
        void HandleInput(InputEvent @event);
        GameState GetNextState();
    }
}