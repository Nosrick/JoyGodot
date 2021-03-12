namespace JoyLib.Code.Graphics
{
    public interface IAnimated : ISpriteStateContainer
    {
        ISpriteState CurrentSpriteState { get; }
        int FrameIndex { get; }
        string ChosenSprite { get; }
        string TileSet { get; }
        float TimeSinceLastChange { get; }
        bool Finished { get; }
    }
}