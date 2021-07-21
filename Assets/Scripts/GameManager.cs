using System;
using Godot;
using JoyGodot.Assets.Scripts.Audio;
using JoyGodot.Assets.Scripts.Collections;
using JoyGodot.Assets.Scripts.Combat;
using JoyGodot.Assets.Scripts.Conversation;
using JoyGodot.Assets.Scripts.Conversation.Subengines.Rumours;
using JoyGodot.Assets.Scripts.Conversation.Subengines.Rumours.Parameters;
using JoyGodot.Assets.Scripts.Cultures;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Abilities;
using JoyGodot.Assets.Scripts.Entities.AI.LOS.Providers;
using JoyGodot.Assets.Scripts.Entities.Gender;
using JoyGodot.Assets.Scripts.Entities.Jobs;
using JoyGodot.Assets.Scripts.Entities.Needs;
using JoyGodot.Assets.Scripts.Entities.Relationships;
using JoyGodot.Assets.Scripts.Entities.Romance;
using JoyGodot.Assets.Scripts.Entities.Sexes;
using JoyGodot.Assets.Scripts.Entities.Sexuality;
using JoyGodot.Assets.Scripts.Entities.Statistics;
using JoyGodot.Assets.Scripts.Godot;
using JoyGodot.Assets.Scripts.Graphics;
using JoyGodot.Assets.Scripts.GUI;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.Items;
using JoyGodot.Assets.Scripts.Items.Crafting;
using JoyGodot.Assets.Scripts.Managers;
using JoyGodot.Assets.Scripts.Physics;
using JoyGodot.Assets.Scripts.Quests;
using JoyGodot.Assets.Scripts.Rollers;
using JoyGodot.Assets.Scripts.Scripting;
using JoyGodot.Assets.Scripts.Settings;
using JoyGodot.Assets.Scripts.States;
using JoyGodot.Assets.Scripts.World;
using JoyGodot.Assets.Scripts.World.WorldInfo;
using Thread = System.Threading.Thread;

namespace JoyGodot.Assets.Scripts
{
    public class GameManager : Node, IGameManager
    {
        protected StateManager m_StateManager;

        protected IGameState NextState { get; set; }
        
        protected Thread LoadingThread { get; set; }

        // Use this for initialization
        public GameManager()
        {
            if (GlobalConstants.GameManager is null)
            {
                GlobalConstants.GameManager = this;
            }

            this.LoadingMessage = "Just waking up";
        }

        public override void _Ready()
        {
            base._Ready();
            this.BegunInitialisation = true;
            
            Input.SetMouseMode(Input.MouseMode.Hidden);

            this.LoadingMessage = "Initialising object pools";
            this.WorldHolder = this.GetNode<Node2D>("WorldHolder");
            this.FloorTileMap = this.WorldHolder.FindNode("WorldFloors") as TileMap;
            this.WallTileMap = this.WorldHolder.FindNode("WorldWalls") as TileMap;
            this.ItemHolder = (Node2D) this.WorldHolder.FindNode("WorldObjects");
            this.EntityHolder = (Node2D) this.WorldHolder.FindNode("WorldEntities");
            this.FogHolder = (Node2D) this.WorldHolder.FindNode("WorldFog");

            PackedScene prefab = GD.Load<PackedScene>(GlobalConstants.GODOT_ASSETS_FOLDER + "Scenes/Parts/JoyObject.tscn");
            PackedScene positionableSprite = GD.Load<PackedScene>(GlobalConstants.GODOT_ASSETS_FOLDER + "Scenes/Parts/ManagedSprite.tscn");
            PackedScene fog =
                GD.Load<PackedScene>(GlobalConstants.GODOT_ASSETS_FOLDER + "Scenes/Parts/Fog of War.tscn");
            this.EntityPool = new GameObjectPool<JoyObjectNode>(prefab, this.EntityHolder);
            this.ItemPool = new GameObjectPool<JoyObjectNode>(prefab, this.ItemHolder);
            this.FogPool = new GameObjectPool<PositionableSprite>(fog, this.FogHolder);

            this.MyNode = this;

            this.LoadingThread = new Thread(this.Load);
            this.LoadingThread.Start();
        }

        // Update is called once per frame
        public override void _Process(float delta)
        {
            this.m_StateManager?.Update();
            this.ActionLog?.Update();
        }

        public override void _Notification(int what)
        {
            base._Notification(what);

            if (what == MainLoop.NotificationCrash
                || what == MainLoop.NotificationWmQuitRequest)
            {
                this.ActionLog.Flush();
            }
        }

        public override void _Input(InputEvent @event)
        {
            base._Input(@event);
            this.m_StateManager?.Process(@event);
        }

        protected void Load()
        {
            this.LoadingMessage = "Initialising action log";
            this.ActionLog = new ActionLog();
            GlobalConstants.ActionLog = this.ActionLog;
            
            GlobalConstants.ScriptingEngine = new ScriptingEngine();
            
            this.LoadingMessage = "Revving up engines";

            this.SettingsManager = new SettingsManager();

            this.AudioHandler = new AudioHandler();

            this.GUIDManager = new GUIDManager();

            this.WorldHandler = new WorldHandler();
            
            this.Roller = new RNG();

            this.ObjectIconHandler = new ObjectIconHandler(this.Roller);

            this.GUIManager = new GUIManager(this.FindNode("MainUI"));
            this.CombatEngine = new CombatEngine();
            
            this.m_StateManager = new StateManager();

            this.PhysicsManager = new PhysicsManager();

            this.RelationshipHandler = new EntityRelationshipHandler();

            this.AbilityHandler = new AbilityHandler();

            this.MaterialHandler = new MaterialHandler();

            this.VisionProviderHandler = new VisionProviderHandler();

            this.LoadingMessage = "Initialising entity gubbinz";
            this.StatisticHandler = new EntityStatisticHandler();
            this.NeedHandler = new NeedHandler();
            this.CultureHandler = new CultureHandler(this.ObjectIconHandler);
            this.BioSexHandler = new EntityBioSexHandler();
            this.SexualityHandler = new EntitySexualityHandler();
            this.RomanceHandler = new EntityRomanceHandler();
            this.JobHandler = new JobHandler(this.AbilityHandler, this.Roller);
            this.GenderHandler = new GenderHandler();
            this.SkillHandler = new EntitySkillHandler();
            this.EntityTemplateHandler = new EntityTemplateHandler(
                this.SkillHandler,
                this.VisionProviderHandler,
                this.AbilityHandler);
            
            this.m_StateManager.ChangeState(new LoadingState());

            this.DerivedValueHandler = new DerivedValueHandler(this.StatisticHandler, this.SkillHandler);

            this.WorldInfoHandler = new WorldInfoHandler(this.ObjectIconHandler);

            this.ParameterProcessorHandler = new ParameterProcessorHandler();

            this.CraftingRecipeHandler = new CraftingRecipeHandler();

            this.ItemDatabase = new ItemDatabase(
                this.ObjectIconHandler,
                this.MaterialHandler,
                this.AbilityHandler,
                this.CraftingRecipeHandler,
                this.Roller);
            
            this.EntityHandler = new LiveEntityHandler();
            this.ItemHandler = new LiveItemHandler(this.Roller);

            this.EntityFactory = new EntityFactory(
                this.GUIDManager, 
                this.NeedHandler, 
                this.ObjectIconHandler, 
                this.CultureHandler,
                this.SexualityHandler, 
                this.BioSexHandler, 
                this.GenderHandler, 
                this.RomanceHandler, 
                this.JobHandler,
                this.PhysicsManager, 
                this.SkillHandler,
                this.DerivedValueHandler,
                this.Roller);

            this.ItemFactory = new ItemFactory(
                this.GUIDManager, 
                this.ItemDatabase,
                this.ItemHandler, 
                this.ObjectIconHandler,
                this.DerivedValueHandler,
                this.ItemPool, this.Roller);

            this.QuestProvider =
                new QuestProvider(this.RelationshipHandler, this.ItemHandler, this.ItemFactory, this.Roller);
            this.QuestTracker = new QuestTracker(this.ItemHandler);

            this.RumourMill = new ConcreteRumourMill();

            this.ConversationEngine = new ConversationEngine(this.RelationshipHandler, this.GUIDManager.AssignGUID());

            this.NaturalWeaponHelper = new NaturalWeaponHelper(this.MaterialHandler, this.ItemFactory);

            this.LoadingMessage = "Done!";
            
            this.Initialised = true;
            this.LoadingThread.Abort();
        }

        public void SetNextState(IGameState nextState = null)
        {
            this.NextState = nextState;
            this.m_StateManager.ChangeState(this.NextState);
        }

        public void RetireAll()
        {
            this.EntityPool.RetireAll();
            this.FogPool.RetireAll();
            this.ItemPool.RetireAll();
            this.FloorTileMap.Clear();
            this.WallTileMap.Clear();
        }

        public void Reset()
        {
            this.m_StateManager.Stop();
            
            this.EntityPool.RetireAll();
            this.FogPool.RetireAll();
            this.ItemPool.RetireAll();

            this.Load();
        }
        
        public bool BegunInitialisation { get; protected set; }

        public bool Initialised { get; protected set; }

        public int LoadingPercentage { get; protected set; }

        public string LoadingMessage { get; protected set; }
        public ActionLog ActionLog { get; protected set; }
        public ICombatEngine CombatEngine { get; protected set; }
        public IQuestTracker QuestTracker { get; protected set; }
        public IQuestProvider QuestProvider { get; protected set; }
        public IEntityRelationshipHandler RelationshipHandler { get; protected set; }
        public IObjectIconHandler ObjectIconHandler { get; protected set; }
        public IMaterialHandler MaterialHandler { get; protected set; }
        public ICultureHandler CultureHandler { get; protected set; }
        public IEntityStatisticHandler StatisticHandler { get; protected set; }
        public IEntityTemplateHandler EntityTemplateHandler { get; protected set; }
        public IEntityBioSexHandler BioSexHandler { get; protected set; }
        public IEntitySexualityHandler SexualityHandler { get; protected set; }
        public IJobHandler JobHandler { get; protected set; }
        public IEntityRomanceHandler RomanceHandler { get; protected set; }
        public IGenderHandler GenderHandler { get; protected set; }
        public IGUIManager GUIManager { get; protected set; }
        public IAudioHandler AudioHandler { get; protected set; }
        public IParameterProcessorHandler ParameterProcessorHandler { get; protected set; }
        public ILiveEntityHandler EntityHandler { get; protected set; }
        public ILiveItemHandler ItemHandler { get; protected set; }
        public IItemDatabase ItemDatabase { get; protected set; }
        public ICraftingRecipeHandler CraftingRecipeHandler { get; protected set; }
        public INeedHandler NeedHandler { get; protected set; }
        public IEntitySkillHandler SkillHandler { get; protected set; }
        public IWorldInfoHandler WorldInfoHandler { get; protected set; }
        public IPhysicsManager PhysicsManager { get; protected set; }
        public IConversationEngine ConversationEngine { get; protected set; }
        public IAbilityHandler AbilityHandler { get; protected set; }
        public IDerivedValueHandler DerivedValueHandler { get; protected set; }
        public IVisionProviderHandler VisionProviderHandler { get; protected set; }
        public IWorldHandler WorldHandler { get; protected set; }
        
        public IRumourMill RumourMill { get; protected set; }

        public NaturalWeaponHelper NaturalWeaponHelper { get; protected set; }
        public RNG Roller { get; protected set; }
        public IEntityFactory EntityFactory { get; protected set; }
        public IItemFactory ItemFactory { get; protected set; }
        
        public SettingsManager SettingsManager { get; protected set; }
        
        public Node MyNode { get; protected set; }

        public GUIDManager GUIDManager { get; protected set; }
        
        public IEntity Player => this.EntityHandler?.GetPlayer();

        public TileMap FloorTileMap { get; protected set; }
        public TileMap WallTileMap { get; protected set; }
        public GameObjectPool<JoyObjectNode> EntityPool { get; protected set; }
        public GameObjectPool<JoyObjectNode> ItemPool { get; protected set; }
        public GameObjectPool<PositionableSprite> FogPool { get; protected set; }
        public Node2D FogHolder { get; protected set; }
        public Node2D WorldHolder { get; protected set; }
        public Node2D EntityHolder { get; protected set; }
        public Node2D ItemHolder { get; protected set; }
    }
}