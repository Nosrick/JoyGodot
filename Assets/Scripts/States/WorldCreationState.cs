using System;
using System.Collections.Generic;
using Godot;
using JoyGodot.Assets.Scripts.Calendar;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.IO;
using JoyGodot.Assets.Scripts.Items;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Scripting;
using JoyGodot.Assets.Scripts.World;
using JoyGodot.Assets.Scripts.World.Generators;
using JoyGodot.Assets.Scripts.World.Generators.Interiors;
using JoyGodot.Assets.Scripts.World.Generators.Overworld;
using JoyGodot.Assets.Scripts.World.WorldInfo;

namespace JoyGodot.Assets.Scripts.States
{
    public class WorldCreationState : GameState
    {
        protected IEntity m_Player;

        protected IWorldInstance m_World;
        protected IWorldInstance m_ActiveWorld;

        protected IWorldInfoHandler m_WorldInfoHandler = GlobalConstants.GameManager.WorldInfoHandler;

        protected const int SIMULATION_TICKS = 600;

        protected const int WORLD_SIZE = 35;

        public WorldCreationState(IEntity playerRef) : base()
        {
            this.m_Player = playerRef;
        }

        public override void Start()
        {
            this.CreateWorld();
        }

        public override void LoadContent()
        {
        }

        private void CreateWorld()
        {
            //Make a new overworld generator
            OverworldGenerator overworldGen = new OverworldGenerator(this.m_WorldInfoHandler);
            overworldGen.GenerateWorldSpace(WORLD_SIZE, "plains");
            WorldTile[,] tiles = overworldGen.Tiles;

            //Generate the basic overworld
            this.m_World = new WorldInstance(
                "overworld-floor",
                "overworld-walls",
                tiles,
                new[] {"overworld", "exterior"},
                "Everse",
                GlobalConstants.GameManager.EntityHandler,
                GlobalConstants.GameManager.Roller);

            //Set the date and time for 1/1/1555, 12:00pm
            this.m_World.SetDateTime(new JoyDateTime(1555, 1, 1, 12));

            //Do the spawn point
            SpawnPointPlacer spawnPlacer = new SpawnPointPlacer(GlobalConstants.GameManager.Roller);
            Vector2Int spawnPoint = spawnPlacer.PlaceSpawnPoint(this.m_World);
            while ((spawnPoint.x == -1 && spawnPoint.y == -1))
            {
                spawnPoint = spawnPlacer.PlaceSpawnPoint(this.m_World);
            }

            this.m_World.SpawnPoint = spawnPoint;

            //Set up the player
            //m_Player.Move(m_World.SpawnPoint);
            //m_World.AddEntity(m_Player);

            //Begin the first floor of the Naga Pits
            WorldInfo worldInfo = this.m_WorldInfoHandler.GetRandom("interior");

            DungeonGenerator dungeonGenerator = new DungeonGenerator();
            WorldInstance dungeon = dungeonGenerator.GenerateDungeon(
                worldInfo,
                WORLD_SIZE,
                3,
                GlobalConstants.GameManager,
                GlobalConstants.GameManager.Roller);

            Vector2Int transitionPoint = spawnPlacer.PlaceTransitionPoint(this.m_World);
            this.m_World.AddArea(transitionPoint, dungeon);
            this.Done = true;

            this.m_ActiveWorld = dungeon;
            this.m_Player.Move(dungeon.SpawnPoint);
            dungeon.AddEntity(this.m_Player);

            GlobalConstants.GameManager.EntityHandler.Add(this.m_Player);

            IItemInstance lightSource = GlobalConstants.GameManager.ItemFactory.CreateRandomItemOfType(
                new string[] {"light source"},
                true);

            IItemInstance bag = GlobalConstants.GameManager.ItemFactory.CreateRandomItemOfType(
                new[] {"container"},
                true);
            
            IJoyAction addItemAction = this.m_Player.FetchAction("additemaction");
            addItemAction.Execute(
                new IJoyObject[] {this.m_Player, lightSource},
                new[] {"pickup"},
                new Dictionary<string, object>
                {
                    {"newOwner", true}
                });

            addItemAction.Execute(
                new IJoyObject[] {this.m_Player, bag},
                new[] {"pickup"},
                new Dictionary<string, object>
                {
                    {"newOwner", true}
                });

            for (int i = 0; i < 4; i++)
            {
                IItemInstance newItem = GlobalConstants.GameManager.ItemFactory.CreateRandomWeightedItem(
                    true,
                    false);
                addItemAction.Execute(
                    new IJoyObject[]
                    {
                        this.m_Player,
                        newItem
                    },
                    new[] {"pickup"},
                    new Dictionary<string, object>
                    {
                        {"newOwner", true}
                    });
            }

            this.m_World.Tick();

            GlobalConstants.GameManager.WorldHandler.Add(this.m_World);

            WorldSerialiser worldSerialiser = new WorldSerialiser(GlobalConstants.GameManager.ObjectIconHandler);
            worldSerialiser.Serialise(this.m_World);

            this.Done = true;
        }

        protected void SimulateWorld()
        {
            List<IWorldInstance> worlds = this.m_World.GetWorlds(this.m_World);
            for (int a = 0; a < worlds.Count; a++)
            {
                for (int i = 0; i < SIMULATION_TICKS; i++)
                {
                    worlds[a].Tick();
                }
            }
        }

        public override void Update()
        {
        }

        public override void HandleInput(InputEvent @event)
        {
        }

        public override GameState GetNextState()
        {
            //return new WorldInitialisationState(m_World, m_World);
            return new WorldInitialisationState(this.m_World, this.m_ActiveWorld);
        }
    }
}