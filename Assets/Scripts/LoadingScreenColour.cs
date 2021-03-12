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

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.Background = this.GetNodeOrNull<NinePatchRect>("Background");
        this.Border = this.GetNodeOrNull<NinePatchRect>("Border");

        GlobalConstants.ActionLog = new ActionLog();

        this.Background.Modulate = new Color(1, 0, 0);
        this.Border.Modulate = new Color(0, 1, 0);

        IObjectIconHandler objectIconHandler = new ObjectIconHandler(new RNG());

        ICultureHandler cultureHandler = new CultureHandler(objectIconHandler);
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
