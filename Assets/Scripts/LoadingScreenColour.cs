using Godot;
using System;
using JoyLib.Code;
using JoyLib.Code.Cultures;
using JoyLib.Code.Entities.Needs;
using JoyLib.Code.Graphics;
using JoyLib.Code.Helpers;
using JoyLib.Code.Rollers;
using JoyLib.Code.Scripting;

public class LoadingScreenColour : Node
{
    protected NinePatchRect Background { get; set; }
    protected NinePatchRect Border { get; set; }
    
    protected ICultureHandler CultureHandler { get; set; }
    
    protected IObjectIconHandler IconHandler { get; set; }
    
    protected RNG Roller { get; set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.Background = this.GetNodeOrNull<NinePatchRect>("Background");
        this.Border = this.GetNodeOrNull<NinePatchRect>("Border");

        GlobalConstants.ActionLog = new ActionLog();

        this.Roller = new RNG();

        this.IconHandler = new ObjectIconHandler(this.Roller);

        this.CultureHandler = new CultureHandler(this.IconHandler);

        ICulture chosenCulture = this.Roller.SelectFromCollection(this.CultureHandler.Cultures);
        
        this.Background.Modulate = chosenCulture.BackgroundColours["DefaultWindow"]["background"];
        this.Border.Modulate = chosenCulture.BackgroundColours["DefaultWindow"]["border"];
    }
    
    public override void _Input(InputEvent inputEvent)
    {
        if (inputEvent.IsActionPressed("ui_accept"))
        {
            ICulture chosenCulture = this.Roller.SelectFromCollection(this.CultureHandler.Cultures);
        
            this.Background.Modulate = chosenCulture.BackgroundColours["DefaultWindow"]["background"];
            this.Border.Modulate = chosenCulture.BackgroundColours["DefaultWindow"]["border"];
        }
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
