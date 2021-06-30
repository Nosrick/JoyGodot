using Godot;

namespace JoyGodot.Assets.Scripts.Managed_Assets
{
    public interface IManagedFonts : IColourableElement
    {
        bool CacheFont { get; set; }
        bool AutoSize { get; set; }
        bool OverrideSize { get; set; }
        bool OverrideColour { get; set; }
        bool OverrideOutline { get; set; }
        bool HasFont { get; }
        bool HasFontColours { get; }
        Color FontColour { get; set; }
        int FontSize { get; set; }
        Color OutlineColour { get; set; }
        int OutlineThickness { get; set; }
        int FontMinSize { get; set; }
        int FontMaxSize { get; set; }
    }
}