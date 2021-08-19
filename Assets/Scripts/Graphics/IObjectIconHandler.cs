using System.Collections.Generic;
using Godot;
using Godot.Collections;
using JoyGodot.Assets.Scripts.Managed_Assets;

namespace JoyGodot.Assets.Scripts.Graphics
{
    public interface IObjectIconHandler
    {
        bool AddSpriteData(string tileSet, SpriteData dataToAdd);
        bool AddSpriteDataRange(string tileSet, IEnumerable<SpriteData> dataToAdd);
        bool AddSpriteDataFromJson(Dictionary spriteDict);
        IEnumerable<SpriteData> GetManagedSprites(string tileSet, string tileName, string state = "DEFAULT");
        IEnumerable<SpriteData> GetSpritesForManagedAssets(string tileSet);
        TileSet GetStaticTileSet(string tileSet, bool addStairs = false);
        SpriteData GetStaticSpriteData(string tileSet);
        ShaderMaterial TileSetMaterial { get; }
        ShaderMaterial JoyMaterial { get; }
        ShaderMaterial UiMaterial { get; }
    }
}