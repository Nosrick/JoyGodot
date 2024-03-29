﻿using System;
using System.Collections.Generic;
using Godot;
using JoyGodot.Assets.Scripts.Calendar;
using JoyGodot.Assets.Scripts.Conversation;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Needs;
using JoyGodot.Assets.Scripts.Entities.Relationships;
using JoyGodot.Assets.Scripts.Events;
using JoyGodot.Assets.Scripts.Godot;
using JoyGodot.Assets.Scripts.GUI;
using JoyGodot.Assets.Scripts.GUI.Inventory_System;
using JoyGodot.Assets.Scripts.GUI.WorldState;
using JoyGodot.Assets.Scripts.IO;
using JoyGodot.Assets.Scripts.Items;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Physics;
using JoyGodot.Assets.Scripts.World;
using Thread = System.Threading.Thread;

namespace JoyGodot.Assets.Scripts.States
{
    public class WorldState : GameState
    {
        protected IWorldInstance m_ActiveWorld;

        protected IWorldInstance m_Overworld;

        protected Camera2D m_Camera;

        protected const int TICK_TIMER = 50;
        protected double m_TickTimer;
        protected IGameManager GameManager { get; set; }
        protected IPhysicsManager PhysicsManager { get; set; }
        protected IEntityRelationshipHandler RelationshipHandler { get; set; }
        protected IConversationEngine ConversationEngine { get; set; }

        protected Thread TickTimer { get; set; }

        protected IJoyObject PrimaryTarget { get; set; }

        protected Node2D FogHolder { get; set; }

        public WorldState(IWorldInstance overworldRef, IWorldInstance activeWorldRef) : base()
        {
            this.m_ActiveWorld = activeWorldRef;
            this.m_Overworld = overworldRef;

            this.GameManager = GlobalConstants.GameManager;
            this.PhysicsManager = this.GameManager.PhysicsManager;
            this.RelationshipHandler = this.GameManager.RelationshipHandler;
            this.ConversationEngine = this.GameManager.ConversationEngine;
            this.GUIManager = this.GameManager.GUIManager;
            
            this.m_Camera = new Camera2D
            {
                AnchorMode = Camera2D.AnchorModeEnum.DragCenter,
                ProcessMode = Camera2D.Camera2DProcessMode.Physics,
                Current = true,
                ZIndex = 100,
                ZAsRelative = false
            };

            this.FogHolder = this.GameManager.FogHolder;
            
            GlobalConstants.GameManager?.Player.MyNode?.AddChild(this.m_Camera);

            GlobalConstants.GameManager.Player.AliveChange -= this.OnPlayerDeath;
            GlobalConstants.GameManager.Player.AliveChange += this.OnPlayerDeath;
            GlobalConstants.GameManager.Player.ConsciousnessChange -= this.OnPlayerConsciousChange;
            GlobalConstants.GameManager.Player.ConsciousnessChange += this.OnPlayerConsciousChange;

            var player = GlobalConstants.GameManager.Player;
            for (int i = 0; i < this.FogHolder.GetChildCount(); i++)
            {
                PositionableSprite fog = this.FogHolder.GetChild(i) as PositionableSprite;
                ShaderMaterial shaderMaterial = fog?.Material as ShaderMaterial;
                shaderMaterial?.SetShaderParam("darkColour", player.VisionProvider.DarkColour);
                shaderMaterial?.SetShaderParam("lightColour", player.VisionProvider.LightColour);
                shaderMaterial?.SetShaderParam("minimumLight", player.VisionProvider.MinimumLightLevel);
                shaderMaterial?.SetShaderParam("minimumComfort", player.VisionProvider.MinimumComfortLevel);
                shaderMaterial?.SetShaderParam("maximumComfort", player.VisionProvider.MaximumComfortLevel);
                shaderMaterial?.SetShaderParam("maximumLight", player.VisionProvider.MaximumLightLevel);
            }

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
            this.GUIManager.FindGUIs();
            this.GUIManager.OpenGUI(this, GUINames.NEEDS_PANEL);
            this.GUIManager.OpenGUI(this, GUINames.DERIVED_VALUES);
            this.GUIManager.OpenGUI(this, GUINames.ACTION_LOG);

            var inventory = this.GUIManager.Get(GUINames.INVENTORY) as ItemContainer;
            IEntity player = GlobalConstants.GameManager.Player;
            inventory.ContainerOwner = player;
            inventory.TitleText = player.JoyName + "'s Inventory";

            var equipment = this.GUIManager.Get(GUINames.EQUIPMENT) as ConstrainedItemContainer;
            equipment.ContainerOwner = player;
            equipment.TitleText = player.JoyName + "'s Equipment";

            var entryBanner = this.GUIManager.Get<EntryBanner>(GUINames.ENTRY_BANNER);
            entryBanner.TitleText = this.m_ActiveWorld.Name;
            this.GUIManager.OpenGUI(this, GUINames.ENTRY_BANNER);
        }

        public override void Start()
        {
            this.TickTimer = new Thread(this.TickEvent);
            this.TickTimer.Start();
            
            this.m_ActiveWorld.Tick();

            this.SetEntityWorld(this.Overworld);
        }

        public override void Stop()
        {
            base.Stop();
            
            GlobalConstants.GameManager?.Player?.MyNode?.RemoveChild(this.m_Camera);
            this.m_Camera.QueueFree();
            
            this.GameManager.RetireAll();

            this.TickTimer.Abort();
        }

        public override void Update()
        {
            IEntity player = GlobalConstants.GameManager.Player;

            if ((player.NeedFulfillmentData.IsEmpty() || player.NeedFulfillmentData.Counter <= 0)
                && this.AutoTurn
                && player.Conscious
                && this.ManualAutoTurn == false)
            {
                this.AutoTurn = false;
            }
            else if (player.NeedFulfillmentData.IsEmpty() == false
                     && player.NeedFulfillmentData.Counter > 0
                     && this.AutoTurn == false)
            {
                this.AutoTurn = true;
            }
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

            IEntity player = GlobalConstants.GameManager.Player;

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

            player.Move(spawnPoint);
            player.Tick();

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
            InputEvent action = @event;
            
            bool hasMoved = false;
            IEntity player = GlobalConstants.GameManager.Player;

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

            if (action.IsActionReleased("auto turn"))
            {
                this.AutoTurn = !this.AutoTurn;
                this.ManualAutoTurn = this.AutoTurn;
            }

            Vector2Int newPlayerPoint = GlobalConstants.GameManager.Player.WorldPosition;

            if (action.IsActionReleased("close all windows"))
            {
                if (this.GUIManager.AreAnyOpen() == false)
                {
                    this.GUIManager.OpenGUI(this, GUINames.PAUSE);
                }
                else
                {
                    this.GUIManager.CloseAllGUIs();
                }
            }

            if (action.IsActionReleased("toggle inventory"))
            {
                this.ToggleWindow(GUINames.INVENTORY);
                if (this.GUIManager.IsActive(GUINames.INVENTORY) == false)
                {
                    this.GUIManager.CloseGUI(this, GUINames.INVENTORY_CONTAINER);
                }
            }
            else if (action.IsActionReleased("toggle equipment"))
            {
                this.ToggleWindow(GUINames.EQUIPMENT);
            }
            else if (action.IsActionReleased("toggle journal"))
            {
                this.ToggleWindow(GUINames.QUEST_JOURNAL);
            }
            else if (action.IsActionReleased("toggle job management"))
            {
                this.ToggleWindow(GUINames.JOB_MANAGEMENT);
            }
            else if (action.IsActionReleased("toggle character sheet"))
            {
                this.ToggleWindow(GUINames.CHARACTER_SHEET);
            }
            else if (action.IsActionReleased("toggle_crafting"))
            {
                this.ToggleWindow(GUINames.CRAFTING_SCREEN);
            }

            if (this.GUIManager.AreAnyOpen() == false)
            {
                this.GUIManager.OpenGUI(this, GUINames.NEEDS_PANEL);
                this.GUIManager.OpenGUI(this, GUINames.DERIVED_VALUES);
            }

            if (this.GUIManager.RemovesControl())
            {
                return;
            }

            if (action.IsActionReleased("interact"))
            {
                if (this.m_ActiveWorld.IsObjectAt(player.WorldPosition) == PhysicsResult.ObjectCollision)
                {
                    this.m_ActiveWorld.PickUpObject(player);
                    return;
                }
                
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

            if (action.IsActionReleased("skip turn"))
            {
                this.Tick();
            }
            //North
            else if (action.IsActionReleased("N"))
            {
                newPlayerPoint.y -= 1;
                hasMoved = true;
            }
            //North east
            else if (action.IsActionReleased("NE"))
            {
                newPlayerPoint.x += 1;
                newPlayerPoint.y -= 1;
                hasMoved = true;
            }
            //East
            else if (action.IsActionReleased("E"))
            {
                newPlayerPoint.x += 1;
                hasMoved = true;
            }
            //South east
            else if (action.IsActionReleased("SE"))
            {
                newPlayerPoint.x += 1;
                newPlayerPoint.y += 1;
                hasMoved = true;
            }
            //South
            else if (action.IsActionReleased("S"))
            {
                newPlayerPoint.y += 1;
                hasMoved = true;
            }
            //South west
            else if (action.IsActionReleased("SW"))
            {
                newPlayerPoint.x -= 1;
                newPlayerPoint.y += 1;
                hasMoved = true;
            }
            //West
            else if (action.IsActionReleased("W"))
            {
                newPlayerPoint.x -= 1;
                hasMoved = true;
            }
            //North west
            else if (action.IsActionReleased("NW"))
            {
                newPlayerPoint.x -= 1;
                newPlayerPoint.y -= 1;
                hasMoved = true;
            }

            if (hasMoved)
            {
                this.AutoTurn = false;
                player.NeedFulfillmentData = new NeedFulfillmentData();
                
                PhysicsResult physicsResult = this.PhysicsManager.IsCollision(
                    player.WorldPosition, 
                    newPlayerPoint, 
                    this.m_ActiveWorld);

                if (physicsResult == PhysicsResult.EntityCollision)
                {
                    IEntity tempEntity = this.m_ActiveWorld.GetEntity(newPlayerPoint);
                    this.PlayerWorld.SwapPosition(player, tempEntity);
                    this.Tick();
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
        }

        protected void ToggleWindow(string name)
        {
            this.GUIManager.CloseGUI(this, GUINames.CONTEXT_MENU);
            this.GUIManager.CloseGUI(this, GUINames.TOOLTIP);
            this.GUIManager.ToggleGUI(this, name);
        }

        protected void Tick()
        {
            this.m_ActiveWorld.Tick();
            this.DrawObjects();
        }

        protected void DrawObjects()
        {
            var player = GlobalConstants.GameManager.Player;

            var playerVision = player.VisionProvider; 

            for (int i = 0; i < this.FogHolder.GetChildCount(); i++)
            {
                PositionableSprite fog = this.FogHolder.GetChild<PositionableSprite>(i);
                ShaderMaterial shaderMaterial = fog?.Material as ShaderMaterial;

                bool canSee = playerVision.HasVisibility(player, player.MyWorld, fog.WorldPosition);
                int lightLevel = this.m_ActiveWorld.LightCalculator.Light.GetLight(fog.WorldPosition);
                shaderMaterial?.SetShaderParam("canSee", canSee);
                shaderMaterial?.SetShaderParam("lightLevel", lightLevel);
            }

            for (int i = 0; i < this.GameManager.EntityHolder.GetChildCount(); i++)
            {
                var node = this.GameManager.EntityHolder.GetChild<JoyObjectNode>(i);
                bool canSee = playerVision.HasVisibility(player, player.MyWorld, node.WorldPosition);
                if (canSee != node.Visible)
                {
                    node.Visible = canSee;
                }
            }

            for (int i = 0; i < this.GameManager.ItemHolder.GetChildCount(); i++)
            {
                var node = this.GameManager.ItemHolder.GetChild<JoyObjectNode>(i);
                
                if (node.MyJoyObject is IItemInstance item)
                {
                    if (item.InWorld == false)
                    {
                        continue;
                    }
                    
                    bool canSee = playerVision.HasVisibility(player, player.MyWorld, node.WorldPosition) & item.InWorld;
                    if (canSee != node.Visible)
                    {
                        node.Visible = canSee;
                    }
                }
            }
        }

        public override GameState GetNextState()
        {
            this.TickTimer.Abort();
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