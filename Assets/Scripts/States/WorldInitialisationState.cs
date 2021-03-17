﻿using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Godot;
using JoyLib.Code.Entities;
using JoyLib.Code.Entities.Items;
using JoyLib.Code.Entities.Statistics;
using JoyLib.Code.Godot;
using JoyLib.Code.Graphics;
using JoyLib.Code.Unity;
using JoyLib.Code.World;

namespace JoyLib.Code.States
{
    public class WorldInitialisationState : GameState
    {
        protected IWorldInstance m_Overworld;
        protected IWorldInstance m_ActiveWorld;

        protected IObjectIconHandler m_ObjectIcons = GlobalConstants.GameManager.ObjectIconHandler;

        public WorldInitialisationState(IWorldInstance overWorld, IWorldInstance activeWorld)
        {
            this.m_Overworld = overWorld;
            this.m_ActiveWorld = activeWorld;
        }

        public override void LoadContent()
        {
        }

        public override void Start()
        {
            this.InstantiateWorld();
        }

        public override void Stop()
        {
        }

        public override void Update()
        {
        }

        public override void HandleInput(InputEvent @event)
        {
        }

        protected void InstantiateWorld()
        {
            IGameManager gameManager = GlobalConstants.GameManager;
            List<IBasicValue<float>> values = new List<IBasicValue<float>>
            {
                new ConcreteBasicFloatValue("weight", 1),
                new ConcreteBasicFloatValue("bonus", 1),
                new ConcreteBasicFloatValue("size", 1),
                new ConcreteBasicFloatValue("hardness", 1),
                new ConcreteBasicFloatValue("density", 1)
            };
            
            this.m_ActiveWorld.Initialise();
            //int terrainLayer = LayerMask.NameToLayer("Terrain");
            //Make the upstairs
            if (this.m_ActiveWorld.Guid != this.m_Overworld.Guid)
            {
                Node2D child = gameManager.FloorPool.Get();
                child.Position = new Vector2(this.m_ActiveWorld.SpawnPoint.x, this.m_ActiveWorld.SpawnPoint.y);
                child.Name = this.m_ActiveWorld.Parent.Name + " stairs";
                //TooltipComponent tooltip = child.GetComponent<TooltipComponent>();
                //tooltip.WorldPosition = this.m_ActiveWorld.SpawnPoint;
                //tooltip.RefreshTooltip = WorldState.GetTooltipData;

                ManagedSprite sprite = (ManagedSprite) child;
                sprite.Clear();
                sprite.AddSpriteState(new SpriteState(
                        child.Name,
                    this.m_ObjectIcons.GetSprites("Stairs", "Upstairs").First()));
                child.Visible = true;
            }

            //Make each downstairs
            foreach(KeyValuePair<Vector2Int, IWorldInstance> pair in this.m_ActiveWorld.Areas)
            {
                ManagedSprite child = gameManager.FloorPool.Get();
                child.Name = pair.Value.Name + " stairs";
                child.Position = new Vector2(pair.Key.x, pair.Key.y);
                //TooltipComponent tooltip = child.GetComponent<TooltipComponent>();
                //tooltip.WorldPosition = pair.Key;
                child.Clear();
                child.AddSpriteState(new SpriteState(
                        child.Name,
                    this.m_ObjectIcons.GetSprites("Stairs", "Downstairs").First()), 
                    true);
                child.Visible = true;
            }

            ISpriteState state = null;
            for(int i = 0; i < this.m_ActiveWorld.Tiles.GetLength(0); i++)
            {
                for(int j = 0; j < this.m_ActiveWorld.Tiles.GetLength(1); j++)
                {
                    Vector2Int intPos = new Vector2Int(i, j);
                    
                    //Make the fog of war
                    Sprite fog = gameManager.FogPool.Get();
                    fog.Name = "Fog of War";
                    fog.Visible = true;
                    
                    
                    //Make the floor
                    ManagedSprite floor = gameManager.FloorPool.Get();
                    floor.Name = this.m_ActiveWorld.Name + " floor";
                    //TooltipComponent tooltip = floor.GetComponent<TooltipComponent>();
                    //tooltip.Move(intPos);
                    //tooltip.RefreshTooltip = WorldState.GetTooltipData;
                    if (state is null
                        || state.SpriteData.m_Name.Equals(this.m_ActiveWorld.Tiles[i, j].TileSet,
                            StringComparison.OrdinalIgnoreCase) == false)
                    {
                        state = new SpriteState(
                            "Floor",
                            this.m_ObjectIcons.GetSprites(this.m_ActiveWorld.Tiles[i, j].TileSet, "surroundfloor").First());
                    }
                    floor.Clear();
                    floor.AddSpriteState(state);
                    floor.Visible = true;
                }
            }
            
            //Create the walls
            foreach(IJoyObject wall in this.m_ActiveWorld.Walls.Values)
            {
                wall.MyWorld = this.m_ActiveWorld;
                JoyObjectNode gameObject = gameManager.WallPool.Get();
                gameObject.AttachJoyObject(wall);
                gameObject.Clear();
                gameObject.AddSpriteState(wall.States.First(), true);
                gameObject.Visible = true;
            }
            
            this.CreateItems(this.m_ActiveWorld.Objects);

            //Create the entities
            foreach(IEntity entity in this.m_ActiveWorld.Entities)
            {
                JoyObjectNode gameObject = gameManager.EntityPool.Get();
                gameObject.Visible = true;
                gameObject.AttachJoyObject(entity);

                gameObject.Clear();
                gameObject.AddSpriteState(entity.States.First(), true);
                this.CreateItems(entity.Contents, false);
                this.CreateItems(entity.Equipment.Contents, false);
            }

            this.Done = true;
        }

        protected void CreateItems(IEnumerable<IJoyObject> items, bool active = true)
        {
            if (items.Any() == false)
            {
                return;
            }
            
            IGameManager gameManager = GlobalConstants.GameManager;
            foreach (IJoyObject item in items)
            {
                if (item is ItemInstance itemInstance)
                {
                    itemInstance.Instantiate(true, gameManager.ItemPool.Get(), active);
                    if (itemInstance.Contents.IsNullOrEmpty() == false)
                    {
                        this.CreateItems(itemInstance.Contents, false);
                    }
                }
            }
        }

        public override GameState GetNextState()
        {
            return new WorldState(this.m_Overworld, this.m_ActiveWorld);
        }
    }
}