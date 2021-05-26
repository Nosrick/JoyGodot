using System.Collections.Generic;
using Godot;
using Godot.Collections;
using JoyGodot.addons.Managed_Assets;

namespace JoyLib.Code.Graphics
{
    public interface IObjectIconHandler
    {
        bool AddSpriteData(string tileSet, SpriteData dataToAdd, bool isTileSet);
        bool AddSpriteDataRange(string tileSet, IEnumerable<SpriteData> dataToAdd, bool isTileSet);
        bool AddSpriteDataFromJson(Dictionary spriteDict);
        IEnumerable<SpriteData> GetManagedSprites(string tileSet, string tileName, string state = "DEFAULT");
        IEnumerable<SpriteData> GetSpritesForManagedAssets(string tileSet);
        TileSet GetStaticTileSet(string tileSet, bool addStairs = false);

        SpriteData GetStaticSpriteData(string tileSet);
    }
}