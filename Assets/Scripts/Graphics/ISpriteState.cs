using System;
using System.Collections.Generic;
using Godot;

namespace JoyLib.Code.Graphics
{
    public interface ISpriteState : IDisposable
    {
        string Name { get; }

        IList<Tuple<Color, Texture>> GetSpriteForFrame(int frame);
        
        SpriteData SpriteData { get; }

        void RandomiseColours();
        void SetColourIndices(List<int> indices);
        void OverrideColours(IDictionary<string, Color> colours);
        void OverrideWithSingleColour(Color colour);

        List<int> GetIndices();
        
        AnimationType AnimationType { get; }
        bool Looping { get; }
        
        bool IsAnimated { get; set; }
    }

    public enum AnimationType
    {
        Forward,
        Reverse,
        PingPong
    }
}