namespace JoyLib.Code
{
    public interface IPosition
    {
        Vector2Int WorldPosition { get; }

        void Move(Vector2Int position);
    }
}