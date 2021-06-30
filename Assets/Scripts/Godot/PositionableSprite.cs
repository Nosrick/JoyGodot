using Godot;
using JoyGodot.Assets.Scripts.Base_Interfaces;
using JoyGodot.Assets.Scripts.JoyObject;

namespace JoyGodot.Assets.Scripts.Godot
{
    public class PositionableSprite : Sprite, IPosition
    {
        public Vector2Int WorldPosition { get; protected set; }
        
        public void Move(Vector2Int position)
        {
            this.WorldPosition = position;
            this.Position = (position * GlobalConstants.SPRITE_WORLD_SIZE).ToVec2();
        }
    }
}