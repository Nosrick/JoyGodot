using System;
using System.Collections.Generic;
using Godot;
using JoyLib.Code.Graphics;

namespace JoyLib.Code.Unity.GUI
{
    public interface IGUIManager : IHandler<GUIData, string>
    {
        void Clear();

        void FindGUIs();

        void ToggleGUI(string name);

        void SetupManagedComponents(GUIData gui, bool crossFade = false, float duration = 0.1f);

        void SetUIColours(IDictionary<string, IDictionary<string, Color>> background,
            IDictionary<string, IDictionary<string, Color>> cursor,
            IDictionary<string, Color> mainFontColours,
            bool recolour = true,
            bool crossFade = false, 
            float duration = 0.1f);

        void RecolourGUIs(bool crossFade = false, float duration = 0.1f);

        GUIData OpenGUI(string name, bool bringToFront = false);

        void CloseGUI(string activeName);

        void BringToFront(string name);

        void CloseAllOtherGUIs(string activeName = "");

        void CloseAllGUIs();

        bool RemovesControl();

        bool RemoveActiveGUI(string name);

        bool IsActive(string name);

        bool AreAnyOpen(bool includeAlwaysOpen = false);

        void InstantiateUIScene(PackedScene ui);
        
        IDictionary<string, Theme> Themes { get; }
        IDictionary<string, ISpriteState> Cursors { get; }

        IDictionary<string, DynamicFont> FontsInUse { get; }
        
        IDictionary<string, IDictionary<string, Color>> CursorColours { get; }
        
        IDictionary<string, IDictionary<string, Color>> UISpriteColours { get; }
        
        IDictionary<string, ISpriteState> UISprites { get; }

        IDictionary<string, Tuple<float, float>> FontSizesInUse { get; }
    }
}