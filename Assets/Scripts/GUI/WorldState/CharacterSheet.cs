using System.Linq;
using Godot;
using JoyGodot.Assets.Scripts.GUI.Managed_Assets;
using JoyLib.Code;
using JoyLib.Code.Entities;
using JoyLib.Code.Entities.Statistics;
using JoyLib.Code.Graphics;
using JoyLib.Code.Unity.GUI;

public class CharacterSheet : GUIData
{
    protected IEntity Player { get; set; }
    protected VBoxContainer DerivedValueList { get; set; }
    protected VBoxContainer StatisticList { get; set; }
    protected VBoxContainer SkillList { get; set; }
    
    protected ManagedLabel SpeciesAndJob { get; set; }
    protected ManagedLabel Culture { get; set; }
    protected Label PlayerName { get; set; }
    protected ManagedUIElement PlayerIcon { get; set; }
    
    protected PackedScene ValueItemPrefab { get; set; }
    protected PackedScene AbilityItemPrefab { get; set; }
    
    public override void _Ready()
    {
        this.ValueItemPrefab = GD.Load<PackedScene>(
            GlobalConstants.GODOT_ASSETS_FOLDER
            + "Scenes/Parts/Static Int List Item.tscn");

        this.AbilityItemPrefab = GD.Load<PackedScene>(
            GlobalConstants.GODOT_ASSETS_FOLDER
            + "Scenes/Parts/ManagedLabel.tscn");
        
        this.PlayerName = this.FindNode("Player Name") as Label;
        this.SpeciesAndJob = this.FindNode("Player Species and Job") as ManagedLabel;
        this.Culture = this.FindNode("Culture Label") as ManagedLabel;
        this.PlayerIcon = this.FindNode("Player Icon") as ManagedUIElement;

        this.DerivedValueList = this.FindNode("Derived Value List") as VBoxContainer;
        this.StatisticList = this.FindNode("Statistics List") as VBoxContainer;
        this.SkillList = this.FindNode("Skills List") as VBoxContainer;

        this.RefreshPlayer();
    }

    public void RefreshPlayer()
    {
        this.Player = GlobalConstants.GameManager.Player;

        this.PlayerName.Text = this.Player.JoyName;
        this.SpeciesAndJob.Text = this.Player.CreatureType + " " + this.Player.CurrentJob.Name;
        this.Culture.Text = this.Player.Cultures.First().CultureName;
        this.PlayerIcon.Clear();
        ISpriteState spriteState = this.Player.States.First();
        this.PlayerIcon.AddSpriteState(spriteState);
        this.PlayerIcon.OverrideAllColours(spriteState.SpriteData.GetCurrentPartColours());
    }

    protected void SetUpUi()
    {
        if (this.Player is null)
        {
            return;
        }
        
        var derivedValues = this.Player.DerivedValues.Values.ToList();
        var statistics = this.Player.Statistics.Values.ToList();
        var skills = this.Player.Skills.Values.ToList();

        bool addedChildren = false;
        
        if (this.DerivedValueList.GetChildCount() < derivedValues.Count)
        {
            for (int i = this.DerivedValueList.GetChildCount(); i < derivedValues.Count; i++)
            {
                var instance = this.ValueItemPrefab.Instance();
                this.DerivedValueList.AddChild(instance);
            }

            addedChildren = true;
        }

        if (this.StatisticList.GetChildCount() < statistics.Count)
        {
            for (int i = this.StatisticList.GetChildCount(); i < statistics.Count; i++)
            {
                var instance = this.ValueItemPrefab.Instance();
                this.StatisticList.AddChild(instance);
            }

            addedChildren = true;
        }
        
        if (this.SkillList.GetChildCount() < skills.Count)
        {
            for (int i = this.SkillList.GetChildCount(); i < skills.Count; i++)
            {
                var instance = this.ValueItemPrefab.Instance();
                this.SkillList.AddChild(instance);
            }

            addedChildren = true;
        }
        
        for (int i = 0; i < derivedValues.Count; i++)
        {
            var child = this.DerivedValueList.GetChild(i) as StaticValueItem;
            IDerivedValue derivedValue = derivedValues[i];
            child.ValueName = derivedValue.Name;
            child.Value = derivedValue.Value + "/" + derivedValue.Maximum;
        }
        
        for (int i = 0; i < statistics.Count; i++)
        {
            var child = this.StatisticList.GetChild(i) as StaticValueItem;
            IEntityStatistic statistic = statistics[i];
            child.ValueName = statistic.Name;
            child.Value = statistic.Value.ToString();
        }
        
        for (int i = 0; i < skills.Count; i++)
        {
            var child = this.SkillList.GetChild(i) as StaticValueItem;
            IEntitySkill skill = skills[i];
            child.ValueName = skill.Name;
            child.Value = skill.Value.ToString();
        }

        if (addedChildren)
        {
            this.GUIManager.SetupManagedComponents(this);
        }
    }

    public override void Display()
    {
        base.Display();
        this.SetUpUi();
    }
}
