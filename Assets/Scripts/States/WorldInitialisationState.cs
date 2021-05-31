using System;
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
        { }

        public override void Start()
        {
            this.InstantiateWorld();
        }

        public override void Stop()
        { }

        public override void Update()
        { }

        public override void HandleInput(InputEvent @event)
        { }

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

            ISpriteState state = null;
            float scale = (float) GlobalConstants.SPRITE_WORLD_SIZE / GlobalConstants.SPRITE_TEXTURE_SIZE;

            var floorTileMap = gameManager.FloorTileMap;
            floorTileMap.TileSet = this.m_ObjectIcons.GetStaticTileSet(this.m_ActiveWorld.Tiles[0, 0].TileSet, true);
            int surroundFloorIndex = floorTileMap.TileSet.FindTileByName("SurroundFloor");

            var wallTileMap = gameManager.WallTileMap;
            wallTileMap.TileSet = this.m_ObjectIcons.GetStaticTileSet(this.m_ActiveWorld.Tiles[0, 0].TileSet);
            int surroundWallIndex = wallTileMap.TileSet.FindTileByName("SurroundWall");
            
            for (int i = 0; i < this.m_ActiveWorld.Tiles.GetLength(0); i++)
            {
                for (int j = 0; j < this.m_ActiveWorld.Tiles.GetLength(1); j++)
                {
                    Vector2Int intPos = new Vector2Int(i, j);

                    //Make the fog of war
                    PositionableSprite fog = gameManager.FogPool.Get();
                    fog.Name = "Fog of War " + intPos;
                    fog.Show();
                    fog.Scale = new Vector2(scale, scale);
                    fog.Move(intPos);

                    //Make the floor
                    floorTileMap.SetCell(i, j, surroundFloorIndex);
                }
            }

            //Make the upstairs
            if (this.m_ActiveWorld.Guid != this.m_Overworld.Guid)
            {
                floorTileMap.SetCellv(
                    this.m_ActiveWorld.SpawnPoint.ToVec2, 
                    floorTileMap.TileSet.FindTileByName("upstairs"));
            }

            //Make each downstairs
            foreach (KeyValuePair<Vector2Int, IWorldInstance> pair in this.m_ActiveWorld.Areas)
            {
                floorTileMap.SetCellv(
                    pair.Key.ToVec2, 
                    floorTileMap.TileSet.FindTileByName("downstairs"));
            }

            //Create the walls
            foreach (Vector2Int position in this.m_ActiveWorld.Walls)
            {
                wallTileMap.SetCellv(
                    position.ToVec2,
                    surroundWallIndex);
            }

            this.CreateItems(this.m_ActiveWorld.Items);

            //Create the entities
            foreach (IEntity entity in this.m_ActiveWorld.Entities)
            {
                JoyObjectNode gameObject = gameManager.EntityPool.Get();
                gameObject.Show();
                gameObject.AttachJoyObject(entity);
                this.CreateItems(entity.Contents, false);
                this.CreateItems(entity.Equipment.Contents, false);
            }

            this.Done = true;
        }

        protected void CreateItems(IEnumerable<IItemInstance> items, bool active = true)
        {
            if (items.Any() == false)
            {
                return;
            }

            IGameManager gameManager = GlobalConstants.GameManager;
            foreach (IItemInstance itemInstance in items)
            {
                itemInstance.Instantiate(true, gameManager.ItemPool.Get(), active);
                if (itemInstance.Contents.IsNullOrEmpty() == false)
                {
                    this.CreateItems(itemInstance.Contents, false);
                }
            }
        }

        public override GameState GetNextState()
        {
            return new WorldState(this.m_Overworld, this.m_ActiveWorld);
        }
    }
}