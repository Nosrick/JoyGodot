using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;
using JoyLib.Code;
using JoyLib.Code.Entities.Statistics;
using JoyLib.Code.Unity.GUI;

namespace JoyGodot.Assets.Scripts.GUI.CharacterCreationState
{
    public class DerivedValuesList : Control
    {
        public ICollection<IDerivedValue> DerivedValues
        {
            get => this.m_DerivedValues;
            set
            {
                this.m_DerivedValues = value.ToList();
                this.SetUpDerivedValues(this.m_DerivedValues);
                this.EmitSignal("DerivedValuesSet");
            }
        }

        protected List<IDerivedValue> m_DerivedValues;
        
        protected List<IntValueItem> Parts { get; set; }
        
        protected PackedScene PartPrefab { get; set; }

        [Signal]
        public delegate void DerivedValuesSet();

        [Signal]
        public delegate void DerivedValueChanged(string name, int delta, int newValue);
        
        public override void _EnterTree()
        {
            this.Parts = new List<IntValueItem>();
            this.PartPrefab = GD.Load<PackedScene>(
                GlobalConstants.GODOT_ASSETS_FOLDER +
                "Scenes/Parts/Int List Item.tscn");
        }
        
        public override void _ExitTree()
        {
            base._ExitTree();
            
            foreach (var part in this.Parts)
            {
                if (part.IsConnected(
                    "ValueChanged",
                    this,
                    nameof(this.ChangeValue)))
                {
                    part.Disconnect(
                        "ValueChanged",
                        this,
                        nameof(this.ChangeValue));
                }
            }
        }
    
        protected void SetUpDerivedValues(List<IDerivedValue> derivedValues)
        {
            if (derivedValues.Count > this.Parts.Count 
                && this.PartPrefab is null == false)
            {
                for (int i = this.Parts.Count; i < derivedValues.Count; i++)
                {
                    if (!(this.PartPrefab.Instance() is IntValueItem instance))
                    {
                        GD.PushWarning("INT LIST ITEM PREFAB IS NULL, AT " + this.GetPath());
                        return;
                    }

                    instance.Visible = false;
                    this.AddChild(instance);
                    this.Parts.Add(instance);
                }
            }
            
            for (int i = 0; i < derivedValues.Count; i++)
            {
                var derivedValue = derivedValues[i];
                var part = this.Parts[i];
                part.ValueName = derivedValue.Name;
                part.Minimum = derivedValue.Base;
                part.Maximum = derivedValue.Base + 5;
                part.Value = derivedValue.Value;
                part.Visible = true;
                if (!part.IsConnected(
                    "ValueChanged",
                    this,
                    nameof(this.ChangeValue)))
                {
                    part.Connect(
                        "ValueChanged",
                        this,
                        nameof(this.ChangeValue));
                }
            }
            
            this.CallDeferred("DeferredSetUp");
        }
        
        protected void DeferredSetUp()
        {
            GlobalConstants.GameManager.GUIManager.SetupManagedComponents(this);
        }
        
        public void ChangeValue(string name, int delta, int newValue)
        {
            GD.Print(name + " : " + delta + " : " + newValue);
            this.EmitSignal("DerivedValueChanged", name, delta, newValue);
        }
    }
}