using System.Collections.Generic;
using JoyGodot.Assets.Scripts.Entities;
using JoyGodot.Assets.Scripts.Items;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.Rollers;
using JoyGodot.Assets.Scripts.World.WorldInfo;

namespace JoyGodot.Assets.Scripts.World.Generators.Interiors
{
    public class DungeonGenerator
    {
        public WorldInstance GenerateDungeon(
            WorldInfo.WorldInfo worldInfo, 
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
                gameManager.CultureHandler,
                gameManager.EntityTemplateHandler, 
                gameManager.PhysicsManager, 
                gameManager.EntityFactory);

            List<string> entitiesToPlace = new List<string>(worldInfo.inhabitants);
            List<string> cultures = new List<string>(worldInfo.cultures);

            WorldInstance root = null;
            WorldInstance current = null;
            for (int i = 1; i <= levels; i++)
            {
                WorldTile[,] tiles = interiorGenerator.GenerateWorldSpace(size, worldInfo.name);
                HashSet<Vector2Int> walls = interiorGenerator.GenerateWalls(tiles);
                WorldInstance worldInstance = new WorldInstance(
                    tiles, 
                    worldInfo.tags, 
                    worldInfo.name + " " + i, 
                    gameManager.EntityHandler, 
                    roller);
                foreach(Vector2Int wall in walls)
                {
                    worldInstance.AddWall(wall);
                }

                List<IItemInstance> items = itemPlacer.PlaceItems(worldInstance);

                IEnumerable<IEntity> entities = entityPlacer.PlaceEntities(
                    worldInstance, 
                    cultures, 
                    entitiesToPlace, 
                    roller);
                
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
