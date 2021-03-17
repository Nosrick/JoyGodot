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
    
    protected ICultureHandler CultureHandler { get; set; }
    
    protected IObjectIconHandler IconHandler { get; set; }
    
    protected IEntityFactory EntityFactory { get; set; }
    
    protected RNG Roller { get; set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.Background = (ManagedUIElement) this.FindNode("Background");
        this.Node = (JoyObjectNode) this.FindNode("JoyObject");

        GlobalConstants.ActionLog = new ActionLog();

        this.Roller = new RNG();

        this.IconHandler = new ObjectIconHandler(this.Roller);

        this.CultureHandler = new CultureHandler(this.IconHandler);

        ICulture chosenCulture = this.Roller.SelectFromCollection(this.CultureHandler.Cultures);

        this.Background.AddSpriteState(new SpriteState(
            "Background",
            this.IconHandler.GetSprites("Windows", this.Background.ElementName).First()));
        this.Background.OverrideAllColours(chosenCulture.BackgroundColours[this.Background.ElementName]);

        IEntityStatisticHandler statisticHandler = new EntityStatisticHandler();
        IEntitySkillHandler skillHandler = new EntitySkillHandler();
        IAbilityHandler abilityHandler = new AbilityHandler();
        IEntityTemplateHandler templateHandler = new EntityTemplateHandler(skillHandler, new VisionProviderHandler(), abilityHandler);
        
        this.EntityFactory = new EntityFactory(
            new GUIDManager(),
            new NeedHandler(),
            this.IconHandler,
            this.CultureHandler,
            new EntitySexualityHandler(),
            new EntityBioSexHandler(),
            new GenderHandler(),
            new EntityRomanceHandler(),
            new JobHandler(abilityHandler, this.Roller),
            new PhysicsManager(),
            skillHandler,
            new DerivedValueHandler(statisticHandler, skillHandler),
            this.Roller);
        
        this.Node.AttachJoyObject(this.EntityFactory.CreateFromTemplate(templateHandler.GetRandom(), Vector2Int.Zero));
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
