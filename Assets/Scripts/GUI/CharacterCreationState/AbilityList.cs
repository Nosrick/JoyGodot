using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using JoyGodot.Assets.Scripts.GUI.Managed_Assets;
using JoyGodot.Assets.Scripts.Managed_Assets;
using JoyLib.Code;
using JoyLib.Code.Entities.Abilities;

namespace JoyGodot.Assets.Scripts.GUI.CharacterCreationState
{
    public class AbilityList : Control
    {
        [Signal]
        public delegate void AbilityBlockChanged();

        [Signal]
        public delegate void AbilityValueChanged(string name, int delta, bool newValue);
        
        public ICollection<IAbility> Abilities
        {
            get => this.m_Abilities.Where(ability => 
                this.Parts.Any(part => 
                    part.Name.Equals(ability.Name, StringComparison.OrdinalIgnoreCase)
                    && part.Pressed))
                .ToList();
            set
            {
                this.m_Abilities = value;
                this.SetUpAbilities(this.m_Abilities);
                this.EmitSignal("AbilityBlockChanged");
            }
        }

        protected ICollection<IAbility> m_Abilities;
        
        protected List<ConstrainedManagedTextButton> Parts { get; set; }
        
        protected ManagedLabel PointsLabel { get; set; }
        
        protected PackedScene PartPrefab { get; set; }
        
        protected VBoxContainer ChildContainer { get; set; }

        public int Points
        {
            get => this.m_Points;
            set
            {
                this.m_Points = value;
                if (this.PointsLabel is null == false)
                {
                    this.PointsLabel.Text = "Ability Picks Remaining: " + this.m_Points;
                }
            }
        }

        protected int m_Points;
        
        public override void _EnterTree()
        {
            this.Parts = new List<ConstrainedManagedTextButton>();
            this.PartPrefab = GD.Load<PackedScene>(
                GlobalConstants.GODOT_ASSETS_FOLDER +
                "Scenes/Parts/ConstrainedManagedTextButton.tscn");
        }

        public override void _Ready()
        {
            base._Ready();

            this.PointsLabel = this.FindNode("Points Remaining") as ManagedLabel;
            this.ChildContainer = this.FindNode("Abilities List") as VBoxContainer;
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            
            foreach (var part in this.Parts)
            {
                if (part.IsConnected(
                    "ValueToggle",
                    this,
                    nameof(this.ChangeValue)))
                {
                    part.Disconnect(
                        "ValueToggle",
                        this,
                        nameof(this.ChangeValue));
                }
            }
        }
        
        protected void SetUpAbilities(ICollection<IAbility> abilities)
        {
            if (abilities.Count > this.Parts.Count 
                && this.PartPrefab is null == false)
            {
                for (int i = this.Parts.Count; i < abilities.Count; i++)
                {
                    if (!(this.PartPrefab.Instance() is ConstrainedManagedTextButton instance))
                    {
                        GD.PushWarning("MANAGED TEXT BUTTON ITEM PREFAB IS NULL, AT " + this.GetPath());
                        return;
                    }

                    instance.Visible = false;
                    instance.ToggleMode = true;
                    this.ChildContainer.AddChild(instance);
                    this.Parts.Add(instance);
                }
            }
            
            for (int i = 0; i < abilities.Count; i++)
            {
                var ability = abilities.ElementAt(i);
                var part = this.Parts[i];
                part.Text = ability.Name;
                part.Name = ability.Name;
                part.Visible = true;
                part.Pressed = false;
                part.PointRestriction = this.Points;
                part.Value = 1;
                part.Tooltip = new List<string> {ability.Description};
                if (!part.IsConnected(
                    "ValueToggle",
                    this,
                    nameof(this.ChangeValue)))
                {
                    part.Connect(
                        "ValueToggle",
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
        
        public void ChangeValue(string elementName, int delta, bool newValue)
        {
            if (this.Points - delta >= 0)
            {
                this.Points -= delta;
                this.SetChildPoints();
                this.EmitSignal("AbilityValueChanged", elementName, delta, newValue);
            }
        }

        protected void SetChildPoints()
        {
            foreach (var part in this.Parts)
            {
                part.PointRestriction = this.Points;
            }
        }
    }
}