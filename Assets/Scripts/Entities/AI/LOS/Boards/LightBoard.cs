using System.Collections.Generic;
using Godot;

namespace JoyLib.Code.Entities.AI.LOS
{
    public class LightBoard
    {
        public int[,] LightLevels { get; protected set; }
        
        protected bool[,] Walls { get; set; }
        protected bool[,] Visited { get; set; }
        
        public int Width { get; protected set; }
        public int Height { get; protected set; }

        public LightBoard(int width, int height, IEnumerable<Vector2Int> walls)
        {
            this.Width = width;
            this.Height = height;
            this.Walls = new bool[width, height];
            this.Visited = new bool[width, height];
            this.LightLevels = new int[width, height];

            foreach (Vector2Int wall in walls)
            {
                this.Walls[wall.x, wall.y] = true;
            }
        }

        public bool Visit(Vector2Int position)
        {
            this.Visited[position.x, position.y] = true;
            return true;
        }

        public bool HasVisited(Vector2Int position)
        {
            return this.Visited[position.x, position.y];
        }

        public int AddLight(Vector2Int position, int lightLevel)
        {
            if (this.HasVisited(position))
            {
                return this.LightLevels[position.x, position.y];
            }

            this.LightLevels[position.x, position.y] = Mathf.Min(this.LightLevels[position.x, position.y] + lightLevel, GlobalConstants.MAX_LIGHT);

            this.Visited[position.x, position.y] = true;
            return this.LightLevels[position.x, position.y];
        }

        public int DiffuseLight(Vector2Int position, int lightLevel)
        {
            /*
            if (this.HasVisited(position))
            {
                return this.LightLevels[position.x, position.y];
            }
            */

            this.LightLevels[position.x, position.y] = Mathf.Clamp(
                lightLevel, 
                this.LightLevels[position.x, position.y], 
                GlobalConstants.MAX_LIGHT);

            this.Visited[position.x, position.y] = true;
            return this.LightLevels[position.x, position.y];
        }

        public int GetLight(Vector2Int position)
        {
            return this.LightLevels[position.x, position.y];
        }

        public bool IsObstacle(Vector2Int position)
        {
            return this.Walls[position.x, position.y];
        }

        public void ClearVisited()
        {
            this.Visited = new bool[this.Width, this.Height];
        }

        public void ClearBoard()
        {
            this.LightLevels = new int[this.Width, this.Height];
            this.Visited = new bool[this.Width, this.Height];
        }
    }
}