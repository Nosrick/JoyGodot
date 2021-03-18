using System.Collections.Generic;
using JoyLib.Code.Entities;
using JoyLib.Code.Entities.Items;
using JoyLib.Code.Rollers;

namespace JoyLib.Code.World.Generators.Interiors
{
    public class DungeonGenerator
    {
        public WorldInstance GenerateDungeon(
            WorldInfo worldInfo, 
            int size, 
            int levels, 
            IGameManager gameManager,
            RNG roller)
        {
            DungeonInteriorGenerator interiorGenerator = new DungeonInteriorGenerator(
                gameManager.GUIDManager,
                gameManager.ObjectIconHandler,
                gameManager.DerivedValueHandler,
                gameManager.WorldInfoHandler,
                roller);
            SpawnPointPlacer spawnPointPlacer = new SpawnPointPlacer(roller);
            DungeonItemPlacer itemPlacer = new DungeonItemPlacer(
                gameManager.ItemHandler, 
                roller,
                gameManager.ItemFactory);
            
            DungeonEntityPlacer entityPlacer = new DungeonEntityPlacer(
                gameManager.EntityHandler, 
                gameManager.EntityTemplateHandler, 
                gameManager.PhysicsManager, 
                gameManager.EntityFactory);

            List<string> entitiesToPlace = new List<string>();
            entitiesToPlace.AddRange(worldInfo.inhabitants);

            WorldInstance root = null;
            WorldInstance current = null;
            for (int i = 1; i <= levels; i++)
            {
                WorldTile[,] tiles = interiorGenerator.GenerateWorldSpace(size, worldInfo.name);
                WorldInstance worldInstance = new WorldInstance(
                    tiles, 
                    worldInfo.tags, 
                    worldInfo.name + " " + i, 
                    gameManager.EntityHandler, 
                    roller);

                List<JoyObject> walls = interiorGenerator.GenerateWalls(tiles);
                foreach(JoyObject wall in walls)
                {
                    worldInstance.AddObject(wall);
                }

                List<IItemInstance> items = itemPlacer.PlaceItems(worldInstance);

                IEnumerable<IEntity> entities = entityPlacer.PlaceEntities(worldInstance, entitiesToPlace, roller);
                foreach(IEntity entity in entities)
                {
                    worldInstance.AddEntity(entity);
                }

                //Do the spawn points
                worldInstance.SpawnPoint = spawnPointPlacer.PlaceSpawnPoint(worldInstance);

                //Use this as our root if we don't have one
                if(root == null)
                {
                    root = worldInstance;
                }

                //Link to the previous floor
                if(current != null)
                {
                    current.AddArea(spawnPointPlacer.PlaceTransitionPoint(current), worldInstance);
                }
                current = worldInstance;
            }

            return root;
        }
    }
}
