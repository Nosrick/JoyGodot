﻿using System;
using System.Collections.Generic;
using Godot;
using JoyGodot.Assets.Scripts.Base_Interfaces;
using JoyGodot.Assets.Scripts.Managed_Assets;

namespace JoyGodot.Assets.Scripts.GUI
{
    public interface IGUIManager : IHandler<GUIData, string>
    {
        ManagedCursor Cursor { get;}
        
        Tooltip Tooltip { get; }
        
        ContextMenu ContextMenu { get; }
        
        void Clear();

        void FindGUIs();

        void ToggleGUI(object sender, string name);

        T Get<T>(string name) where T : GUIData;

        void SetupManagedComponents(Control gui, bool crossFade = false, float duration = 0.1F);
        void SetUpManagedComponent(IManagedElement element, bool crossFade = false, float duration = 0.1f);

        void SetUIColours(IDictionary<string, IDictionary<string, Color>> background,
            IDictionary<string, IDictionary<string, Color>> cursor,
            IDictionary<string, Color> mainFontColours,
            bool recolour = true,
            bool crossFade = false, 
            float duration = 0.1f);

        void RecolourGUIs(bool crossFade = false, float duration = 0.1f);

        GUIData OpenGUI(object sender, string name, bool bringToFront = false);

        void CloseGUI(object sender, string activeName);

        void BringToFront(string name);

        void CloseAllOtherGUIs(string activeName = "");

        void CloseAllGUIs();

        bool RemovesControl();

        bool RemoveActiveGUI(string name, bool close = false, bool disable = false);

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