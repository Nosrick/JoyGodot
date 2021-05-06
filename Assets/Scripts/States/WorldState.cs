using System;
using System.Collections;
using System.Collections.Generic;
using Code.States;
using Godot;
using JoyLib.Code.Conversation;
using JoyLib.Code.Entities;
using JoyLib.Code.Entities.Relationships;
using JoyLib.Code.Events;
using JoyLib.Code.IO;
using JoyLib.Code.Physics;
using JoyLib.Code.Unity;
using JoyLib.Code.Unity.GUI;
using JoyLib.Code.World;
using Thread = System.Threading.Thread;

namespace JoyLib.Code.States
{
    public class WorldState : GameState
    {
        protected IWorldInstance m_ActiveWorld;

        protected IWorldInstance m_Overworld;

        protected Camera2D m_Camera;

        protected DateTime m_DateTime;

        protected const int TICK_TIMER = 50;
        protected double m_TickTimer;

        protected readonly WorldSerialiser m_WorldSerialiser;

        protected IGameManager GameManager { get; set; }
        protected IPhysicsManager PhysicsManager { get; set; }
        protected IEntityRelationshipHandler RelationshipHandler { get; set; }
        protected IConversationEngine ConversationEngine { get; set; }

        protected Thread TickTimer { get; set; }

        protected IJoyObject PrimaryTarget { get; set; }

        public WorldState(IWorldInstance overworldRef, IWorldInstance activeWorldRef) : base()
        {
            this.m_WorldSerialiser = new WorldSerialiser(GlobalConstants.GameManager.ObjectIconHandler);

            this.m_ActiveWorld = activeWorldRef;
            this.m_Overworld = overworldRef;

            this.m_Camera = GlobalConstants.GameManager.MyNode.FindNode("Main Camera") as Camera2D;

            this.GameManager = GlobalConstants.GameManager;
            this.PhysicsManager = this.GameManager.PhysicsManager;
            this.RelationshipHandler = this.GameManager.RelationshipHandler;
            this.ConversationEngine = this.GameManager.ConversationEngine;
            this.GUIManager = this.GameManager.GUIManager;

            //this.TickTimer = this.TickEvent();
            this.TickTimer = new Thread(this.TickEvent);
            this.TickTimer.Start();

            GlobalConstants.GameManager.Player.AliveChange -= this.OnPlayerDeath;
            GlobalConstants.GameManager.Player.AliveChange += this.OnPlayerDeath;
            GlobalConstants.GameManager.Player.ConsciousnessChange -= this.OnPlayerConsciousChange;
            GlobalConstants.GameManager.Player.ConsciousnessChange += this.OnPlayerConsciousChange;

            this.Tick();
        }

        public override void LoadContent()
        { }

        public override void SetUpUi()
        {
            this.GUIManager.InstantiateUIScene(
                GD.Load<PackedScene>(
                    GlobalConstants.GODOT_ASSETS_FOLDER +
                    "Scenes/UI/MainGame.tscn"));
            
            base.SetUpUi();
            this.GUIManager.OpenGUI(GUINames.NEEDSRECT);
            this.GUIManager.OpenGUI(GUINames.DERIVED_VALUES);
            this.GUIManager.OpenGUI(GUINames.ACTION_LOG);

            //this.GUIManager.Get(GUINames.INVENTORY).GetComponent<ItemContainer>().Owner = this.PlayerWorld.Player;
            //GUIManager.GetGUI(GUINames.EQUIPMENT).GetComponent<ItemContainer>().Owner = this.PlayerWorld.Player;

            /*
            EquipmentHandler equipmentHandler =
                this.GUIManager.Get(GUINames.EQUIPMENT).GetComponentInChildren<EquipmentHandler>();
            equipmentHandler.SetPlayer(this.m_ActiveWorld.Player);

            var entryBanner = this.GUIManager.OpenGUI(GUINames.ENTRY_BANNER).GetComponent<EntryBanner>();
            entryBanner.Activate(this.m_ActiveWorld.Name);
            */
        }

        public override void Start()
        {
            this.m_ActiveWorld.Player.Tick();

            this.SetEntityWorld(this.Overworld);
        }

        public override void Stop()
        { }

        public override void Update()
        {
            IEntity player = this.m_ActiveWorld.Player;

            if ((player.FulfillmentData is null || player.FulfillmentData.Counter <= 0)
                && this.AutoTurn
                && player.Conscious
                && this.ManualAutoTurn == false)
            {
                this.AutoTurn = false;
            }
            else if (player.FulfillmentData is null == false
                     && player.FulfillmentData.Counter > 0
                     && this.AutoTurn == false)
            {
                this.AutoTurn = true;
            }
            
            this.m_Camera.Position = new Vector2(
                player.WorldPosition.x, 
                player.WorldPosition.y);
        }

        protected void SetEntityWorld(IWorldInstance world)
        {
            foreach (IEntity entity in world.Entities)
            {
                entity.MyWorld = world;
            }

            foreach (IWorldInstance nextWorld in world.Areas.Values)
            {
                this.SetEntityWorld(nextWorld);
            }
        }

        protected void ChangeWorld(IWorldInstance newWorld, Vector2Int spawnPoint)
        {
            this.Done = true;

            IWorldInstance oldWorld = this.m_ActiveWorld;
            IEntity player = oldWorld.Player;

            newWorld.Initialise();
            player.FetchAction("enterworldaction")
                .Execute(
                    new IJoyObject[] {player},
                    new[] {"exploration", "world change"},
                    new Dictionary<string, object>
                    {
                        {"world", newWorld}
                    });
            this.m_ActiveWorld = newWorld;

            player = this.m_ActiveWorld.Player;

            player.Move(spawnPoint);
            player.Tick();

            /*
            var entryBanner = this.GUIManager.OpenGUI(GUINames.ENTRY_BANNER).GetComponent<EntryBanner>();
            entryBanner.Activate(this.m_ActiveWorld.Name);
            */

            this.Tick();
        }

        protected void TickEvent()
        {
            while (true)
            {
                if (this.AutoTurn)
                {
                    this.Tick();
                }
            }
        }

        protected void OnPlayerDeath(object sender, BooleanChangeEventArgs args)
        {
            if (args.Value == false)
            {
                this.AutoTurn = false;
                this.GameManager.SetNextState(new GameOverState(this.m_Overworld));
            }
        }

        protected void OnPlayerConsciousChange(object sender, BooleanChangeEventArgs args)
        {
            this.AutoTurn = !args.Value;
        }

        public override void HandleInput(InputEvent @event)
        {
            bool hasMoved = false;
            IEntity player = this.m_ActiveWorld.Player;

            /*
            if(Input.GetMouseButtonDown(0))
            {
                Vector3 mouseWorld = m_Camera.ScreenToWorldPoint(Input.mousePosition);
                int x = (int)mouseWorld.x;
                int y = (int)mouseWorld.y;

                Pathfinder pathfinder = new Pathfinder();
                Queue<Vector2Int> path = pathfinder.FindPath(player.WorldPosition, new Vector2Int(x, y), m_ActiveWorld);
                player.SetPath(path);
                autoTurn = true;
            }
            */

            if (!(@event is InputEventAction action))
            {
                return;
            }

            if (action.Action.Equals("auto turn", StringComparison.OrdinalIgnoreCase))
            {
                this.AutoTurn = !this.AutoTurn;
                this.ManualAutoTurn = this.AutoTurn;
            }
            
            if (this.AutoTurn)
            {
                return;
            }

            Vector2Int newPlayerPoint = this.m_ActiveWorld.Player.WorldPosition;

            if (action.Action.Equals("close all windows", StringComparison.OrdinalIgnoreCase))
            {
                if (this.GUIManager.AreAnyOpen() == false)
                {
                    this.GUIManager.OpenGUI(GUINames.PAUSE);
                }
                else
                {
                    this.GUIManager.CloseAllGUIs();
                }
            }

            if (action.Action.Equals("toggle inventory", StringComparison.OrdinalIgnoreCase))
            {
                this.GUIManager.ToggleGUI(GUINames.INVENTORY);
                if (this.GUIManager.IsActive(GUINames.INVENTORY) == false)
                {
                    this.GUIManager.CloseGUI(GUINames.INVENTORY_CONTAINER);
                }
            }
            else if (action.Action.Equals("toggle equipment", StringComparison.OrdinalIgnoreCase))
            {
                this.GUIManager.ToggleGUI(GUINames.EQUIPMENT);
            }
            else if (action.Action.Equals("toggle journal", StringComparison.OrdinalIgnoreCase))
            {
                this.GUIManager.ToggleGUI(GUINames.QUEST_JOURNAL);
            }
            else if (action.Action.Equals("toggle job management", StringComparison.OrdinalIgnoreCase))
            {
                this.GUIManager.ToggleGUI(GUINames.JOB_MANAGEMENT);
            }
            else if (action.Action.Equals("toggle character sheet", StringComparison.OrdinalIgnoreCase))
            {
                this.GUIManager.ToggleGUI(GUINames.CHARACTER_SHEET);
            }

            if (this.GUIManager.AreAnyOpen() == false)
            {
                this.GUIManager.OpenGUI(GUINames.NEEDSRECT);
                this.GUIManager.OpenGUI(GUINames.DERIVED_VALUES);
            }

            if (this.GUIManager.RemovesControl())
            {
                return;
            }

            if (action.Action.Equals("interact", StringComparison.OrdinalIgnoreCase))
            {
                //Going up a level
                if (this.m_ActiveWorld.Parent != null && player.WorldPosition == this.m_ActiveWorld.SpawnPoint &&
                    !player.HasMoved)
                {
                    this.ChangeWorld(this.m_ActiveWorld.Parent, this.m_ActiveWorld.GetTransitionPointForParent());
                    return;
                }

                //Going down a level
                if (this.m_ActiveWorld.Areas.ContainsKey(player.WorldPosition) && !player.HasMoved)
                {
                    this.ChangeWorld(this.m_ActiveWorld.Areas[player.WorldPosition],
                        this.m_ActiveWorld.Areas[player.WorldPosition].SpawnPoint);
                    return;
                }
            }

            if (action.Action.Equals("skip turn", StringComparison.OrdinalIgnoreCase))
            {
                this.Tick();
            }
            //North
            else if (action.Action.Equals("N", StringComparison.OrdinalIgnoreCase))
            {
                newPlayerPoint.y += 1;
                hasMoved = true;
            }
            //North east
            else if (action.Action.Equals("NE", StringComparison.OrdinalIgnoreCase))
            {
                newPlayerPoint.x += 1;
                newPlayerPoint.y += 1;
                hasMoved = true;
            }
            //East
            else if (action.Action.Equals("E", StringComparison.OrdinalIgnoreCase))
            {
                newPlayerPoint.x += 1;
                hasMoved = true;
            }
            //South east
            else if (action.Action.Equals("SE", StringComparison.OrdinalIgnoreCase))
            {
                newPlayerPoint.x += 1;
                newPlayerPoint.y -= 1;
                hasMoved = true;
            }
            //South
            else if (action.Action.Equals("S", StringComparison.OrdinalIgnoreCase))
            {
                newPlayerPoint.y -= 1;
                hasMoved = true;
            }
            //South west
            else if (action.Action.Equals("SW", StringComparison.OrdinalIgnoreCase))
            {
                newPlayerPoint.x -= 1;
                newPlayerPoint.y -= 1;
                hasMoved = true;
            }
            //West
            else if (action.Action.Equals("W", StringComparison.OrdinalIgnoreCase))
            {
                newPlayerPoint.x -= 1;
                hasMoved = true;
            }
            //North west
            else if (action.Action.Equals("NW", StringComparison.OrdinalIgnoreCase))
            {
                newPlayerPoint.x -= 1;
                newPlayerPoint.y += 1;
                hasMoved = true;
            }

            if (hasMoved)
            {
                PhysicsResult physicsResult =
                    this.PhysicsManager.IsCollision(player.WorldPosition, newPlayerPoint, this.m_ActiveWorld);

                if (physicsResult == PhysicsResult.EntityCollision)
                {
                    IEntity tempEntity = this.m_ActiveWorld.GetEntity(newPlayerPoint);
                    this.PlayerWorld.SwapPosition(player, tempEntity);
                    this.Tick();

                    /*
                        if (m_GameplayFlags == GameplayFlags.Interacting)
                        { }
                        else if (m_GameplayFlags == GameplayFlags.Giving)
                        { }
                        else if (m_GameplayFlags == GameplayFlags.Moving)
                        {
                            
                        }
                        else if (m_GameplayFlags == GameplayFlags.Attacking)
                        {
                            if (tempEntity.GUID != player.GUID)
                            {
                                //CombatEngine.SwingWeapon(player, tempEntity);
                                IEnumerable<IRelationship> relationships =
                                    RelationshipHandler.Get(new IJoyObject[] {tempEntity, player});
                                foreach (IRelationship relationship in relationships)
                                {
                                    relationship.ModifyValueOfParticipant(player.GUID, tempEntity.GUID, -50);
                                }

                                if (!tempEntity.Alive)
                                {
                                    m_ActiveWorld.RemoveEntity(newPlayerPoint);

                                    //Find a way to remove the GameObject
                                    for (int i = 0; i < m_EntitiesHolder.transform.childCount; i++)
                                    {
                                        if (m_EntitiesHolder.transform.GetChild(i).name
                                            .Contains(tempEntity.GUID.ToString()))
                                        {
                                            this.GameManager.EntityPool.Retire(m_EntitiesHolder.transform.GetChild(i)
                                                .gameObject);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        */
                }
                else if (physicsResult == PhysicsResult.WallCollision)
                {
                    //Do nothing!
                }
                else
                {
                    if (newPlayerPoint.x >= 0 && newPlayerPoint.x < this.m_ActiveWorld.Tiles.GetLength(0) &&
                        newPlayerPoint.y >= 0 && newPlayerPoint.y < this.m_ActiveWorld.Tiles.GetLength(1))
                    {
                        player.Move(newPlayerPoint);
                        this.Tick();
                    }
                }
            }

            /*
                else if (m_GameplayFlags == GameplayFlags.Targeting)
                {
                    if (player.TargetingAbility.TargetType == AbilityTarget.Adjacent)
                    {
                        if (AdjacencyHelper.IsAdjacent(player.WorldPosition, player.TargetPoint))
                        {
                            IEntity tempEntity = m_ActiveWorld.GetEntity(player.TargetPoint);
                            if (tempEntity != null && Input.GetKeyDown(KeyCode.Return))
                            {
                                player.TargetingAbility.OnUse(player, tempEntity);
                                Tick();
                                m_GameplayFlags = GameplayFlags.Moving;
                            }
                        }
                    }
                    else if (player.TargetingAbility.TargetType == AbilityTarget.Ranged)
                    {
                        IEntity tempEntity = m_ActiveWorld.GetEntity(player.TargetPoint);
                        if (tempEntity != null && Input.GetKeyDown(KeyCode.Return))
                        {
                            player.TargetingAbility.OnUse(player, tempEntity);
                            Tick();
                            m_GameplayFlags = GameplayFlags.Moving;
                        }
                    }
                }
                */
        }

        protected void Tick()
        {
            this.m_ActiveWorld.Tick();
            this.DrawObjects();
        }

        protected void DrawObjects()
        {
            /*
            IEntity player = this.m_ActiveWorld.Player;
            for (int i = 0; i < this.m_FogOfWarHolder.transform.childCount; i++)
            {
                GameObject fog = this.m_FogOfWarHolder.transform.GetChild(i).gameObject;
                Vector2Int position = fog.GetComponent<GridPosition>().WorldPosition;

                if (this.GameManager.Cheats.CheatBank.TryGetValue("fullbright", out bool cheat) && cheat)
                {
                    fog.GetComponent<SpriteRenderer>().color = Color.clear;
                }
                else
                {
                    bool canSee = player.VisionProvider.CanSee(player, this.m_ActiveWorld, position);
                    if (canSee)
                    {
                        int lightLevel = this.m_ActiveWorld.LightCalculator.Light.GetLight(position);
                        fog.GetComponent<SpriteRenderer>().color = LightLevelHelper.GetColour(
                            lightLevel,
                            player.VisionProvider);
                    }
                    else
                    {
                        fog.GetComponent<SpriteRenderer>().color = player.VisionProvider.DarkColour;
                    }
                }
            }
            */
        }

        public override GameState GetNextState()
        {
            //this.GameManager.MyGameObject.GetComponent<MonoBehaviour>().StopCoroutine(this.TickTimer);
            return new WorldDestructionState(this.m_Overworld, this.m_ActiveWorld);
        }

        public IWorldInstance Overworld => this.m_Overworld;

        public IWorldInstance PlayerWorld => this.m_ActiveWorld;

        protected int TickCounter { get; set; }

        protected bool AutoTurn { get; set; }

        protected bool ManualAutoTurn { get; set; }

        protected bool ExpandConsole { get; set; }
    }
}