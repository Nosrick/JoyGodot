using JoyGodot.Assets.Scripts.JoyObject;

namespace JoyGodot.Assets.Scripts.Base_Interfaces
{
    public interface IPosition
    {
        Vector2Int WorldPosition { get; }

        void Move(Vector2Int position);
    }
}