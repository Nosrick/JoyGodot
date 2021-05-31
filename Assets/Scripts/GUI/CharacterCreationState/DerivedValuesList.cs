using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;
using JoyGodot.Assets.Scripts.GUI.Managed_Assets;
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
        
        protected ManagedLabel PointsLabel { get; set; }
        
        protected VBoxContainer ChildContainer { get; set; }
        
        protected PackedScene PartPrefab { get; set; }

        public int Points
        {
            get => this.m_Points;
            set
            {
                this.m_Points = value;
                if (this.PointsLabel is null == false)
                {
                    this.PointsLabel.Text = "Derived Value Points Remaining: " + this.m_Points;
                }
                this.SetChildPoints();
            }
        }

        protected int m_Points;

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

        public override void _Ready()
        {
            base._Ready();

            this.PointsLabel = this.FindNode("Points Remaining") as ManagedLabel;
            this.ChildContainer = this.FindNode("Derived Values List") as VBoxContainer;
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
                    this.ChildContainer.AddChild(instance);
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
                part.UseRestriction = true;
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
            
            this.SetChildPoints();
            this.CallDeferred("DeferredSetUp");
        }
        
        protected void DeferredSetUp()
        {
            GlobalConstants.GameManager.GUIManager.SetupManagedComponents(this);
        }
        
        public void ChangeValue(string name, int delta, int newValue)
        {
            var derivedValue =
                this.m_DerivedValues.FirstOrDefault(
                    value => value.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (derivedValue is null)
            {
                GD.PushError(name + " derived value not found!");
                return;
            }

            if (this.Points - delta >= 0)
            {
                this.Points -= delta;
                derivedValue.SetEnhancement(derivedValue.Enhancement + delta);
                this.SetChildPoints();
                this.EmitSignal("DerivedValueChanged", name, delta, newValue);
            }
        }

        protected void SetChildPoints()
        {
            foreach (var part in this.Parts)
            {
                if (part.UseRestriction)
                {
                    part.PointRestriction = this.Points;
                }
            }
        }
    }
}