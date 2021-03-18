using System.Linq;
using Godot;
using JoyGodot.Assets.Scripts.GUI.Managed_Assets;
using JoyLib.Code;
using JoyLib.Code.Cultures;
using JoyLib.Code.Entities;
using JoyLib.Code.Entities.Abilities;
using JoyLib.Code.Entities.AI.LOS.Providers;
using JoyLib.Code.Entities.Gender;
using JoyLib.Code.Entities.Jobs;
using JoyLib.Code.Entities.Needs;
using JoyLib.Code.Entities.Romance;
using JoyLib.Code.Entities.Sexes;
using JoyLib.Code.Entities.Sexuality;
using JoyLib.Code.Entities.Statistics;
using JoyLib.Code.Godot;
using JoyLib.Code.Graphics;
using JoyLib.Code.Helpers;
using JoyLib.Code.Managers;
using JoyLib.Code.Physics;
using JoyLib.Code.Rollers;

public class LoadingScreenColour : Node
{
    protected ManagedUIElement Background { get; set; }
    
    protected JoyObjectNode Node { get; set; }
    
    protected RNG Roller { get; set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.Roller = new RNG();
        this.Background = (ManagedUIElement) this.FindNode("Background");
        this.Node = (JoyObjectNode) this.FindNode("JoyObject");

        ICulture chosenCulture = this.Roller.SelectFromCollection(GlobalConstants.GameManager.CultureHandler.Cultures);

        this.Background.AddSpriteState(new SpriteState(
            "Background",
            GlobalConstants.GameManager.ObjectIconHandler.GetSprites("Windows", this.Background.ElementName).First()));
        this.Background.OverrideAllColours(chosenCulture.BackgroundColours[this.Background.ElementName]);
        
        this.Node.AttachJoyObject(
            GlobalConstants.GameManager.EntityFactory.CreateFromTemplate(
                GlobalConstants.GameManager.EntityTemplateHandler.GetRandom(), 
                Vector2Int.Zero));
    }
    
    public override void _Input(InputEvent inputEvent)
    {
        if (inputEvent.IsActionPressed("ui_accept"))
        {
            ICulture chosenCulture = this.Roller.SelectFromCollection(GlobalConstants.GameManager.CultureHandler.Cultures);
        
            this.Background.OverrideAllColours(chosenCulture.BackgroundColours[this.Background.ElementName]);
        }
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
