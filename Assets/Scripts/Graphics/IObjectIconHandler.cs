using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace JoyLib.Code.Graphics
{
    public interface IObjectIconHandler
    {
        bool AddSpriteData(string tileSet, SpriteData dataToAdd);
        bool AddSpriteDataRange(string tileSet, IEnumerable<SpriteData> dataToAdd);
        bool AddSpriteDataFromJson(Dictionary spriteDict);
        SpriteData ReturnDefaultIcon();
        IEnumerable<SpriteData> GetSprites(string tileSet, string tileName, string state = "DEFAULT");
        IEnumerable<SpriteData> GetTileSet(string tileSet);
    }
}