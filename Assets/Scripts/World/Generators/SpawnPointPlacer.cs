using System.Collections.Generic;
using System.Linq;
using JoyLib.Code.Entities.AI;
using JoyLib.Code.Rollers;

namespace JoyLib.Code.World.Generators
{
    public class SpawnPointPlacer
    {
        protected RNG Roller { get; set; }

        public SpawnPointPlacer(RNG roller)
        {
            this.Roller = roller;
        }
        
        public Vector2Int PlaceSpawnPoint(IWorldInstance worldRef)
        {
            int x, y;

            x = this.Roller.Roll(1, worldRef.Tiles.GetLength(0));
            y = this.Roller.Roll(1, worldRef.Tiles.GetLength(1));

            Vector2Int point = new Vector2Int(x, y);

            while (worldRef.Walls.Any(l => l.Equals(point)))
            {
                x = this.Roller.Roll(1, worldRef.Tiles.GetLength(0));
                y = this.Roller.Roll(1, worldRef.Tiles.GetLength(1));
                point = new Vector2Int(x, y);
            }

            return point;
        }

        public Vector2Int PlaceTransitionPoint(IWorldInstance worldRef)
        {
            int breakout = (worldRef.Tiles.GetLength(0) * worldRef.Tiles.GetLength(1)) / 4;
            int x, y;

            x = this.Roller.Roll(1, worldRef.Tiles.GetLength(0));
            y = this.Roller.Roll(1, worldRef.Tiles.GetLength(1));

            Vector2Int point = new Vector2Int(x, y);

            int count = 0;
            while (worldRef.Walls.Contains(point) && 
                (point.Equals(worldRef.SpawnPoint) || count < breakout))
            {
                x = this.Roller.Roll(1, worldRef.Tiles.GetLength(0));
                y = this.Roller.Roll(1, worldRef.Tiles.GetLength(1));
                point = new Vector2Int(x, y);
                count += 1;
            }

            Pathfinder pathfinder = new Pathfinder();
            Queue<Vector2Int> points = pathfinder.FindPath(point, worldRef.SpawnPoint, worldRef);

            if (points.Count > 0)
            {
                return point;
            }
            else
            {
                count = 0;
                while (worldRef.Walls.Contains(point) 
                        && (point.Equals(worldRef.SpawnPoint) || count < breakout) 
                        && pathfinder.FindPath(point, worldRef.SpawnPoint, worldRef).Count == 0)
                {
                    x = this.Roller.Roll(1, worldRef.Tiles.GetLength(0));
                    y = this.Roller.Roll(1, worldRef.Tiles.GetLength(1));
                    point = new Vector2Int(x, y);
                    count += 1;
                }

                if (count < breakout)
                {
                    return point;
                }
            }

            return new Vector2Int(-1, -1);
        }

        private bool CanReachPoint(Vector2Int fromRef, Vector2Int toRef, WorldInstance worldRef)
        {
            Dictionary<Vector2Int, IJoyObject> walls = worldRef.GetObjectsOfType(new string[] { "wall" });
            bool[,] blocked = new bool[worldRef.Tiles.GetLength(0), worldRef.Tiles.GetLength(1)];

            for (int i = 0; i < blocked.GetLength(0); i++)
            {
                for (int j = 0; j < blocked.GetLength(1); j++)
                {
                    if (walls.Keys.Any(x => x == new Vector2Int(i, j)))
                    {
                        blocked[i, j] = true;
                    }
                }
            }

            return this.ReachOut(fromRef, toRef, blocked, blocked);
        }

        private bool ReachOut(Vector2Int point, Vector2Int destination, bool[,] blockedTiles, bool[,] steppedTiles)
        {
            if (point.x <= 0)
                return false;

            if (point.x >= blockedTiles.GetLength(0))
                return false;

            if (point.y <= 0)
                return false;

            if (point.y >= blockedTiles.GetLength(1))
                return false;

            if (blockedTiles[point.x, point.y])
                return false;

            if (destination.x == point.x && destination.y == point.y)
            {
                return true;
            }
            else
            {
                steppedTiles[point.x, point.y] = true;

                //North
                if(!steppedTiles[point.x, point.y - 1])
                    if (this.ReachOut(new Vector2Int(point.x, point.y - 1), destination, blockedTiles, steppedTiles))
                        return true;

                //North East
                if (!steppedTiles[point.x + 1, point.y - 1])
                    if (this.ReachOut(new Vector2Int(point.x + 1, point.y - 1), destination, blockedTiles, steppedTiles))
                         return true;

                //East
                if (!steppedTiles[point.x + 1, point.y])
                    if (this.ReachOut(new Vector2Int(point.x + 1, point.y), destination, blockedTiles, steppedTiles))
                        return true;

                //South East
                if (!steppedTiles[point.x + 1, point.y + 1])
                    if (this.ReachOut(new Vector2Int(point.x + 1, point.y + 1), destination, blockedTiles, steppedTiles))
                        return true;

                //South
                if (!steppedTiles[point.x, point.y + 1])
                    if (this.ReachOut(new Vector2Int(point.x, point.y + 1), destination, blockedTiles, steppedTiles))
                        return true;

                //South West
                if (!steppedTiles[point.x - 1, point.y + 1])
                    if (this.ReachOut(new Vector2Int(point.x - 1, point.y + 1), destination, blockedTiles, steppedTiles))
                        return true;

                //West
                if (!steppedTiles[point.x - 1, point.y])
                    if (this.ReachOut(new Vector2Int(point.x - 1, point.y), destination, blockedTiles, steppedTiles))
                        return true;

                //North West
                if (!steppedTiles[point.x - 1, point.y - 1])
                    if (this.ReachOut(new Vector2Int(point.x - 1, point.y - 1), destination, blockedTiles, steppedTiles))
                        return true;
            }
            return false;
        }
    }
}
