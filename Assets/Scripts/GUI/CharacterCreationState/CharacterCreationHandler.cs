﻿using System;
using System.Linq;
using Godot;
using JoyGodot.Assets.Scripts.GUI.Managed_Assets;
using JoyLib.Code.Cultures;
using JoyLib.Code.Graphics;
using JoyLib.Code.Rollers;

namespace JoyLib.Code.Unity.GUI.CharacterCreationState
{
    public class CharacterCreationHandler : GUIData
    {
        protected LineEdit PlayerName { get; set; }
        
        protected ManagedUIElement PlayerSprite { get; set; }
        
        protected ICultureHandler CultureHandler { get; set; }
        
        protected IObjectIconHandler IconHandler { get; set; }
        
        protected IRollable Roller { get; set; }

        public override void _Ready()
        {
            this.PlayerName = this.FindNode("Player Name Input") as LineEdit;
            this.PlayerSprite = this.FindNode("Player Icon") as ManagedUIElement;

            this.CultureHandler = GlobalConstants.GameManager.CultureHandler;
            this.Roller = GlobalConstants.GameManager.Roller;
            this.IconHandler = GlobalConstants.GameManager.ObjectIconHandler;
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
            var culture = this.Roller.SelectFromCollection(this.CultureHandler.Values);

            string name = culture.GetRandomName("male");
            
            this.GUIManager.SetUIColours(
                culture.BackgroundColours,
                culture.CursorColours,
                culture.FontColours);
            Button button = new Button();

            ISpriteState state = new SpriteState(
                "player",
                this.IconHandler.GetSprites(
                    culture.Tileset,
                    this.Roller.SelectFromCollection(culture.Inhabitants),
                    "idle").First());
            
            this.PlayerSprite.Clear();
            this.PlayerSprite.AddSpriteState(state);
            this.PlayerSprite.OverrideAllColours(state.SpriteData.GetRandomPartColours());
            this.PlayerName.Text = name;
        }
    }
}