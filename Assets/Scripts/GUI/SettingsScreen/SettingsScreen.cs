using System.Collections.Generic;
using Godot;

namespace JoyLib.Code.Unity.GUI.SettingsScreen
{
    public class SettingsScreen : GUIData
    {
        protected List<StringValueItem> Parts { get; set; }
        
        protected PackedScene ItemPrefab { get; set; }

        public override void _Ready()
        {
            base._Ready();

            this.ItemPrefab = GD.Load<PackedScene>(
                GlobalConstants.GODOT_ASSETS_FOLDER +
                "Scenes/Parts/String List Item.tscn");
        }
    }
}