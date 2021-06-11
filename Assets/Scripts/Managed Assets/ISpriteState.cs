using System;
using System.Collections.Generic;
using Godot;
using JoyGodot.addons.Managed_Assets;

namespace JoyLib.Code.Graphics
{
    public interface ISpriteState : ISerialisationHandler
    {
        string Name { get; }

        SpritePart GetPart(string name);
        
        SpriteData SpriteData { get; }

        void RandomiseColours();
        void OverrideColours(IDictionary<string, Color> colours, bool overwrite = false);
        void OverrideWithSingleColour(Color colour);
        
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