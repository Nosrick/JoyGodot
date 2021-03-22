using System.Linq;
using Godot;
using JoyGodot.Assets.Scripts.GUI.Managed_Assets;
using JoyLib.Code;
using JoyLib.Code.Cultures;
using JoyLib.Code.Entities;
using JoyLib.Code.Godot;
using JoyLib.Code.Graphics;
using JoyLib.Code.Rollers;

public class LoadingScreenColour : Node
{
    protected ManagedUIElement Background { get; set; }
    
    protected JoyObjectNode Node { get; set; }
    
    protected RNG Roller { get; set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.CallDeferred("DeferredReady");
    }

    protected void DeferredReady()
    {
        this.Roller = new RNG();
        this.Background = (ManagedUIElement) this.FindNode("Background");
        this.Node = (JoyObjectNode) this.FindNode("JoyObject");

        ICulture chosenCulture = this.Roller.SelectFromCollection(GlobalConstants.GameManager.CultureHandler.Cultures);

        this.Background.AddSpriteState(new SpriteState(
            "Background",
            GlobalConstants.GameManager.ObjectIconHandler.GetSprites("Windows", this.Background.ElementName).First()));
        this.Background.OverrideAllColours(chosenCulture.BackgroundColours[this.Background.ElementName]);

        IEntityTemplate entityTemplate = GlobalConstants.GameManager.EntityTemplateHandler.GetRandom();
        this.Node.AttachJoyObject(
            GlobalConstants.GameManager.EntityFactory.CreateFromTemplate(
                entityTemplate, 
                Vector2Int.Zero));
        
        GlobalConstants.ActionLog.Log("JoyObject children:");
        GlobalConstants.ActionLog.Log(this.Node.GetChildren());
        
        GlobalConstants.ActionLog.Log("Background children:");
        GlobalConstants.ActionLog.Log(this.Background.GetChildren());
    }
    
    public override void _Input(InputEvent inputEvent)
    {
        if (inputEvent.IsActionPressed("ui_accept"))
        {
            ICulture chosenCulture = this.Roller.SelectFromCollection(GlobalConstants.GameManager.CultureHandler.Cultures);
        
            this.Background.OverrideAllColours(chosenCulture.BackgroundColours[this.Background.ElementName]);
            this.Node.Clear();
            this.Node.AttachJoyObject(GlobalConstants.GameManager.EntityFactory.CreateFromTemplate(
                GlobalConstants.GameManager.EntityTemplateHandler.GetRandom(),
                Vector2Int.Zero));
        }
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
