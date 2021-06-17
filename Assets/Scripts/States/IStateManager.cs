using Godot;

namespace JoyGodot.Assets.Scripts.States
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