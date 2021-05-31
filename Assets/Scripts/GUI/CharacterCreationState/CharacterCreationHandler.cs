using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using JoyGodot.Assets.Scripts.GUI.CharacterCreationState;
using JoyGodot.Assets.Scripts.GUI.Managed_Assets;
using JoyLib.Code.Cultures;
using JoyLib.Code.Entities;
using JoyLib.Code.Entities.Abilities;
using JoyLib.Code.Entities.AI.Drivers;
using JoyLib.Code.Entities.Statistics;
using JoyLib.Code.Events;
using JoyLib.Code.Graphics;
using JoyLib.Code.Rollers;

namespace JoyLib.Code.Unity.GUI.CharacterCreationState
{
    public class CharacterCreationHandler : GUIData
    {
        protected LineEdit PlayerName { get; set; }

        protected ManagedUIElement PlayerSprite { get; set; }

        protected ICultureHandler CultureHandler { get; set; }

        protected IEntityTemplateHandler EntityTemplateHandler { get; set; }

        protected IObjectIconHandler IconHandler { get; set; }

        protected IEntitySkillHandler SkillHandler { get; set; }

        protected IAbilityHandler AbilityHandler { get; set; }

        protected IEntityFactory EntityFactory { get; set; }

        protected IRollable Roller { get; set; }

        protected const int STATISTIC_POINTS_MAX = 8;
        protected const int DERIVED_VALUE_POINTS_MAX = 10;
        protected const int SKILL_POINTS_MAX = 10;
        protected const int ABILITY_PICKS_MAX = 2;

        protected Control Part1 { get; set; }
        protected Control Part2 { get; set; }

        protected BasicPlayerInfo BasicPlayerInfo { get; set; }
        protected StatisticsList StatisticsList { get; set; }
        protected DerivedValuesList DerivedValuesList { get; set; }
        protected SkillsList SkillsList { get; set; }
        protected AbilityList AbilityList { get; set; }

        protected IEntity Player { get; set; }
        public event PlayerCreatedHandler PlayerCreated;

        public override void _Ready()
        {
            this.Part1 = this.GetNode<Control>("Character Creation Part 1");
            this.Part2 = this.GetNode<Control>("Character Creation Part 2");

            this.PlayerName = this.FindNode("Player Name Input") as LineEdit;
            this.PlayerSprite = this.FindNode("Player Icon") as ManagedUIElement;
            this.BasicPlayerInfo = this.FindNode("Basic Player Info") as BasicPlayerInfo;
            this.StatisticsList = this.FindNode("Statistics Container") as StatisticsList;
            this.DerivedValuesList = this.FindNode("Derived Values Container") as DerivedValuesList;
            this.SkillsList = this.FindNode("Skills Container") as SkillsList;
            this.AbilityList = this.FindNode("Abilities Container") as AbilityList;

            this.BasicPlayerInfo.Connect(
                "ValueChanged",
                this,
                nameof(this.BasicPlayerInfoChanged));

            if (this.StatisticsList is null == false)
            {
                this.StatisticsList.Points = STATISTIC_POINTS_MAX;
            }

            if (this.DerivedValuesList is null == false)
            {
                this.DerivedValuesList.Points = DERIVED_VALUE_POINTS_MAX;
            }

            this.EntityTemplateHandler = GlobalConstants.GameManager.EntityTemplateHandler;
            this.CultureHandler = GlobalConstants.GameManager.CultureHandler;
            this.SkillHandler = GlobalConstants.GameManager.SkillHandler;
            this.AbilityHandler = GlobalConstants.GameManager.AbilityHandler;
            this.Roller = GlobalConstants.GameManager.Roller;
            this.IconHandler = GlobalConstants.GameManager.ObjectIconHandler;
            this.EntityFactory = GlobalConstants.GameManager.EntityFactory;

            this.Part2.Visible = false;

            this.CallDeferred("Initialise");
        }

        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("ui_cancel"))
            {
                GlobalConstants.GameManager.GUIManager.CloseAllGUIs();
                GlobalConstants.GameManager.SetNextState(new States.MainMenuState());
            }
        }

        public void RandomiseName()
        {
            var culture = this.BasicPlayerInfo.CurrentCulture;
            string name = culture.GetRandomName(this.BasicPlayerInfo.CurrentGender);

            this.PlayerName.Text = name;
        }

        public void Initialise()
        {
            this.OnCultureChange(this.BasicPlayerInfo.CurrentTemplate);
        }

        protected void OnCultureChange(IEntityTemplate template)
        {
            this.RandomiseName();
            var culture = this.BasicPlayerInfo.CurrentCulture;

            this.GUIManager.SetUIColours(
                culture.BackgroundColours,
                culture.CursorColours,
                culture.FontColours,
                true,
                true,
                1f);
            ISpriteState state = new SpriteState(
                "player",
                this.IconHandler.GetManagedSprites(
                    culture.Tileset,
                    template.CreatureType,
                    "idle").First());

            this.PlayerSprite.Clear();
            this.PlayerSprite.AddSpriteState(state);
            this.PlayerSprite.OverrideAllColours(
                state.SpriteData.GetRandomPartColours(),
                false,
                0f,
                true);

            this.SetUpStatistics(template);
            this.SetUpDerivedValues(template);
            this.SetUpSkills(template);
            this.SetUpAbilities(
                template,
                this.StatisticsList.Statistics,
                this.SkillsList.Skills,
                this.DerivedValuesList.DerivedValues);
        }

        protected void SetUpStatistics(IEntityTemplate template)
        {
            this.StatisticsList.Points = STATISTIC_POINTS_MAX;
            this.StatisticsList.Statistics = template.Statistics.Values;
        }

        protected void SetUpDerivedValues(IEntityTemplate template)
        {
            this.DerivedValuesList.Points = DERIVED_VALUE_POINTS_MAX;
            this.DerivedValuesList.DerivedValues =
                GlobalConstants.GameManager.DerivedValueHandler
                    .GetEntityStandardBlock(template.Statistics.Values)
                    .Values;
        }

        protected void SetUpSkills(IEntityTemplate template)
        {
            this.SkillsList.Points = SKILL_POINTS_MAX;
            var skills = this.SkillHandler.GetDefaultSkillBlock().Values;
            var templateSkills = template.Skills.Values;

            foreach (IEntitySkill skill in skills)
            {
                var found = templateSkills.FirstOrDefault(entitySkill =>
                    entitySkill.Name.Equals(skill.Name, StringComparison.OrdinalIgnoreCase));

                if (found is null)
                {
                    continue;
                }

                skill.ModifyValue(found.Value);
            }

            this.SkillsList.Skills = skills;
        }

        protected void SetUpAbilities(
            IEntityTemplate template,
            ICollection<IEntityStatistic> stats,
            ICollection<IEntitySkill> skills,
            ICollection<IDerivedValue> derivedValues)
        {
            var abilities = this.AbilityHandler.GetAvailableAbilities(
                template,
                stats,
                skills,
                derivedValues);

            this.AbilityList.Points = ABILITY_PICKS_MAX;
            this.AbilityList.Abilities = abilities.ToArray();
        }

        public void BasicPlayerInfoChanged(string name, string newValue)
        {
            if (name.Equals("Template") || name.Equals("Culture"))
            {
                this.OnCultureChange(this.BasicPlayerInfo.CurrentTemplate);
            }
        }

        public void NextScreen()
        {
            this.SetUpAbilities(
                this.BasicPlayerInfo.CurrentTemplate,
                this.StatisticsList.Statistics,
                this.SkillsList.Skills,
                this.DerivedValuesList.DerivedValues);
            
            this.Part2.Visible = true;
            this.Part1.Visible = false;
        }

        public void PreviousScreen()
        {
            this.Part1.Visible = true;
            this.Part2.Visible = false;
        }

        public void CreatePlayer()
        {
            this.Player = this.EntityFactory.CreateFromTemplate(
                this.BasicPlayerInfo.CurrentTemplate,
                Vector2Int.Zero,
                this.PlayerName.Text,
                this.StatisticsList.Statistics.ToDictionary(statistic => statistic.Name, statistic => statistic),
                this.DerivedValuesList.DerivedValues.ToDictionary(value => value.Name, value => value),
                this.SkillsList.Skills.ToDictionary(skill => skill.Name, skill => skill),
                this.AbilityList.Abilities,
                new[] {this.BasicPlayerInfo.CurrentCulture},
                this.BasicPlayerInfo.GenderHandler.Get(this.BasicPlayerInfo.CurrentGender),
                this.BasicPlayerInfo.BioSexHandler.Get(this.BasicPlayerInfo.CurrentSex),
                this.BasicPlayerInfo.SexualityHandler.Get(this.BasicPlayerInfo.CurrentSexuality),
                this.BasicPlayerInfo.RomanceHandler.Get(this.BasicPlayerInfo.CurrentRomance),
                GlobalConstants.GameManager.JobHandler.Get(this.BasicPlayerInfo.CurrentJob),
                null,
                null,
                new PlayerDriver());
            
            this.PlayerCreated?.Invoke(this.Player);
        }
    }
}