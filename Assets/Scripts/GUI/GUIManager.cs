using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using JoyLib.Code.Graphics;
using JoyLib.Code.Helpers;
using Array = Godot.Collections.Array;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace JoyLib.Code.Unity.GUI
{
    public class GUIManager : IGUIManager
    {
        protected HashSet<GUIData> GUIs { get; set; }
        protected HashSet<GUIData> ActiveGUIs { get; set; }
        
        protected Node RootUI { get; set; }
        
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

        public IEnumerable<GUIData> Load()
        {
            return new GUIData[0];
        }

        protected void Initialise()
        {
            if (this.GUIs is null)
            {
                this.Themes = new Dictionary<string, Theme>();
                
                this.GUIs = new HashSet<GUIData>();
                this.ActiveGUIs = new HashSet<GUIData>();

                this.UISprites = new Dictionary<string, ISpriteState>();
                
                /*
                this.Cursors = GlobalConstants.GameManager.ObjectIconHandler.GetTileSet("Cursors")
                    .Select(data => new SpriteState(data.m_Name, data))
                    .Cast<ISpriteState>()
                    .ToDictionary(state => state.Name, state => state);
*/
                this.CursorColours = new Dictionary<string, IDictionary<string, Color>>();
                this.UISpriteColours = new Dictionary<string, IDictionary<string, Color>>();
                this.LoadedFonts = new Dictionary<string, DynamicFont>
                {
                    {"default", GD.Load<DynamicFont>("Fonts/OpenDyslexic3")}
                };

                this.DyslexicModeFonts = new Dictionary<string, DynamicFont>
                {
                    {"default", this.LoadedFonts["default"]}
                };
                this.FontColours = new Dictionary<string, Color>
                {
                    {"default", Colors.Black}
                };
                this.StandardFontSizes = new Dictionary<string, Tuple<float, float>>
                {
                    {"default", new Tuple<float, float>(8f, 36f)}
                };
                this.DyslexicModeFontSizes = new Dictionary<string, Tuple<float, float>>
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
            string file = Directory.GetCurrentDirectory() + GlobalConstants.SETTINGS_FOLDER + "/GUIDefaults.json";

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
            /*
            using (StreamReader reader = new StreamReader(file))
            {
                using (JsonTextReader jsonReader = new JsonTextReader(reader))
                {
                    try
                    {
                        JObject jToken = JObject.Load(jsonReader);

                        if (jToken.IsNullOrEmpty())
                        {
                            return;
                        }

                        foreach (JToken child in jToken["GUIData"])
                        {
                            string name = (string) child["Name"] ?? "default";
                            TMP_FontAsset font = child["Value"] is null
                                ? this.LoadedFonts["default"]
                                : Resources.Load<TMP_FontAsset>("Fonts/" + child["Value"]);

                            float minSize = (float) (child["MinFontSize"] ?? 8);
                            float maxSize = (float) (child["MaxFontSize"] ?? 24);

                            sizes.Add(
                                name,
                                new Tuple<float, float>(
                                    minSize,
                                    maxSize));

                            fonts.Add(name, font);
                        }
                    }
                    catch (Exception e)
                    {
                        GlobalConstants.ActionLog.AddText("Failed loading default GUI settings in " + file);
                        GlobalConstants.ActionLog.StackTrace(e);
                    }
                }
            }
            */
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
                /*
                using (StreamReader reader = new StreamReader(file))
                {
                    using (JsonTextReader jsonReader = new JsonTextReader(reader))
                    {
                        try
                        {
                            JObject jToken = JObject.Load(jsonReader);

                            if (jToken.IsNullOrEmpty())
                            {
                                continue;
                            }

                            JToken child = jToken["Objects"]["TileSet"];

                            string tileSetName = (string) child["Name"];
                            var spriteData = GlobalConstants.GameManager.ObjectIconHandler.GetTileSet(tileSetName);
                            foreach (SpriteData data in spriteData)
                            {
                                if (this.UISprites.ContainsKey(data.m_Name))
                                {
                                    continue;
                                }
                                this.UISprites.Add(data.m_Name, new SpriteState(data.m_Name, data));
                            }
                        }
                        catch (Exception e)
                        {
                            GlobalConstants.ActionLog.AddText("Could not load GUI definitions from " + file);
                            GlobalConstants.ActionLog.StackTrace(e);
                        }
                    }
                }
                */
            }
        }

        public void SetUIColours(IDictionary<string, IDictionary<string, Color>> background,
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
            this.ActiveGUIs.Clear();
            this.GUIs.Clear();
        }

        public void FindGUIs()
        {
            Array guiData = this.RootUI.GetChildren();
            foreach (var child in guiData)
            {
                if (child is GUIData data)
                {
                    this.Add(data);
                }
            }
        }

        public bool Add(GUIData gui)
        {
            this.Initialise();
            if (this.GUIs.Contains(gui))
            {
                return false;
            }

            gui.GUIManager = this;
            gui.Close();

            this.SetupManagedComponents(gui);

            this.GUIs.Add(gui);
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

            Cursor cursor = (Cursor) this.GUIs.FirstOrDefault(data => data is Cursor);
            if (cursor is null == false)
            {
                cursor.SetCursorSprites(this.Cursors["DefaultCursor"]);
                cursor.SetCursorColours(this.CursorColours["DefaultCursor"]);
            }
        }

        public void SetupManagedComponents(GUIData gui, bool crossFade = false, float duration = 0.1f)
        {
            Array managedComponents = gui.GetChildren();
            foreach (var component in managedComponents)
            {
                if (component is ManagedFonts fonts)
                {
                    if (this.Themes.TryGetValue(fonts.ElementName, out Theme value))
                    {
                        fonts.SetTheme(value);
                    }
                    else
                    {
                        GlobalConstants.ActionLog.Log("Could not find font theme " + fonts.ElementName, LogLevel.Warning);
                    }
                }
                else if (component is ManagedSprite managedSprite)
                {
                    if (this.Themes.TryGetValue(managedSprite.ElementName, out Theme value))
                    {
                        managedSprite.SetTheme(value);
                    }
                    else
                    {
                        GlobalConstants.ActionLog.Log("Could not find font theme " + managedSprite.ElementName, LogLevel.Warning);
                    }
                }
            }
        }

        public void ToggleGUI(string name)
        {
            if (this.ActiveGUIs.Any(gui => gui.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                GUIData[] toToggle = this.ActiveGUIs
                    .Where(gui => gui.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    .ToArray();
                foreach (GUIData data in toToggle)
                {
                    this.CloseGUI(data.Name);
                }
            }
            else
            {
                this.OpenGUI(name);
            }
        }

        public GUIData OpenGUI(string name, bool bringToFront = false)
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

            GUIData toOpen = this.GUIs.First(gui =>
                gui.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (toOpen.m_ClosesOthers)
            {
                List<GUIData> activeCopy = new List<GUIData>(this.ActiveGUIs);
                foreach (GUIData widget in activeCopy)
                {
                    this.CloseGUI(widget.Name);
                }
            }

            toOpen.Show();

            this.ActiveGUIs.Add(toOpen);

            if (bringToFront)
            {
                this.BringToFront(toOpen.Name);
            }

            return toOpen;
        }

        public void CloseGUI(string activeName)
        {
            if (this.ActiveGUIs.Any(data => data.Name.Equals(activeName, StringComparison.OrdinalIgnoreCase)) == false)
            {
                return;
            }

            GUIData toClose = this.ActiveGUIs
                .First(gui => gui.Name.Equals(activeName, StringComparison.OrdinalIgnoreCase));

            if (toClose.m_AlwaysOpen)
            {
                return;
            }

            toClose.Close();
            this.ActiveGUIs.Remove(toClose);
        }

        public bool RemoveActiveGUI(string name)
        {
            if (this.ActiveGUIs.Any(data => data.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) == false)
            {
                return false;
            }

            GUIData toClose = this.ActiveGUIs.First(data => data.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (toClose.m_AlwaysOpen)
            {
                return false;
            }

            toClose.Close();
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
                if (toFront.Equals(gui) || gui.m_AlwaysOnTop)
                {
                    continue;
                }

                gui.ZIndex = gui.DefaultSortingOrder;
            }

            GUIData[] found = this.ActiveGUIs
                .Where(data => data.m_AlwaysOpen == false)
                .ToArray();
            if (found.Any())
            {
                toFront.ZIndex = found.Max(data => data.DefaultSortingOrder) + 1;
            }
        }

        public void CloseAllOtherGUIs(string activeName = "")
        {
            GUIData[] toClose = this.ActiveGUIs
                .Where(gui => gui.Name.Equals(activeName, StringComparison.OrdinalIgnoreCase) == false
                              && gui.m_AlwaysOpen == false)
                .ToArray();

            foreach (GUIData data in toClose)
            {
                this.ActiveGUIs.Remove(data);
                data.Close();
            }
        }

        public void CloseAllGUIs()
        {
            GUIData[] toClose = this.ActiveGUIs
                .Where(gui => gui.m_AlwaysOpen == false)
                .ToArray();

            foreach (GUIData data in toClose)
            {
                data.Close();
            }

            this.ActiveGUIs.RemoveWhere(guiData => guiData.m_AlwaysOpen == false);
        }

        public bool RemovesControl()
        {
            return this.ActiveGUIs.Any(gui => gui.m_RemovesControl);
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

            return this.ActiveGUIs.Count(data => data.m_AlwaysOpen == false) > 0;
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