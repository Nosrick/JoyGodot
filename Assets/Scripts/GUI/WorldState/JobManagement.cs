using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Abilities;
using JoyGodot.Assets.Scripts.Entities.Jobs;
using JoyGodot.Assets.Scripts.Entities.Statistics;
using JoyGodot.Assets.Scripts.GUI.Tools;
using JoyGodot.Assets.Scripts.Managed_Assets;

namespace JoyGodot.Assets.Scripts.GUI.WorldState
{
    public class JobManagement : GUIData
    {
        protected IEntity Player { get; set; }
        protected VBoxContainer DerivedValueList { get; set; }
        protected VBoxContainer StatisticList { get; set; }
        protected VBoxContainer SkillList { get; set; }
        protected VBoxContainer AbilityList { get; set; }
        protected VBoxContainer MiscStatisticList { get; set; }

        protected StringValueItem SpeciesAndJobOne { get; set; }
        protected StringValueItem SpeciesAndJobTwo { get; set; }
        protected ManagedLabel PointsRemainingOne { get; set; }
        protected ManagedLabel PointsRemainingTwo { get; set; }
        protected Label PlayerNameOne { get; set; }
        protected Label PlayerNameTwo { get; set; }
        protected ManagedUIElement PlayerIconOne { get; set; }
        protected ManagedUIElement PlayerIconTwo { get; set; }

        protected PackedScene ValueItemPrefab { get; set; }
        protected PackedScene AbilityItemPrefab { get; set; }
        protected PackedScene StringItemPrefab { get; set; }

        protected Control PageOne { get; set; }
        protected Control PageTwo { get; set; }

        protected int JobPoints { get; set; }
        protected IJob CurrentJob { get; set; }

        protected IDictionary<string, int> NewSkillValues { get; set; }
        protected IDictionary<string, int> NewStatisticValues { get; set; }
        protected IDictionary<string, int> NewDerivedValues { get; set; }
        protected List<string> PurchasedAbilities { get; set; }

        public override void _Ready()
        {
            this.GetPrefabs();
            this.PageOne = this.GetNode<Control>("Page 1");
            this.PageTwo = this.GetNode<Control>("Page 2");

            this.PurchasedAbilities = new List<string>();
            this.NewDerivedValues = new Dictionary<string, int>();
            this.NewSkillValues = new Dictionary<string, int>();
            this.NewStatisticValues = new Dictionary<string, int>();

            this.PlayerNameOne = this.PageOne.FindNode("Player Name") as Label;
            this.SpeciesAndJobOne = this.PageOne.FindNode("Job Selector") as StringValueItem;
            this.PointsRemainingOne = this.PageOne.FindNode("Points Remaining Label") as ManagedLabel;
            this.PlayerIconOne = this.PageOne.FindNode("Player Icon") as ManagedUIElement;

            this.PlayerNameTwo = this.PageTwo.FindNode("Player Name") as Label;
            this.SpeciesAndJobTwo = this.PageTwo.FindNode("Job Selector") as StringValueItem;
            this.PointsRemainingTwo = this.PageTwo.FindNode("Points Remaining Label") as ManagedLabel;
            this.PlayerIconTwo = this.PageTwo.FindNode("Player Icon") as ManagedUIElement;

            this.DerivedValueList = this.PageOne.FindNode("Derived Value List") as VBoxContainer;
            this.StatisticList = this.PageOne.FindNode("Statistics List") as VBoxContainer;
            this.SkillList = this.PageOne.FindNode("Skills List") as VBoxContainer;

            this.AbilityList = this.PageTwo.FindNode("Ability List") as VBoxContainer;
            this.MiscStatisticList = this.PageTwo.FindNode("Misc Statistics List") as VBoxContainer;

            this.RefreshPlayer();
        }

        public virtual void RefreshPlayer()
        {
            this.Player = GlobalConstants.GameManager.Player;

            this.PlayerNameOne.Text = this.Player.JoyName;
            this.CurrentJob = this.Player.CurrentJob;
            this.JobPoints = this.CurrentJob.Experience;
            this.SpeciesAndJobOne.ValueName = "Job";
            this.SpeciesAndJobOne.Values = this.Player.Jobs.Select(job => job.Name).ToArray();
            this.SpeciesAndJobOne.Tooltip = new List<string>
            {
                GlobalConstants.GameManager.EntityTemplateHandler.Get(this.Player.CreatureType).Description,
                this.Player.CurrentJob.Description
            };
            this.PointsRemainingOne.Text = "Points Remaining: " + this.JobPoints;

            this.PlayerNameTwo.Text = this.PlayerNameOne.Text;
            this.SpeciesAndJobTwo.ValueName = "Job";
            this.SpeciesAndJobTwo.Values = this.SpeciesAndJobOne.Values;
            this.SpeciesAndJobTwo.Tooltip = new List<string>
            {
                GlobalConstants.GameManager.EntityTemplateHandler.Get(this.Player.CreatureType).Description,
                this.Player.CurrentJob.Description
            };
            this.PointsRemainingTwo.Text = this.PointsRemainingOne.Text;

            if (this.SpeciesAndJobOne.IsConnected("ValueChanged", this, nameof(this.OnJobChange)))
            {
                this.SpeciesAndJobOne.Disconnect("ValueChanged", this, nameof(this.OnJobChange));
            }

            this.SpeciesAndJobOne.Connect("ValueChanged", this, nameof(this.OnJobChange));

            if (this.SpeciesAndJobTwo.IsConnected("ValueChanged", this, nameof(this.OnJobChange)))
            {
                this.SpeciesAndJobTwo.Disconnect("ValueChanged", this, nameof(this.OnJobChange));
            }

            this.SpeciesAndJobTwo.Connect("ValueChanged", this, nameof(this.OnJobChange));

            ISpriteState spriteState = this.Player.States.First();

            this.PlayerIconOne.Clear();
            this.PlayerIconOne.AddSpriteState(spriteState);
            this.PlayerIconOne.OverrideAllColours(spriteState.SpriteData.GetCurrentPartColours());

            this.PlayerIconTwo.Clear();
            this.PlayerIconTwo.AddSpriteState(spriteState);
            this.PlayerIconTwo.OverrideAllColours(spriteState.SpriteData.GetCurrentPartColours());
        }

        public override void Display()
        {
            base.Display();
            this.SetUpUi();
        }

        public void SetExperienceRemaining()
        {
            this.PointsRemainingOne.Text =
                this.PointsRemainingTwo.Text =
                    "Points Remaining: " + this.JobPoints;
        }

        protected virtual void SetUpUi()
        {
            if (this.Player is null)
            {
                return;
            }

            this.SetExperienceRemaining();

            var derivedValues = this.Player.DerivedValues.Values.ToList();
            var statistics = this.Player.Statistics.Values.ToList();
            var skills = this.Player.Skills.Values.ToList();
            var abilities = this.CurrentJob.Abilities.Where(pair =>
                    this.Player.Abilities.Contains(pair.Key) == false)
                .Select(pair => new Tuple<IAbility, int>(pair.Key, pair.Value))
                .ToList();

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

            if (this.AbilityList.GetChildCount() < abilities.Count)
            {
                for (int i = this.AbilityList.GetChildCount(); i < abilities.Count; i++)
                {
                    var instance = this.AbilityItemPrefab.Instance() as ConstrainedManagedTextButton;
                    instance.ElementName = "AccentBackground";
                    instance.RectMinSize = new Vector2(0, 24);
                    instance.ToggleMode = true;
                    this.AbilityList.AddChild(instance);
                }

                addedChildren = true;
            }

            if (abilities.Count == 0)
            {
                if (this.AbilityList.GetChildCount() == 0)
                {
                    var instance = this.AbilityItemPrefab.Instance() as ConstrainedManagedTextButton;
                    instance.ElementName = "AccentBackground";
                    instance.RectMinSize = new Vector2(0, 24);
                    this.AbilityList.AddChild(instance);
                }

                var item = this.AbilityList.GetChild(0) as ConstrainedManagedTextButton;
                item.Name = "No Abilities Available";
                item.Text = item.Name;
            }

            for (int i = 0; i < derivedValues.Count; i++)
            {
                var child = this.DerivedValueList.GetChild(i) as IntValueItem;
                IDerivedValue derivedValue = derivedValues[i];
                child.ValueName = derivedValue.Name;
                child.Value = derivedValue.Maximum;
                child.Minimum = derivedValue.Maximum;
                child.Maximum = child.Minimum + 50;
                child.UseRestriction = true;
                child.IncreaseCost = 10;
                child.DecreaseCost = -10;
                if (child.IsConnected("ValueChanged", this, nameof(this.OnDerivedValueChange)))
                {
                    child.Disconnect("ValueChanged", this, nameof(this.OnDerivedValueChange));
                }

                child.Connect("ValueChanged", this, nameof(this.OnDerivedValueChange));
            }

            for (int i = 0; i < statistics.Count; i++)
            {
                var child = this.StatisticList.GetChild(i) as IntValueItem;
                IEntityStatistic statistic = statistics[i];
                child.ValueName = statistic.Name;
                child.Value = statistic.Value;
                child.Minimum = statistic.Value;
                child.Maximum = 10;
                child.UseRestriction = true;
                child.Tooltip = new List<string>(statistic.Tooltip)
                {
                    "Cost: " + child.IncreaseCost
                };
                if (child.IsConnected("ValueChanged", this, nameof(this.OnStatisticChange)))
                {
                    child.Disconnect("ValueChanged", this, nameof(this.OnStatisticChange));
                }

                child.Connect("ValueChanged", this, nameof(this.OnStatisticChange));
            }

            for (int i = 0; i < skills.Count; i++)
            {
                var child = this.SkillList.GetChild(i) as IntValueItem;
                IEntitySkill skill = skills[i];
                child.ValueName = skill.Name;
                child.Value = skill.Value;
                child.Minimum = skill.Value;
                child.Maximum = 10;
                child.UseRestriction = true;
                child.Tooltip = new List<string>(skill.Tooltip)
                {
                    "Cost: " + child.IncreaseCost
                };
                if (child.IsConnected("ValueChanged", this, nameof(this.OnSkillChange)))
                {
                    child.Disconnect("ValueChanged", this, nameof(this.OnSkillChange));
                }

                child.Connect("ValueChanged", this, nameof(this.OnSkillChange));
            }

            for (int i = 0; i < abilities.Count; i++)
            {
                var child = this.AbilityList.GetChild(i) as ConstrainedManagedTextButton;
                IAbility ability = abilities[i].Item1;
                child.Name = ability.Name;
                child.Text = ability.Name;
                child.IncreaseCost = abilities[i].Item2;
                child.DecreaseCost = -abilities[i].Item2;
                child.UseRestriction = true;
                child.Tooltip = new List<string>
                {
                    ability.Description,
                    "Cost: " + child.IncreaseCost
                };
                if (child.IsConnected("ValuePress", this, nameof(this.OnAbilityChange)))
                {
                    child.Disconnect("ValuePress", this, nameof(this.OnAbilityChange));
                }

                child.Connect("ValuePress", this, nameof(this.OnAbilityChange));
            }

            if (addedChildren)
            {
                this.GUIManager.SetupManagedComponents(this);
            }

            this.SetChildPoints();

            this.OpenPageOne();
        }

        public void OnStatisticChange(string name, int delta, int newValue)
        {
            this.JobPoints -= delta;
            if (this.NewStatisticValues.ContainsKey(name))
            {
                this.NewStatisticValues[name] = newValue;
            }
            else
            {
                this.NewStatisticValues.Add(name, newValue);
            }

            this.SetChildPoints();
            this.SetExperienceRemaining();
        }

        public void OnSkillChange(string name, int delta, int newValue)
        {
            this.JobPoints -= delta;
            if (this.NewSkillValues.ContainsKey(name))
            {
                this.NewSkillValues[name] = newValue;
            }
            else
            {
                this.NewSkillValues.Add(name, newValue);
            }

            this.SetChildPoints();
            this.SetExperienceRemaining();
        }

        public void OnDerivedValueChange(string name, int delta, int newValue)
        {
            this.JobPoints -= delta;
            if (this.NewDerivedValues.ContainsKey(name))
            {
                this.NewDerivedValues[name] = newValue;
            }
            else
            {
                this.NewDerivedValues.Add(name, newValue);
            }

            this.SetChildPoints();
            this.SetExperienceRemaining();
        }

        public void OnAbilityChange(string name, int delta, bool newValue)
        {
            this.JobPoints -= delta;
            this.SetChildPoints();
            this.SetExperienceRemaining();
            if (newValue)
            {
                this.PurchasedAbilities.Add(name);
            }
            else
            {
                this.PurchasedAbilities.Remove(name);
            }

            this.SetChildPoints();
            this.SetExperienceRemaining();
        }

        public void OnJobChange(string name, int delta, string newValue)
        {
            this.CurrentJob = this.Player.Jobs.FirstOrDefault(
                job => job.Name.Equals(newValue, StringComparison.OrdinalIgnoreCase));

            if (this.CurrentJob is null)
            {
                GD.PushWarning("Could not find job " + newValue + " on player!");
                return;
            }

            this.JobPoints = this.CurrentJob.Experience;
            this.SpeciesAndJobOne.Index =
                this.SpeciesAndJobTwo.Index =
                    this.SpeciesAndJobOne.Values.ToList().IndexOf(newValue);
            
            this.SpeciesAndJobOne.Tooltip =
                this.SpeciesAndJobTwo.Tooltip =
                    new List<string>
                    {
                        GlobalConstants.GameManager.EntityTemplateHandler.Get(this.Player.CreatureType).Description,
                        this.Player.CurrentJob.Description
                    };

            this.SetUpUi();
        }

        public void MakeChanges()
        {
            var job = this.Player.Jobs.FirstOrDefault(job => job.Name.Equals(this.SpeciesAndJobOne.Value));
            if (job is null)
            {
                GD.PushWarning("Could not find current job on player!");
                return;
            }

            job.SpendExperience(job.Experience - this.JobPoints);
            List<IAbility> abilities = new List<IAbility>();
            foreach (string name in this.PurchasedAbilities)
            {
                abilities.Add(GlobalConstants.GameManager.AbilityHandler.Get(name));
            }

            this.Player.Abilities.AddRange(abilities);

            foreach (KeyValuePair<string, int> pair in this.NewDerivedValues)
            {
                var oldEnhancement = this.Player.DerivedValues[pair.Key].Enhancement;
                this.Player.SetEnhancement(pair.Key, pair.Value - oldEnhancement);
            }

            foreach (KeyValuePair<string, int> pair in this.NewSkillValues)
            {
                this.Player.SetValue(pair.Key, pair.Value);
            }

            foreach (KeyValuePair<string, int> pair in this.NewStatisticValues)
            {
                this.Player.SetValue(pair.Key, pair.Value);
            }

            this.SetUpUi();
        }

        protected void SetChildPoints()
        {
            var derivedValues = this.Player.DerivedValues.Values.ToList();
            var statistics = this.Player.Statistics.Values.ToList();
            var skills = this.Player.Skills.Values.ToList();
            var abilities = this.CurrentJob.Abilities.Where(pair =>
                    this.Player.Abilities.Contains(pair.Key) == false)
                .Select(pair => new Tuple<IAbility, int>(pair.Key, pair.Value))
                .ToList();

            for (int i = 0; i < this.StatisticList.GetChildCount(); i++)
            {
                var child = this.StatisticList.GetChild(i) as IntValueItem;
                if (child?.Visible != true)
                {
                    continue;
                }

                child.PointRestriction = this.JobPoints;

                int delta = (child.Value * 10) - this.CurrentJob.GetStatisticDiscount(child.ValueName);
                child.IncreaseCost = delta + 10;
                child.DecreaseCost = -delta;
                child.Tooltip = new List<string>(statistics[i].Tooltip)
                {
                    "Cost: " + (delta + 10)
                };
            }

            for (int i = 0; i < this.SkillList.GetChildCount(); i++)
            {
                var child = this.SkillList.GetChild(i) as IntValueItem;
                if (child?.Visible != true)
                {
                    continue;
                }

                child.PointRestriction = this.JobPoints;

                int delta = (child.Value * 10) - this.CurrentJob.GetSkillDiscount(child.ValueName);
                child.IncreaseCost = delta + 10;
                child.DecreaseCost = -delta;
                child.Tooltip = new List<string>(skills[i].Tooltip)
                {
                    "Cost: " + (delta + 10)
                };
            }

            for (int i = 0; i < this.DerivedValueList.GetChildCount(); i++)
            {
                var child = this.DerivedValueList.GetChild(i) as IntValueItem;
                if (child?.Visible != true)
                {
                    continue;
                }

                child.Tooltip = new List<string>(derivedValues[i].Tooltip)
                {
                    "Cost: " + child.IncreaseCost
                };

                child.PointRestriction = this.JobPoints;
            }

            for (int i = 0; i < this.AbilityList.GetChildCount(); i++)
            {
                var child = this.AbilityList.GetChild(i) as ConstrainedManagedTextButton;
                if (child?.Visible != true)
                {
                    continue;
                }

                child.PointRestriction = this.JobPoints;
                child.Tooltip = new[]
                {
                    abilities[i].Item1.Description,
                    "Cost: " + child.IncreaseCost
                };
            }
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

        protected virtual void GetPrefabs()
        {
            this.AbilityItemPrefab = GD.Load<PackedScene>(
                GlobalConstants.GODOT_ASSETS_FOLDER
                + "Scenes/Parts/ConstrainedManagedTextButton.tscn");
            this.ValueItemPrefab = GD.Load<PackedScene>(
                GlobalConstants.GODOT_ASSETS_FOLDER
                + "Scenes/Parts/Int List Item.tscn");
            this.StringItemPrefab = GD.Load<PackedScene>(
                GlobalConstants.GODOT_ASSETS_FOLDER
                + "Scenes/Parts/String List Item.tscn");
        }
    }
}