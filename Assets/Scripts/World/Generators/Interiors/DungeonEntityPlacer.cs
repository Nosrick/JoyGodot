using System;
using System.Collections.Generic;
using System.Linq;
using JoyLib.Code.Entities;
using JoyLib.Code.Physics;
using JoyLib.Code.Rollers;

namespace JoyLib.Code.World.Generators.Interiors
{
    public class DungeonEntityPlacer
    {
        protected RNG Roller { get; set; }
        protected ILiveEntityHandler EntityHandler { get; set; }
        protected IEntityTemplateHandler EntityTemplateHandler { get; set; }
        protected IPhysicsManager PhysicsManager { get; set; }
        protected IEntityFactory EntityFactory { get; set; }

        public DungeonEntityPlacer(
            ILiveEntityHandler entityHandler,
            IEntityTemplateHandler templateHandler,
            IPhysicsManager physicsManager,
            IEntityFactory entityFactory)
        {
            this.EntityFactory = entityFactory;
            this.EntityTemplateHandler = templateHandler;
            this.PhysicsManager = physicsManager;
            this.EntityHandler = entityHandler;
        }

        public IEnumerable<IEntity> PlaceEntities(IWorldInstance worldRef, IEnumerable<string> entityTypes, RNG roller)
        {
            this.Roller = roller;
            List<IEntity> entities = new List<IEntity>();

            List<IEntityTemplate> templates = this.EntityTemplateHandler.Values
                .Where(x => entityTypes.Contains(x.CreatureType, StringComparer.OrdinalIgnoreCase)).ToList();

            int numberToPlace = (worldRef.Tiles.GetLength(0) * worldRef.Tiles.GetLength(1)) / 50;
            //int numberToPlace = 1;

            List<Vector2Int> availablePoints = new List<Vector2Int>();

            for (int i = 0; i < worldRef.Tiles.GetLength(0); i++)
            {
                for (int j = 0; j < worldRef.Tiles.GetLength(1); j++)
                {
                    Vector2Int point = new Vector2Int(i, j);
                    if (this.PhysicsManager.IsCollision(point, point, worldRef) == PhysicsResult.None 
                        && point != worldRef.SpawnPoint
                        && worldRef.Areas.ContainsKey(point) == false)
                    {
                        availablePoints.Add(point);
                    }
                }
            }

            for (int i = 0; i < numberToPlace; i++)
            {
                int pointIndex = this.Roller.Roll(0, availablePoints.Count);

                int entityIndex = this.Roller.Roll(0, templates.Count);

                IEntity newEntity = this.EntityFactory.CreateFromTemplate(
                    templates[entityIndex], 
                    availablePoints[pointIndex],
                    null,
                    null,
                    null, 
                    null,
                    null,
                    null,
                    null,
                    null, 
                    null,
                    null,
                    null, 
                    null,
                    worldRef);

                this.EntityHandler.Add(newEntity);
                entities.Add(newEntity);

                availablePoints.RemoveAt(pointIndex);
            }

            return entities;
        }
    }
}
