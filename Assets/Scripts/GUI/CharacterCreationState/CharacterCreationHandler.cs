using System;
using System.Linq;
using Godot;
using JoyGodot.Assets.Scripts.GUI.CharacterCreationState;
using JoyGodot.Assets.Scripts.GUI.Managed_Assets;
using JoyLib.Code.Cultures;
using JoyLib.Code.Entities;
using JoyLib.Code.Entities.Statistics;
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
        
        protected IRollable Roller { get; set; }

        protected const int STATISTIC_POINTS_MAX = 8;
        protected const int DERIVED_VALUE_POINTS_MAX = 10;
        
        protected BasicPlayerInfo BasicPlayerInfo { get; set; }
        protected StatisticsList StatisticsList { get; set; }
        protected DerivedValuesList DerivedValuesList { get; set; }

        public override void _Ready()
        {
            this.PlayerName = this.FindNode("Player Name Input") as LineEdit;
            this.PlayerSprite = this.FindNode("Player Icon") as ManagedUIElement;
            this.BasicPlayerInfo = this.FindNode("Basic Player Info") as BasicPlayerInfo;
            this.StatisticsList = this.FindNode("Statistics List") as StatisticsList;
            this.DerivedValuesList = this.FindNode("Derived Values List") as DerivedValuesList;

            this.BasicPlayerInfo?.Connect(
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
            this.Roller = GlobalConstants.GameManager.Roller;
            this.IconHandler = GlobalConstants.GameManager.ObjectIconHandler;
            
            this.CallDeferred("RandomiseName");
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
            GD.Print(nameof(this.RandomiseName));
            this.StatisticsList.Points = STATISTIC_POINTS_MAX;
            this.DerivedValuesList.Points = DERIVED_VALUE_POINTS_MAX;
            
            var culture = this.BasicPlayerInfo.CurrentCulture;
            var template = this.BasicPlayerInfo.CurrentTemplate;

            string name = culture.GetRandomName(this.BasicPlayerInfo.CurrentGender);
            
            this.GUIManager.SetUIColours(
                culture.BackgroundColours,
                culture.CursorColours,
                culture.FontColours,
                true,
                true,
                1f);
            ISpriteState state = new SpriteState(
                "player",
                this.IconHandler.GetSprites(
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
            this.PlayerName.Text = name;
            this.StatisticsList.Statistics = template.Statistics.Values;
            this.DerivedValuesList.DerivedValues =
                GlobalConstants.GameManager.DerivedValueHandler
                    .GetEntityStandardBlock(template.Statistics.Values)
                    .Values;
        }

        public void BasicPlayerInfoChanged(string name, string newValue)
        {
            GD.Print(name + " : " + newValue);
            this.RandomiseName();
        }

        protected void NextScreen()
        {
            GD.Print("Moving to CC2.");
        }
    }
}