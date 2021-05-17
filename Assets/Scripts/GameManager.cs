using System;
using Code.Collections;
using Godot;
using Joy.Code.Managers;
using JoyGodot.Assets.Scripts.States;
using JoyLib.Code.Combat;
using JoyLib.Code.Conversation;
using JoyLib.Code.Conversation.Conversations;
using JoyLib.Code.Conversation.Subengines.Rumours;
using JoyLib.Code.Conversation.Subengines.Rumours.Parameters;
using JoyLib.Code.Cultures;
using JoyLib.Code.Entities;
using JoyLib.Code.Entities.Abilities;
using JoyLib.Code.Entities.AI.LOS.Providers;
using JoyLib.Code.Entities.Gender;
using JoyLib.Code.Entities.Items;
using JoyLib.Code.Entities.Jobs;
using JoyLib.Code.Entities.Needs;
using JoyLib.Code.Entities.Relationships;
using JoyLib.Code.Entities.Romance;
using JoyLib.Code.Entities.Sexes;
using JoyLib.Code.Entities.Sexuality;
using JoyLib.Code.Entities.Statistics;
using JoyLib.Code.Godot;
using JoyLib.Code.Graphics;
using JoyLib.Code.Helpers;
using JoyLib.Code.Managers;
using JoyLib.Code.Physics;
using JoyLib.Code.Quests;
using JoyLib.Code.Rollers;
using JoyLib.Code.States;
using JoyLib.Code.Unity;
using JoyLib.Code.Unity.GUI;
using JoyLib.Code.World;
using Thread = System.Threading.Thread;

namespace JoyLib.Code
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
            
            //Input.SetMouseMode(Input.MouseMode.Hidden);

            this.LoadingMessage = "Initialising object pools";
            Node2D worldHolder = this.GetNode<Node2D>("WorldHolder");
            Node2D floorHolder = (Node2D) worldHolder.FindNode("WorldFloors");
            Node2D wallHolder = (Node2D) worldHolder.FindNode("WorldWalls");
            Node2D objectHolder = (Node2D) worldHolder.FindNode("WorldObjects");
            Node2D entityHolder = (Node2D) worldHolder.FindNode("WorldEntities");
            Node2D fogHolder = (Node2D) worldHolder.FindNode("WorldFog");

            PackedScene prefab = GD.Load<PackedScene>(GlobalConstants.GODOT_ASSETS_FOLDER + "Scenes/Parts/JoyObject.tscn");
            PackedScene positionableSprite = GD.Load<PackedScene>(GlobalConstants.GODOT_ASSETS_FOLDER + "Scenes/Parts/ManagedSprite.tscn");
            Sprite fog = new Sprite
            {
                Texture = GD.Load<Texture>(GlobalConstants.GODOT_ASSETS_FOLDER + "Sprites/obscure.png")
            };
            this.FloorPool = new GameObjectPool<ManagedSprite>(positionableSprite, floorHolder);
            this.WallPool = new GameObjectPool<JoyObjectNode>(prefab, wallHolder);
            this.EntityPool = new GameObjectPool<JoyObjectNode>(prefab, entityHolder);
            this.ItemPool = new GameObjectPool<JoyObjectNode>(prefab, objectHolder);
            this.FogPool = new GameObjectPool<Sprite>(fog, fogHolder);

            this.MyNode = this;

            this.LoadingThread = new Thread(this.Load);
            this.LoadingThread.Start();
        }

        // Update is called once per frame
        public override void _Process(float delta)
        {
            this.m_StateManager?.Update();
            this.ActionLog.Update();
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
            
            this.LoadingMessage = "Revving up engines";

            this.GUIDManager = new GUIDManager();

            GlobalConstants.ActionLog = this.ActionLog;
            
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

            this.ItemDatabase = new ItemDatabase(
                this.ObjectIconHandler,
                this.MaterialHandler,
                this.AbilityHandler,
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

        public void Reset()
        {
            this.m_StateManager.Stop();
            
            this.EntityPool.RetireAll();
            this.FloorPool.RetireAll();
            this.FogPool.RetireAll();
            this.ItemPool.RetireAll();
            this.WallPool.RetireAll();

            this.Dispose();

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
        public IParameterProcessorHandler ParameterProcessorHandler { get; protected set; }
        public ILiveEntityHandler EntityHandler { get; protected set; }
        public ILiveItemHandler ItemHandler { get; protected set; }
        public IItemDatabase ItemDatabase { get; protected set; }
        public INeedHandler NeedHandler { get; protected set; }
        public IEntitySkillHandler SkillHandler { get; protected set; }
        public IWorldInfoHandler WorldInfoHandler { get; protected set; }
        public IPhysicsManager PhysicsManager { get; protected set; }
        public IConversationEngine ConversationEngine { get; protected set; }
        public IAbilityHandler AbilityHandler { get; protected set; }
        public IDerivedValueHandler DerivedValueHandler { get; protected set; }
        public IVisionProviderHandler VisionProviderHandler { get; protected set; }
        
        public IRumourMill RumourMill { get; protected set; }

        public NaturalWeaponHelper NaturalWeaponHelper { get; protected set; }
        public RNG Roller { get; protected set; }
        //public SettingsManager SettingsManager { get; protected set; }
        public IEntityFactory EntityFactory { get; protected set; }
        public IItemFactory ItemFactory { get; protected set; }
        public Node MyNode { get; protected set; }

        public GUIDManager GUIDManager { get; protected set; }
        
        public IEntity Player => this.EntityHandler.GetPlayer();

        public GameObjectPool<ManagedSprite> FloorPool { get; protected set; }
        public GameObjectPool<JoyObjectNode> WallPool { get; protected set; }
        public GameObjectPool<JoyObjectNode> EntityPool { get; protected set; }
        public GameObjectPool<JoyObjectNode> ItemPool { get; protected set; }
        public GameObjectPool<Sprite> FogPool { get; protected set; }
        
        //public CheatInterface Cheats { get; set; }

        public void Dispose()
        {
            this.GUIManager?.Dispose();
            this.GUIManager = null;
            
            this.EntityHandler?.Dispose();
            this.EntityHandler = null;
            
            this.ItemHandler?.Dispose();
            this.ItemHandler = null;
            
            this.RelationshipHandler?.Dispose();
            this.RelationshipHandler = null;
            
            this.MaterialHandler?.Dispose();
            this.MaterialHandler = null;
            
            this.CultureHandler?.Dispose();
            this.CultureHandler = null;
            
            this.StatisticHandler?.Dispose();
            this.StatisticHandler = null;
            
            this.EntityTemplateHandler?.Dispose();
            this.EntityTemplateHandler = null;
            
            this.BioSexHandler?.Dispose();
            this.BioSexHandler = null;
            
            this.SexualityHandler?.Dispose();
            this.SexualityHandler = null;
            
            this.JobHandler?.Dispose();
            this.JobHandler = null;
            
            this.RomanceHandler?.Dispose();
            this.RomanceHandler = null;
            
            this.GenderHandler?.Dispose();
            this.GenderHandler = null;
            
            this.ItemDatabase?.Dispose();
            this.ItemDatabase = null;
            
            this.NeedHandler?.Dispose();
            this.NeedHandler = null;
            
            this.SkillHandler?.Dispose();
            this.SkillHandler = null;
            
            this.WorldInfoHandler?.Dispose();
            this.WorldInfoHandler = null;
            
            this.DerivedValueHandler?.Dispose();
            this.DerivedValueHandler = null;
            
            this.VisionProviderHandler?.Dispose();
            this.VisionProviderHandler = null;
            
            this.AbilityHandler?.Dispose();
            this.AbilityHandler = null;

            this.ConversationEngine = null;
            this.PhysicsManager = null;

            this.ParameterProcessorHandler = null;

            this.QuestProvider = null;
            this.QuestTracker = null;
            this.CombatEngine = null;
            
            this.RumourMill?.Dispose();
            this.RumourMill = null;
            
            this.ActionLog?.Dispose();
            this.ActionLog = null;
            
            this.GUIDManager?.Dispose();
            this.GUIDManager = null;

            this.EntityFactory = null;
            this.ItemFactory = null;
            this.NaturalWeaponHelper = null;

            GC.Collect();
        }
    }
}