using Godot;
using System;
using System.Linq;
using JoyGodot.Assets.Scripts.GUI.Managed_Assets;
using JoyLib.Code;
using JoyLib.Code.Cultures;
using JoyLib.Code.Entities.Needs;
using JoyLib.Code.Graphics;
using JoyLib.Code.Helpers;
using JoyLib.Code.Rollers;
using JoyLib.Code.Scripting;

public class LoadingScreenColour : Node
{
    protected ManagedUIElement Background { get; set; }
    
    protected ICultureHandler CultureHandler { get; set; }
    
    protected IObjectIconHandler IconHandler { get; set; }
    
    protected RNG Roller { get; set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.Background = (ManagedUIElement) this.FindNode("Background");

        GlobalConstants.ActionLog = new ActionLog();

        this.Roller = new RNG();

        this.IconHandler = new ObjectIconHandler(this.Roller);

        this.CultureHandler = new CultureHandler(this.IconHandler);

        ICulture chosenCulture = this.Roller.SelectFromCollection(this.CultureHandler.Cultures);

        this.Background.AddSpriteState(new SpriteState(
            "Background",
            this.IconHandler.GetSprites("Windows", this.Background.ElementName).First()));
        this.Background.OverrideAllColours(chosenCulture.BackgroundColours[this.Background.ElementName]);
    }
    
    public override void _Input(InputEvent inputEvent)
    {
        if (inputEvent.IsActionPressed("ui_accept"))
        {
            ICulture chosenCulture = this.Roller.SelectFromCollection(this.CultureHandler.Cultures);
        
            this.Background.OverrideAllColours(chosenCulture.BackgroundColours[this.Background.ElementName]);
        }
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
