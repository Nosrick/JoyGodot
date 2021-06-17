using System.Collections.Generic;
using System.Linq;
using Godot;
using JoyGodot.Assets.Scripts.GUI.Tools;
using JoyGodot.Assets.Scripts.Settings;

namespace JoyGodot.Assets.Scripts.GUI.SettingsScreen
{
    public class SettingsScreen : GUIData
    {
        protected List<StringValueItem> Parts { get; set; }
        
        protected SettingsManager SettingsManager { get; set; }
        
        protected PackedScene ItemPrefab { get; set; }
        
        protected VBoxContainer ChildParent { get; set; }

        public override void _Ready()
        {
            base._Ready();

            this.SettingsManager = GlobalConstants.GameManager.SettingsManager;
            this.Parts = new List<StringValueItem>();

            this.ItemPrefab = GD.Load<PackedScene>(
                GlobalConstants.GODOT_ASSETS_FOLDER +
                "Scenes/Parts/String List Item.tscn");
            
            this.ChildParent = this.FindNode("SettingsList") as VBoxContainer;

            this.MakeSettings();
        }

        protected void MakeSettings()
        {
            foreach (var part in this.Parts)
            {
                part.Hide();
            }
            
            var settings = this.SettingsManager.Values.ToArray();
            if (this.Parts.Count < settings.Length)
            {
                for (int i = this.Parts.Count; i < settings.Length; i++)
                {
                    var instance = this.ItemPrefab.Instance() as StringValueItem;
                    instance?.Hide();
                    this.ChildParent.AddChild(instance);
                    this.Parts.Add(instance);
                }
            }

            for (int i = 0; i < settings.Length; i++)
            {
                var item = this.Parts[i];
                var setting = settings[i];
                item.Values = setting.ValueNames;
                item.Index = setting.Index;
                item.Minimum = 0;
                item.Maximum = setting.ValueNames.Count - 1;
                item.ValueName = setting.Name;
                item.Name = setting.Name;
                item.Show();
            }
        }

        public void Save()
        {
            foreach (var part in this.Parts)
            {
                if (part.Visible)
                {
                    this.SettingsManager.ChangeSetting(part.Name, part.Index);
                }
            }
            
            this.SettingsManager.Save();
        }
    }
}