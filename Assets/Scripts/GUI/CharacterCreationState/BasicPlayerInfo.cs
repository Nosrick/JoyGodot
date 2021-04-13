using Godot;
using System;
using System.Collections.Generic;
using JoyLib.Code;
using JoyLib.Code.Entities.Statistics;
using JoyLib.Code.Helpers;
using JoyLib.Code.Unity.GUI;

public class BasicPlayerInfo : Control
{
    public List<IEntityStatistic> Statistics
    {
        get => this.m_Statistics;
        set
        {
            this.RepaintStatisticList();
            this.m_Statistics = value;
        }
    }

    protected List<IEntityStatistic> m_Statistics;
    
    protected PackedScene ListItemPrefab { get; set; }
    
    protected List<StringValueItem> Parts { get; set; }

    [Signal]
    public delegate void StatisticBlockSet();

    [Signal]
    public delegate void StatisticChanged(string name, int newValue);

    public override void _Ready()
    {
        this.Parts = new List<StringValueItem>();
        this.ListItemPrefab = GD.Load<PackedScene>("Scenes/Parts/String List Item.tscn");
    }

    protected void RepaintStatisticList()
    {
        if (this.Statistics.Count > this.Parts.Count)
        {
            for (int i = this.Parts.Count; i < this.Statistics.Count; i++)
            {
                var instance = this.ListItemPrefab.Instance() as StringValueItem;
                if (instance is null)
                {
                    GlobalConstants.ActionLog.Log("List item prefab is incorrect!", LogLevel.Error);
                    return;
                }
                instance.Visible = false;
                this.AddChild(instance);
                this.Parts.Add(instance);
            }
        }
        
        for(int i = 0; i < this.Statistics.Count; i++)
        {
            
        }
        
        this.EmitSignal("StatisticBlockSet");
    }
}
