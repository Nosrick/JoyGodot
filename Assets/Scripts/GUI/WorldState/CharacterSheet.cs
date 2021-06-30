using System.Collections.Generic;
using System.Linq;
using Godot;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Abilities;
using JoyGodot.Assets.Scripts.Entities.Statistics;
using JoyGodot.Assets.Scripts.GUI.Tools;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.Managed_Assets;

namespace JoyGodot.Assets.Scripts.GUI.WorldState
{
    public class CharacterSheet : GUIData
    {
        protected IEntity Player { get; set; }
        protected VBoxContainer DerivedValueList { get; set; }
        protected VBoxContainer StatisticList { get; set; }
        protected VBoxContainer SkillList { get; set; }
        protected VBoxContainer AbilityList { get; set; }
        protected VBoxContainer MiscStatisticList { get; set; }

        protected ManagedLabel SpeciesAndJobOne { get; set; }
        protected ManagedLabel SpeciesAndJobTwo { get; set; }
        protected ManagedLabel CultureNameOne { get; set; }
        protected ManagedLabel CultureNameTwo { get; set; }
        protected Label PlayerNameOne { get; set; }
        protected Label PlayerNameTwo { get; set; }
        protected ManagedUIElement PlayerIconOne { get; set; }
        protected ManagedUIElement PlayerIconTwo { get; set; }

        protected PackedScene ValueItemPrefab { get; set; }
        protected PackedScene AbilityItemPrefab { get; set; }

        protected Control PageOne { get; set; }
        protected Control PageTwo { get; set; }

        public override void _Ready()
        {
            this.GetPrefabs();
            this.PageOne = this.GetNode<Control>("Page 1");
            this.PageTwo = this.GetNode<Control>("Page 2");

            this.PlayerNameOne = this.PageOne.FindNode("Player Name") as Label;
            this.SpeciesAndJobOne = this.PageOne.FindNode("Player Species and Job") as ManagedLabel;
            this.CultureNameOne = this.PageOne.FindNode("Culture Label") as ManagedLabel;
            this.PlayerIconOne = this.PageOne.FindNode("Player Icon") as ManagedUIElement;

            this.PlayerNameTwo = this.PageTwo.FindNode("Player Name") as Label;
            this.SpeciesAndJobTwo = this.PageTwo.FindNode("Player Species and Job") as ManagedLabel;
            this.CultureNameTwo = this.PageTwo.FindNode("Culture Label") as ManagedLabel;
            this.PlayerIconTwo = this.PageTwo.FindNode("Player Icon") as ManagedUIElement;

            this.DerivedValueList = this.PageOne.FindNode("Derived Value List") as VBoxContainer;
            this.StatisticList = this.PageOne.FindNode("Statistics List") as VBoxContainer;
            this.SkillList = this.PageOne.FindNode("Skills List") as VBoxContainer;

            this.AbilityList = this.PageTwo.FindNode("Ability List") as VBoxContainer;
            this.MiscStatisticList = this.PageTwo.FindNode("Misc Statistics List") as VBoxContainer;

            this.RefreshPlayer();
        }

        protected virtual void GetPrefabs()
        {
            this.ValueItemPrefab = GD.Load<PackedScene>(
                GlobalConstants.GODOT_ASSETS_FOLDER
                + "Scenes/Parts/Static Int List Item.tscn");

            this.AbilityItemPrefab = GD.Load<PackedScene>(
                GlobalConstants.GODOT_ASSETS_FOLDER
                + "Scenes/Parts/ManagedLabel.tscn");
        }

        public virtual void RefreshPlayer()
        {
            this.Player = GlobalConstants.GameManager.Player;

            this.PlayerNameOne.Text = this.Player.JoyName;
            this.SpeciesAndJobOne.Text = this.Player.CreatureType + " " + this.Player.CurrentJob.Name;
            this.SpeciesAndJobOne.Tooltip = new List<string>
            {
                GlobalConstants.GameManager.EntityTemplateHandler.Get(this.Player.CreatureType).Description,
                this.Player.CurrentJob.Description
            };
            this.CultureNameOne.Text = this.Player.Cultures.First().CultureName;
            this.CultureNameOne.Tooltip = new List<string>
            {
                this.Player.Cultures.First().Description
            };

            this.PlayerNameTwo.Text = this.PlayerNameOne.Text;
            this.SpeciesAndJobTwo.Text = this.SpeciesAndJobOne.Text;
            this.SpeciesAndJobTwo.Tooltip = new List<string>
            {
                GlobalConstants.GameManager.EntityTemplateHandler.Get(this.Player.CreatureType).Description,
                this.Player.CurrentJob.Description
            };
            this.CultureNameTwo.Text = this.CultureNameOne.Text;
            this.CultureNameTwo.Tooltip = new List<string>
            {
                this.Player.Cultures.First().Description
            };

            ISpriteState spriteState = this.Player.States.First();

            this.PlayerIconOne.Clear();
            this.PlayerIconOne.AddSpriteState(spriteState);
            this.PlayerIconOne.OverrideAllColours(spriteState.SpriteData.GetCurrentPartColours());

            this.PlayerIconTwo.Clear();
            this.PlayerIconTwo.AddSpriteState(spriteState);
            this.PlayerIconTwo.OverrideAllColours(spriteState.SpriteData.GetCurrentPartColours());
        }

        protected virtual void SetUpUi()
        {
            if (this.Player is null)
            {
                return;
            }

            var derivedValues = this.Player.DerivedValues.Values.ToList();
            var statistics = this.Player.Statistics.Values.ToList();
            var skills = this.Player.Skills.Values.ToList();
            var abilities = this.Player.Abilities;
            var data = this.Player.GetData(
                    new string[]
                    {
                        "gender",
                        "biosex",
                        "sexuality",
                        "romance"
                    })
                .ToList();

            bool addedChildren = false;

            if (this.DerivedValueList.GetChildCount() < derivedValues.Count)
            {
                for (int i = this.DerivedValueList.GetChildCount(); i < derivedValues.Count; i++)
                {
                    var instance = this.ValueItemPrefab.Instance() as StaticValueItem;
                    instance.TitleCase = true;
                    this.DerivedValueList.AddChild(instance);
                }

                addedChildren = true;
            }

            if (this.StatisticList.GetChildCount() < statistics.Count)
            {
                for (int i = this.StatisticList.GetChildCount(); i < statistics.Count; i++)
                {
                    var instance = this.ValueItemPrefab.Instance() as StaticValueItem;
                    instance.TitleCase = true;
                    this.StatisticList.AddChild(instance);
                }

                addedChildren = true;
            }

            if (this.SkillList.GetChildCount() < skills.Count)
            {
                for (int i = this.SkillList.GetChildCount(); i < skills.Count; i++)
                {
                    var instance = this.ValueItemPrefab.Instance() as StaticValueItem;
                    instance.TitleCase = true;
                    this.SkillList.AddChild(instance);
                }

                addedChildren = true;
            }

            if (this.AbilityList.GetChildCount() < abilities.Count)
            {
                for (int i = this.AbilityList.GetChildCount(); i < abilities.Count; i++)
                {
                    var instance = this.AbilityItemPrefab.Instance() as ManagedLabel;
                    instance.ElementName = "AccentBackground";
                    instance.RectMinSize = new Vector2(0, 24);
                    instance.TitleCase = true;
                    this.AbilityList.AddChild(instance);
                }

                addedChildren = true;
            }

            if (this.MiscStatisticList.GetChildCount() < data.Count)
            {
                for (int i = this.MiscStatisticList.GetChildCount(); i < data.Count; i++)
                {
                    var instance = this.AbilityItemPrefab.Instance() as ManagedLabel;
                    instance.ElementName = "AccentBackground";
                    instance.RectMinSize = new Vector2(0, 24);
                    instance.TitleCase = true;
                    this.MiscStatisticList.AddChild(instance);
                }
            }

            for (int i = 0; i < derivedValues.Count; i++)
            {
                var child = this.DerivedValueList.GetChild(i) as StaticValueItem;
                IDerivedValue derivedValue = derivedValues[i];
                child.ValueName = derivedValue.Name;
                child.Value = derivedValue.Value + "/" + derivedValue.Maximum;
                child.Tooltip = derivedValue.Tooltip;
            }

            for (int i = 0; i < statistics.Count; i++)
            {
                var child = this.StatisticList.GetChild(i) as StaticValueItem;
                IEntityStatistic statistic = statistics[i];
                child.ValueName = statistic.Name;
                child.Value = statistic.Value.ToString();
                child.Tooltip = statistic.Tooltip;
            }

            for (int i = 0; i < skills.Count; i++)
            {
                var child = this.SkillList.GetChild(i) as StaticValueItem;
                IEntitySkill skill = skills[i];
                child.ValueName = skill.Name;
                child.Value = skill.Value.ToString();
                child.Tooltip = skill.Tooltip;
            }

            for (int i = 0; i < abilities.Count; i++)
            {
                var child = this.AbilityList.GetChild(i) as ManagedLabel;
                IAbility ability = abilities[i];
                child.Name = ability.Name;
                child.Text = ability.Name;
                child.Tooltip = new List<string> {ability.Description};
            }

            for (int i = 0; i < data.Count; i++)
            {
                var child = this.MiscStatisticList.GetChild(i) as ManagedLabel;
                child.Name = data[i].Item1;
                child.Text = data[i].CombineToString();
            }

            if (addedChildren)
            {
                this.GUIManager.SetupManagedComponents(this);
            }

            this.OpenPageOne();
        }

        public override void Display()
        {
            base.Display();
            this.SetUpUi();
        }

        public virtual void OpenPageOne()
        {
            this.PageOne.Visible = true;
            this.PageTwo.Visible = false;
        }

        public virtual void OpenPageTwo()
        {
            this.PageOne.Visible = false;
            this.PageTwo.Visible = true;
        }
    }
}