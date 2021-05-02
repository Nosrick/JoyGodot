using System.Collections.Generic;
using Godot;
using Godot.Collections;
using JoyGodot.Assets.Scripts.GUI.Managed_Assets;

namespace JoyLib.Code.Unity.GUI
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
        DynamicFont CustomFont { get; set; }
        Color FontColour { get; set; }
        int FontSize { get; set; }
        Color OutlineColour { get; set; }
        int OutlineThickness { get; set; }
        int FontMinSize { get; set; }
        int FontMaxSize { get; set; }
    }
}