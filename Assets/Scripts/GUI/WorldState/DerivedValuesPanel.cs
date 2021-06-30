using System.Collections.Generic;
using System.Linq;
using Godot;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Statistics;
using JoyGodot.Assets.Scripts.GUI.Tools;

namespace JoyGodot.Assets.Scripts.GUI.WorldState
{
    public class DerivedValuesPanel : GUIData
    {
        [Export] protected HorizontalAnchor HorizontalAnchor { get; set; }
        [Export] protected VerticalAnchor VerticalAnchor { get; set; }
        
        protected PackedScene BarPrefab { get; set; }
        
        protected List<ValueBar> Parts { get; set; }
        
        protected BoxContainer ContainerParent { get; set; }

        public override void _Ready()
        {
            base._Ready();

            this.BarPrefab = GD.Load<PackedScene>(
                GlobalConstants.GODOT_ASSETS_FOLDER +
                "Scenes/Parts/ValueBar.tscn");

            this.Parts = new List<ValueBar>();
            
            this.ContainerParent = this.FindNode("BarContainer") as BoxContainer;
            
            this.MakeDerivedValues();
        }

        protected void MakeDerivedValues()
        {
            var derivedValues = this.Player.DerivedValues.Count;

            for (int i = this.Parts.Count; i < derivedValues; i++)
            {
                var instance = this.BarPrefab.Instance<ValueBar>();
                this.Parts.Add(instance);
                this.ContainerParent.AddChild(instance);
            }
            this.CallDeferred(nameof(this.DeferredSetup));
        }

        protected void DeferredSetup()
        {
            var derivedValues = this.Player.DerivedValues.Values;
            
            for(int i = 0; i < derivedValues.Count; i++)
            {
                var instance = this.Parts[i];
                instance.DerivedValue = derivedValues.ElementAt(i);
                this.Player.OnDerivedValueChange -= instance.OnDerivedValueChange;
                this.Player.OnDerivedValueChange += instance.OnDerivedValueChange;
            }
        }

        public override void DisconnectEvents()
        {
            base.DisconnectEvents();

            foreach (var part in this.Parts)
            {
                this.Player.OnDerivedValueChange -= part.OnDerivedValueChange;
            }
        }
    }
}