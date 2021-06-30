using System.Collections.Generic;
using System.Linq;

using Godot;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Entities.Items;
using JoyGodot.Assets.Scripts.Entities.Statistics;
using JoyGodot.Assets.Scripts.Godot;
using JoyGodot.Assets.Scripts.Graphics;
using JoyGodot.Assets.Scripts.Helpers;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Managed_Assets;
using JoyGodot.Assets.Scripts.World;

namespace JoyGodot.Assets.Scripts.States
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
                    this.m_ActiveWorld.SpawnPoint.ToVec2(), 
                    floorTileMap.TileSet.FindTileByName("upstairs"));
            }

            //Make each downstairs
            foreach (KeyValuePair<Vector2Int, IWorldInstance> pair in this.m_ActiveWorld.Areas)
            {
                floorTileMap.SetCellv(
                    pair.Key.ToVec2(), 
                    floorTileMap.TileSet.FindTileByName("downstairs"));
            }

            //Create the walls
            foreach (Vector2Int position in this.m_ActiveWorld.Walls)
            {
                wallTileMap.SetCellv(
                    position.ToVec2(),
                    surroundWallIndex);
            }

            int index = 0;
            index = this.CreateItems(index, this.m_ActiveWorld.Items);

            GlobalConstants.GameManager.EntityHolder.ZIndex = index + 10;
            //Create the entities
            int innerIndex = 0;
            int itemIndex = 0;
            foreach (IEntity entity in this.m_ActiveWorld.Entities)
            {
                JoyObjectNode gameObject = gameManager.EntityPool.Get();
                gameObject.Show();
                gameObject.AttachJoyObject(entity);
                gameObject.ZIndex = index;
                innerIndex += gameObject.CurrentSpriteState.SpriteData.Parts.Max(part => part.m_SortingOrder) + 1;
                itemIndex = this.CreateItems(itemIndex, entity.Contents, false);
                itemIndex = this.CreateItems(itemIndex, entity.Equipment.Contents, false);
            }

            GlobalConstants.GameManager.FogHolder.ZIndex = index + innerIndex + 10;

            this.Done = true;
        }

        protected int CreateItems(int index, IEnumerable<IItemInstance> items, bool active = true)
        {
            if (items.Any() == false)
            {
                return index;
            }

            IGameManager gameManager = GlobalConstants.GameManager;
            foreach (IItemInstance itemInstance in items)
            {
                itemInstance.Instantiate(true, gameManager.ItemPool.Get(), active);
                itemInstance.MyNode.ZIndex = index;
                index += itemInstance.MyNode.CurrentSpriteState.SpriteData.Parts.Max(part => part.m_SortingOrder) + 1;
                if (itemInstance.Contents.IsNullOrEmpty() == false)
                {
                    index = this.CreateItems(index, itemInstance.Contents, false);
                }
            }

            return index;
        }

        public override GameState GetNextState()
        {
            return new WorldState(this.m_Overworld, this.m_ActiveWorld);
        }
    }
}