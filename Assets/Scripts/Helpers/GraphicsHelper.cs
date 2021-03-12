using Godot;

namespace JoyLib.Code.Helpers
{
    public static class GraphicsHelper
    {
        public static Color LerpColour(Color left, Color right, float t)
        {
            return left + (right - left) * t;
        }
    }
}