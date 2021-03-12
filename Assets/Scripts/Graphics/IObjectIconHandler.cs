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
        IEnumerable<SpriteData> ReturnDefaultData();
        SpriteData ReturnDefaultIcon();
        IEnumerable<SpriteData> GetTileSet(string tileSet);
        IEnumerable<SpriteData> GetSprites(string tileSet, string tileName, string state = "DEFAULT");
        SpriteData GetFrame(string tileSet, string tileName, string state = "DEFAULT", int frame = 0);
        List<Texture> GetRawFrames(string tileSet, string tileName, string partName, string state = "DEFAULT");
    }
}