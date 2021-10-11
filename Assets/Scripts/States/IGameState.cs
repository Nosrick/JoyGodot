using Godot;
using JoyGodot.Assets.Scripts.GUI;

namespace JoyGodot.Assets.Scripts.States
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
        IGameState GetNextState();
    }
}