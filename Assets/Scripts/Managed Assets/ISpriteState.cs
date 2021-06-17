using System.Collections.Generic;
using Godot;
using JoyGodot.Assets.Scripts.Base_Interfaces;

namespace JoyGodot.Assets.Scripts.Managed_Assets
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