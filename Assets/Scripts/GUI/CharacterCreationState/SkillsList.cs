using System.Collections.Generic;
using System.Linq;
using Godot;
using JoyGodot.Assets.Scripts.GUI.Managed_Assets;
using JoyLib.Code;
using JoyLib.Code.Entities.Statistics;
using JoyLib.Code.Unity.GUI;

namespace JoyGodot.Assets.Scripts.GUI.CharacterCreationState
{
    public class SkillsList : Control
    {
        [Signal]
        public delegate void SkillsBlockChanged();

        [Signal]
        public delegate void SkillValueChanged(string name, int delta, int newValue);
        
        public ICollection<IEntitySkill> Skills
        {
            get => this.m_Skills;
            set
            {
                this.m_Skills = value;
                this.SetUpSkills(this.m_Skills);
                this.EmitSignal("SkillsBlockChanged");
            }
        }

        protected ICollection<IEntitySkill> m_Skills;
        
        protected List<IntValueItem> Parts { get; set; }
        
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
                    this.PointsLabel.Text = "Skill Points Remaining: " + this.m_Points;
                }
                this.SetChildPoints();
            }
        }

        protected int m_Points;
        
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
            this.ChildContainer = this.FindNode("Skills List") as VBoxContainer;
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
        
        protected void SetUpSkills(ICollection<IEntitySkill> skills)
        {
            if (skills.Count > this.Parts.Count 
                && this.PartPrefab is null == false)
            {
                for (int i = this.Parts.Count; i < skills.Count; i++)
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
            
            for (int i = 0; i < skills.Count; i++)
            {
                var skill = skills.ElementAt(i);
                var part = this.Parts[i];
                part.ValueName = skill.Name;
                part.Minimum = 0;
                part.Maximum = 3;
                part.Value = skill.Value;
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
            GD.Print(name + " : " + delta + " : " + newValue);
            if (this.Points - delta >= 0)
            {
                this.Points -= delta;
                this.SetChildPoints();
                this.EmitSignal("SkillValueChanged", name, delta, newValue);
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