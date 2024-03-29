﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Godot;
using Godot.Collections;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.Managed_Assets;
using Array = Godot.Collections.Array;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace JoyGodot.Assets.Scripts.GUI
{
    public class GUIManager : IGUIManager
    {
        protected HashSet<GUIData> GUIs { get; set; }
        protected HashSet<GUIData> ActiveGUIs { get; set; }

        protected Node RootUI { get; set; }

        protected Node PersistentRoot { get; set; }

        public ManagedCursor Cursor { get; protected set; }

        public Tooltip Tooltip { get; protected set; }

        public ContextMenu ContextMenu { get; protected set; }

        public IDictionary<string, Theme> Themes { get; protected set; }

        public IDictionary<string, ISpriteState> UISprites { get; protected set; }
        public IDictionary<string, ISpriteState> Cursors { get; protected set; }

        protected IDictionary<string, DynamicFont> LoadedFonts { get; set; }

        public IDictionary<string, DynamicFont> FontsInUse =>
            this.DyslexicMode ? this.DyslexicModeFonts : this.LoadedFonts;

        public IDictionary<string, Tuple<float, float>> FontSizesInUse =>
            this.DyslexicMode ? this.DyslexicModeFontSizes : this.StandardFontSizes;

        protected IDictionary<string, Tuple<float, float>> StandardFontSizes { get; set; }
        protected IDictionary<string, Tuple<float, float>> DyslexicModeFontSizes { get; set; }
        public IDictionary<string, IDictionary<string, Color>> CursorColours { get; protected set; }
        public IDictionary<string, IDictionary<string, Color>> UISpriteColours { get; protected set; }

        protected bool DyslexicMode { get; set; }
        protected IDictionary<string, DynamicFont> DyslexicModeFonts { get; set; }

        public IDictionary<string, Color> FontColours { get; protected set; }

        public IEnumerable<GUIData> Values => this.GUIs;

        public JSONValueExtractor ValueExtractor { get; protected set; }

        public GUIManager(Node rootUi)
        {
            this.ValueExtractor = new JSONValueExtractor();
            this.RootUI = rootUi;
            this.Initialise();
        }

        public GUIData Get(string name)
        {
            return this.GUIs.FirstOrDefault(gui => gui.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<GUIData> GetMany(IEnumerable<string> keys)
        {
            return keys.Select(this.Get);
        }

        public T Get<T>(string name) where T : GUIData
        {
            return (T) this.GUIs.FirstOrDefault(gui => gui.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<GUIData> Load()
        {
            return new GUIData[0];
        }

        protected void Initialise()
        {
            if (this.GUIs is null)
            {
                this.PersistentRoot = this.RootUI.GetNode("../Persistent UI");

                this.Themes = new System.Collections.Generic.Dictionary<string, Theme>();

                this.GUIs = new HashSet<GUIData>();
                this.ActiveGUIs = new HashSet<GUIData>();

                this.UISprites = new System.Collections.Generic.Dictionary<string, ISpriteState>();
                
                this.Cursors = new System.Collections.Generic.Dictionary<string, ISpriteState>();
                var cursors = GlobalConstants.GameManager.ObjectIconHandler.GetSpritesForManagedAssets("Cursors");
                foreach (SpriteData data in cursors)
                {
                    this.Cursors.Add(
                        data.Name,
                        new SpriteState(
                            data.Name,
                            "Cursors",
                            data));
                }
                
                this.CursorColours = new System.Collections.Generic.Dictionary<string, IDictionary<string, Color>>();
                this.UISpriteColours = new System.Collections.Generic.Dictionary<string, IDictionary<string, Color>>();
                this.LoadedFonts = new System.Collections.Generic.Dictionary<string, DynamicFont>
                {
                    {"default", GD.Load<DynamicFont>(GlobalConstants.GODOT_ASSETS_FOLDER + "Fonts/OpenDyslexic3.tres")}
                };

                this.DyslexicModeFonts = new System.Collections.Generic.Dictionary<string, DynamicFont>
                {
                    {"default", this.LoadedFonts["default"]}
                };
                this.FontColours = new System.Collections.Generic.Dictionary<string, Color>
                {
                    {"default", Colors.Black}
                };
                this.StandardFontSizes = new System.Collections.Generic.Dictionary<string, Tuple<float, float>>
                {
                    {"default", new Tuple<float, float>(8f, 36f)}
                };
                this.DyslexicModeFontSizes = new System.Collections.Generic.Dictionary<string, Tuple<float, float>>
                {
                    {"default", new Tuple<float, float>(8f, 36f)}
                };
                this.LoadDefaults();
                this.LoadDefinitions();

                /*
                GlobalConstants.GameManager.SettingsManager.OnSettingChange -= this.SettingChanged;
                GlobalConstants.GameManager.SettingsManager.OnSettingChange += this.SettingChanged;
                */
            }
        }

        /*
        protected void SettingChanged(SettingChangedEventArgs args)
        {
            if (args.Setting is DyslexicModeSetting dyslexicModeSetting)
            {
                this.DyslexicMode = dyslexicModeSetting.value;
                this.RecolourGUIs();
                LayoutRebuilder.MarkLayoutForRebuild(this.MainUI.GetComponent<RectTransform>());
            }
        }
        */

        protected void LoadDefaults()
        {
            string file = Directory.GetCurrentDirectory() +
                          GlobalConstants.ASSETS_FOLDER +
                          GlobalConstants.SETTINGS_FOLDER +
                          "/GUIDefaults.json";

            if (File.Exists(file))
            {
                this.LoadFontSettings(
                    file,
                    this.StandardFontSizes,
                    this.LoadedFonts);
            }
            else
            {
                GlobalConstants.ActionLog.Log("COULD NOT FIND GUI DEFAULTS.", LogLevel.Error);
            }

            file = Directory.GetCurrentDirectory() + GlobalConstants.SETTINGS_FOLDER + "/DyslexicMode.json";

            if (File.Exists(file))
            {
                this.LoadFontSettings(
                    file,
                    this.DyslexicModeFontSizes,
                    this.DyslexicModeFonts);
            }

            /*
            this.DyslexicMode = (bool) GlobalConstants.GameManager.SettingsManager
                .GetSetting(SettingNames.DYSLEXIC_MODE).objectValue;
                */
        }

        protected void LoadFontSettings(
            string file,
            IDictionary<string, Tuple<float, float>> sizes,
            IDictionary<string, DynamicFont> fonts)
        {
            if (File.Exists(file) == false)
            {
                GlobalConstants.ActionLog.Log("Could not find font settings file " + file, LogLevel.Warning);
                return;
            }

            JSONParseResult result = JSON.Parse(File.ReadAllText(file));

            if (result.Error != Error.Ok)
            {
                this.ValueExtractor.PrintFileParsingError(result, file);
            }

            if (!(result.Result is Dictionary dictionary))
            {
                GlobalConstants.ActionLog.Log("Could not parse JSON to Dictionary from " + file, LogLevel.Warning);
                return;
            }

            ICollection<Dictionary> fontSettings =
                this.ValueExtractor.GetArrayValuesCollectionFromDictionary<Dictionary>(dictionary, "GUIData");

            foreach (Dictionary fontDict in fontSettings)
            {
                string name = this.ValueExtractor.GetValueFromDictionary<string>(fontDict, "Name");
                string value = this.ValueExtractor.GetValueFromDictionary<string>(fontDict, "Value");
                float minFontSize = this.ValueExtractor.GetValueFromDictionary<float>(fontDict, "MinFontSize");
                float maxFontSize = this.ValueExtractor.GetValueFromDictionary<float>(fontDict, "MaxFontSize");

                DynamicFont font = GD.Load<DynamicFont>(
                    GlobalConstants.GODOT_ASSETS_FOLDER +
                    "Fonts/" +
                    value) ?? this.LoadedFonts["default"];

                sizes.Add(name, new Tuple<float, float>(minFontSize, maxFontSize));
                fonts.Add(name, font);
            }
        }

        protected void LoadDefinitions()
        {
            string[] files = Directory.GetFiles(
                Directory.GetCurrentDirectory() +
                GlobalConstants.ASSETS_FOLDER +
                GlobalConstants.DATA_FOLDER +
                "Sprite Definitions/GUI/",
                "*.json",
                SearchOption.AllDirectories);

            foreach (string file in files)
            {
                if (File.Exists(file) == false)
                {
                    GlobalConstants.ActionLog.Log("Could not find GUI definitions file " + file, LogLevel.Warning);
                    return;
                }

                JSONParseResult result = JSON.Parse(File.ReadAllText(file));

                if (result.Error != Error.Ok)
                {
                    this.ValueExtractor.PrintFileParsingError(result, file);
                }

                if (!(result.Result is Dictionary dictionary))
                {
                    GlobalConstants.ActionLog.Log("Could not parse JSON to Dictionary from " + file, LogLevel.Warning);
                    return;
                }

                string tileSetName = this.ValueExtractor.GetValueFromDictionary<string>(
                    this.ValueExtractor.GetValueFromDictionary<Dictionary>(dictionary, "TileSet"),
                    "Name");

                var spriteData = GlobalConstants.GameManager.ObjectIconHandler.GetSpritesForManagedAssets(tileSetName);
                foreach (var data in spriteData)
                {
                    if (this.UISprites.ContainsKey(data.Name))
                    {
                        continue;
                    }

                    this.UISprites.Add(data.Name, new SpriteState(data.Name, tileSetName, data));
                }
            }
        }

        public void SetUIColours(
            IDictionary<string, IDictionary<string, Color>> background,
            IDictionary<string, IDictionary<string, Color>> cursor,
            IDictionary<string, Color> mainFontColours,
            bool recolour = true,
            bool crossFade = false,
            float duration = 0.1f)
        {
            this.UISpriteColours = background;
            this.CursorColours = cursor;
            this.FontColours = mainFontColours;

            if (recolour)
            {
                this.RecolourGUIs(crossFade, duration);
            }
        }

        public void Clear()
        {
            Array children = this.RootUI.GetChildren();
            if (children.IsNullOrEmpty() == false)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    Node child = this.RootUI.GetChild(0);
                    if (child is GUIData data)
                    {
                        data.DisconnectEvents();
                    }
                    
                    this.RootUI.RemoveChild(child);
                    child.QueueFree();
                }
            }
            
            this.ActiveGUIs.Clear();
            this.GUIs.Clear();
        }

        public void FindGUIs()
        {
            Array guiData = this.RootUI.GetAllChildren();
            foreach (var child in guiData)
            {
                if (child is GUIData data)
                {
                    this.Add(data);
                }
            }

            if (this.Cursor is null)
            {
                this.Cursor = this.PersistentRoot.GetNode<ManagedCursor>("Cursor");
            }

            if (this.Tooltip is null)
            {
                this.Tooltip = this.PersistentRoot.GetNode<Tooltip>("Tooltip");
                this.SetupManagedComponents(this.Tooltip);
            }

            this.Add(this.Tooltip);

            if (this.ContextMenu is null)
            {
                this.ContextMenu = this.PersistentRoot.GetNode<ContextMenu>("Context Menu");
                this.SetupManagedComponents(this.ContextMenu);
            }

            this.Add(this.ContextMenu);
        }

        public void InstantiateUIScene(PackedScene ui)
        {
            this.Clear();

            Control newUI = (Control) ui.Instance();
            newUI.AnchorBottom = 1;
            newUI.AnchorRight = 1;
            this.RootUI.AddChild(newUI);
        }

        public bool Add(GUIData gui)
        {
            this.Initialise();
            if (this.GUIs.Contains(gui))
            {
                return false;
            }

            gui.GUIManager = this;
            //gui.Close();

            this.SetupManagedComponents(gui);

            this.GUIs.Add(gui);

            if (gui.Visible)
            {
                this.ActiveGUIs.Add(gui);
            }

            return true;
        }

        public bool Destroy(string key)
        {
            GUIData gui = this.GUIs.FirstOrDefault(data => data.Name.Equals(key, StringComparison.OrdinalIgnoreCase));
            return !(gui is null) && this.GUIs.Remove(gui);
        }

        public void RecolourGUIs(bool crossFade = false, float duration = 0.1f)
        {
            foreach (GUIData gui in this.GUIs)
            {
                this.SetupManagedComponents(gui, crossFade, duration);
            }

            this.Cursor.AddSpriteState(this.Cursors["DefaultCursor"]);
            this.Cursor.OverrideAllColours(this.CursorColours["DefaultCursor"], crossFade, duration);
            this.SetupManagedComponents(this.Tooltip);
        }

        public void SetupManagedComponents(Control gui, bool crossFade = false, float duration = 0.1f)
        {
            if (gui is IManagedElement guiElement)
            {
                this.SetUpManagedComponent(guiElement);
            }

            Array managedComponents = gui.GetAllChildren();
            foreach (var component in managedComponents)
            {
                if (component is IManagedElement element)
                {
                    this.SetUpManagedComponent(element, crossFade, duration);
                }
            }
        }

        public void SetUpManagedComponent(IManagedElement element, bool crossFade = false, float duration = 0.1f)
        {
            if (element.ElementName.IsNullOrEmpty())
            {
                return;
            }

            if (element.Initialised == false)
            {
                element.Initialise();
            }

            if (element is ISpriteStateElement spriteStateElement)
            {
                if (this.UISprites.TryGetValue(spriteStateElement.ElementName, out ISpriteState value))
                {
                    spriteStateElement.Clear();
                    spriteStateElement.AddSpriteState(value);
                }
                else
                {
                    GlobalConstants.ActionLog.Log("Could not find sprite " + spriteStateElement.ElementName,
                        LogLevel.Warning);
                }
            }

            if (element is IColourableElement colourfulElement)
            {
                if (this.UISpriteColours.TryGetValue(colourfulElement.ElementName,
                    out IDictionary<string, Color> value))
                {
                    colourfulElement.OverrideAllColours(value, crossFade, duration, true);
                }
                else
                {
                    GlobalConstants.ActionLog.Log("Could not find colours for " + colourfulElement.ElementName,
                        LogLevel.Warning);
                }
            }
        }

        public void ToggleGUI(object sender, string name)
        {
            if (this.ActiveGUIs.Any(gui => gui.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                GUIData[] toToggle = this.ActiveGUIs
                    .Where(gui => gui.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    .ToArray();
                foreach (GUIData data in toToggle)
                {
                    this.CloseGUI(sender, data.Name);
                }
            }
            else
            {
                this.OpenGUI(sender, name);
            }
        }

        public GUIData OpenGUI(object sender, string name, bool bringToFront = false)
        {
            if (this.ActiveGUIs.Any(widget => widget.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                GUIData openGUI = this.ActiveGUIs.First(ui => ui.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (bringToFront)
                {
                    this.BringToFront(openGUI.Name);
                }

                return openGUI;
            }

            GUIData toOpen = this.GUIs.FirstOrDefault(gui =>
                gui.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (toOpen is null)
            {
                return null;
            }

            if (toOpen.ClosesOthers)
            {
                List<GUIData> activeCopy = new List<GUIData>(this.ActiveGUIs);
                foreach (GUIData widget in activeCopy)
                {
                    this.CloseGUI(sender, widget.Name);
                }
            }

            toOpen.Display();
            toOpen.SetProcess(true);
            this.ActiveGUIs.Add(toOpen);

            if (bringToFront)
            {
                this.BringToFront(toOpen.Name);
            }

            return toOpen;
        }

        public void CloseGUI(object sender, string activeName)
        {
            if (this.ActiveGUIs.Any(data => data.Name.Equals(activeName, StringComparison.OrdinalIgnoreCase)) == false)
            {
                return;
            }

            GUIData toClose = this.ActiveGUIs
                .First(gui => gui.Name.Equals(activeName, StringComparison.OrdinalIgnoreCase));

            if (toClose.AlwaysOpen)
            {
                return;
            }

            if (toClose.Close(sender))
            {
                toClose.SetProcess(false);
                this.ActiveGUIs.Remove(toClose);
            }
        }

        public bool RemoveActiveGUI(string name, bool close = false, bool disable = false)
        {
            if (this.ActiveGUIs.Any(data => data.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) == false)
            {
                return false;
            }

            GUIData toClose = this.ActiveGUIs.First(data => data.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (toClose.AlwaysOpen)
            {
                return false;
            }

            if (close)
            {
                toClose.Close(this);
            }
            
            if (disable)
            {
                toClose.SetProcess(false);
            }
            return this.ActiveGUIs.Remove(toClose);
        }

        public void BringToFront(string name)
        {
            if (!this.ActiveGUIs.Any(gui => gui.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            GUIData toFront = this.ActiveGUIs.First(g => g.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            foreach (GUIData gui in this.ActiveGUIs)
            {
                if (toFront.Equals(gui) || gui.AlwaysOnTop)
                {
                    continue;
                }

                //gui.ZIndex = gui.DefaultSortingOrder;
            }

            GUIData[] found = this.ActiveGUIs
                .Where(data => data.AlwaysOpen == false)
                .ToArray();
            if (found.Any())
            {
                //toFront.ZIndex = found.Max(data => data.DefaultSortingOrder) + 1;
            }
        }

        public void CloseAllOtherGUIs(string activeName = "")
        {
            GUIData[] toClose = this.ActiveGUIs
                .Where(gui => gui.Name.Equals(activeName, StringComparison.OrdinalIgnoreCase) == false
                              && gui.AlwaysOpen == false)
                .ToArray();

            foreach (GUIData data in toClose)
            {
                this.CloseGUI(data, data.Name);
            }
        }

        public void CloseAllGUIs()
        {
            GUIData[] toClose = this.ActiveGUIs
                .Where(gui => gui.AlwaysOpen == false)
                .ToArray();

            foreach (GUIData data in toClose)
            {
                data.Close(this);
            }

            this.ActiveGUIs.RemoveWhere(guiData => guiData.AlwaysOpen == false);
        }

        public bool RemovesControl()
        {
            return this.ActiveGUIs.Any(gui => gui.RemovesControl);
        }

        public bool IsActive(string name)
        {
            return this.ActiveGUIs.Any(gui => gui.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public bool AreAnyOpen(bool includeAlwaysOpen = false)
        {
            if (includeAlwaysOpen)
            {
                return this.ActiveGUIs.Count > 0;
            }

            return this.ActiveGUIs.Count(data => data.AlwaysOpen == false) > 0;
        }

        public void Dispose()
        {
            this.GUIs = null;
            this.ActiveGUIs = null;

            GarbageMan.Dispose(this.Cursors);
            this.Cursors = null;

            GarbageMan.Dispose(this.CursorColours);
            this.CursorColours = null;

            GarbageMan.Dispose(this.LoadedFonts);
            this.LoadedFonts = null;
            GarbageMan.Dispose(this.FontColours);
            this.FontColours = null;

            GarbageMan.Dispose(this.DyslexicModeFonts);
            this.DyslexicModeFonts = null;

            GarbageMan.Dispose(this.StandardFontSizes);
            this.StandardFontSizes = null;

            GarbageMan.Dispose(this.UISprites);
            this.UISprites = null;

            GarbageMan.Dispose(this.UISpriteColours);
            this.UISpriteColours = null;
        }
    }
}