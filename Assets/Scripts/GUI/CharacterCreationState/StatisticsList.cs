using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using JoyGodot.Assets.Scripts.GUI.Managed_Assets;
using JoyLib.Code.Entities.Statistics;

namespace JoyLib.Code.Unity.GUI.CharacterCreationState
{
    public class StatisticsList : Control
    {
        public ICollection<IEntityStatistic> Statistics
        {
            get => this.m_Statistics;
            set
            {
                this.m_Statistics = value.ToList();
                this.SetUpStatistics(this.m_Statistics);
                this.EmitSignal("StatisticBlockSet");
            }
        }

        protected List<IEntityStatistic> m_Statistics;
        
        protected List<IntValueItem> Parts { get; set; }
        
        protected PackedScene PartPrefab { get; set; }
        
        protected ManagedLabel PointsLabel { get; set; }
        
        protected VBoxContainer ChildContainer { get; set; }

        public int Points
        {
            get => this.m_Points;
            set
            {
                this.m_Points = value;
                if (this.PointsLabel is null == false)
                {
                    this.PointsLabel.Text = "Statistic Points Remaining: " + this.m_Points;
                }
                this.SetChildPoints();
            }
        }

        protected int m_Points;

        [Signal]
        public delegate void StatisticBlockSet();

        [Signal]
        public delegate void StatisticChanged(string name, int delta, int newValue);

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
            this.ChildContainer = this.FindNode("Statistics List") as VBoxContainer;
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

        public void SetUpStatistics(List<IEntityStatistic> statistics)
        {
            if (statistics.Count > this.Parts.Count 
                && this.PartPrefab is null == false)
            {
                for (int i = this.Parts.Count; i < statistics.Count; i++)
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
            
            for (int i = 0; i < statistics.Count; i++)
            {
                var stat = statistics[i];
                var part = this.Parts[i];
                part.ValueName = stat.Name;
                part.Minimum = 1;
                part.Maximum = 7;
                part.Value = stat.Value;
                part.Visible = true;
                part.UseRestriction = true;
                part.IncreaseCost = 1;
                part.DecreaseCost = -1;
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
            var stat = this.m_Statistics.FirstOrDefault(statistic =>
                statistic.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            
            if(stat is null)
            {
                GD.Print(name + " statistic not found!");
                return;
            }

            if (this.Points - delta >= 0)
            {
                this.Points -= delta;
                stat.ModifyValue(delta);
                this.SetChildPoints();
                this.EmitSignal("StatisticChanged", name, delta, newValue);
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