using Godot;

namespace JoyLib.Code.Godot
{
    public class PositionableSprite : Sprite, IPosition
    {
        public Vector2Int WorldPosition { get; protected set; }
        
        public void Move(Vector2Int position)
        {
            this.WorldPosition = position;
            this.Position = (position * GlobalConstants.SPRITE_WORLD_SIZE).ToVec2;
        }
    }
}