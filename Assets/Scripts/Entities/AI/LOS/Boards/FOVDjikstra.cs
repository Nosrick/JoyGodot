using System.Collections.Generic;
using JoyLib.Code.World;

namespace JoyLib.Code.Entities.AI.LOS
{
    public class FOVDjikstra : IFOVHandler
    {
        protected int VisionMod { get; set; }
        protected IEntity Viewer { get; set; }
        
        protected FOVArrayBoard Board { get; set; }
        
        public IFOVBoard Do(IEntity viewer, IWorldInstance world, Vector2Int dimensions, IEnumerable<Vector2Int> walls)
        {
            this.Viewer = viewer;
            this.VisionMod = viewer.VisionMod;
            this.Board = new FOVArrayBoard(dimensions.x, dimensions.y, walls);
            this.Board.Visible(this.Viewer.WorldPosition.x, this.Viewer.WorldPosition.y);

            this.DoAdjacent(viewer.WorldPosition, 0);

            return this.Board;
        }

        public LinkedList<Vector2Int> HasLOS(Vector2Int origin, Vector2Int target)
        {
            throw new System.NotImplementedException();
        }
        
        protected void DoAdjacent(Vector2Int position, int distance)
        {
            if (distance > this.VisionMod)
            {
                return;
            }
            
            List<Vector2Int> adjacent = new List<Vector2Int>
            {
                new Vector2Int(position.x - 1, position.y),
                new Vector2Int(position.x + 1, position.y),
                new Vector2Int(position.x, position.y + 1),
                new Vector2Int(position.x, position.y - 1)
            };
            this.Board.Visible(position.x, position.y);
            foreach (Vector2Int point in adjacent)
            { 
                if (!this.Contains(point))
                {
                    continue;
                }

                /*if (this.Board.HasVisited(point.x, point.y))
                {
                    continue;
                }*/

                if (this.Board.IsObstacle(point.x, point.y))
                {
                    this.Board.Visible(point.x, point.y);
                    continue;
                }
                
                this.DoAdjacent(point, distance + 1);
            }
        }

        protected bool Contains(Vector2Int point)
        {
            return point.x >= 0
                   && point.x < this.Board.Width
                   && point.y >= 0
                   && point.y < this.Board.Height;
        }
    }
}