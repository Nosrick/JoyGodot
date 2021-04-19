using System.Collections.Generic;
using System.Linq;
using Godot;
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
            }
        }

        protected List<IEntityStatistic> m_Statistics;
        
        protected List<IntValueItem> Parts { get; set; }
        
        protected PackedScene PartPrefab { get; set; }

        public override void _EnterTree()
        {
            this.Parts = new List<IntValueItem>();
            this.PartPrefab = GD.Load<PackedScene>(
                GlobalConstants.GODOT_ASSETS_FOLDER +
                "Scenes/Parts/Int List Item.tscn");
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
                    this.AddChild(instance);
                    this.Parts.Add(instance);
                }
            }
            this.DeferredSetUp(this.m_Statistics);
        }

        protected void DeferredSetUp(List<IEntityStatistic> statistics)
        {
            for (int i = 0; i < statistics.Count; i++)
            {
                var stat = statistics[i];
                var part = this.Parts[i];
                part.ValueName = stat.Name;
                part.Minimum = 1;
                part.Maximum = 10;
                part.Value = stat.Value;
                part.Visible = true;
            }
        }
    }
}