using System;
using System.Collections.Generic;
using JoyGodot.Assets.Scripts.JoyObject;

namespace JoyGodot.Assets.Scripts.Entities.AI.LOS.Boards
{
    public class FOVHashSetBoard : IFOVBoard
    {
        protected HashSet<Vector2Int> Visited { get; set; } 
        protected HashSet<Vector2Int> VisiblePoints { get; set; }
        protected HashSet<Vector2Int> Walls { get; set; }

        protected int m_Width, m_Height;

        public FOVHashSetBoard(int width, int height, IEnumerable<Vector2Int> walls)
        {
            this.m_Width = width;
            this.m_Height = height;

            this.Visited = new HashSet<Vector2Int>();
            this.VisiblePoints = new HashSet<Vector2Int>();
            this.Walls = new HashSet<Vector2Int>(walls);
        }

        public bool HasVisited(int x, int y)
        {
            return this.Visited.Contains(new Vector2Int(x, y));
        }

        public void Visit(int x, int y)
        {
            Vector2Int point = new Vector2Int(x, y);
            if (x >= 0 && x < this.m_Width 
               && y >= 0 && y < this.m_Height 
               && this.Visited.Contains(point) == false)
            {
                this.Visited.Add(point);
            }
        }

        public void Visible(int x, int y)
        {
            Vector2Int point = new Vector2Int(x, y);
            if (x >= 0 && x < this.m_Width 
                && y >= 0 && y < this.m_Height)
            {
                this.Visited.Add(point);
                this.VisiblePoints.Add(point);
            }
        }

        public bool IsVisible(int x, int y)
        {
            return this.VisiblePoints.Contains(new Vector2Int(x, y));
        }

        public void Block(int x, int y)
        {
            Vector2Int point = new Vector2Int(x, y);
            if (x >= 0 && x < this.m_Width 
                && y >= 0 && y < this.m_Height)
            {
                this.Visited.Add(point);
                this.VisiblePoints.Remove(point);
            }
        }

        public bool IsBlocked(int x, int y)
        {
            Vector2Int point = new Vector2Int(x, y);
            if (x >= 0 && x < this.m_Width 
                && y >= 0 && y < this.m_Height)
            {
                return (this.VisiblePoints.Contains(point) == false) || this.IsObstacle(x, y);
            }
            return true;
        }

        public bool IsObstacle(int x, int y)
        {
            Vector2Int point = new Vector2Int(x, y);
            if (x >= 0 && x < this.m_Width 
                && y >= 0 && y < this.m_Height)
            {
                return this.Walls.Contains(point);
            }
            return true;
        }

        public double Radius(int deltaX, int deltaY)
        {
            return Math.Round(Math.Sqrt(deltaX * deltaX + deltaY * deltaY));
        }

        public void ClearBoard()
        {
            this.VisiblePoints = new HashSet<Vector2Int>();
            this.Visited = new HashSet<Vector2Int>();
        }

        public bool Contains(int x, int y)
        {
            return (x >= 0 && x < this.m_Width && y >= 0 && y < this.m_Height);
        }

        public IEnumerable<Vector2Int> GetVision()
        {
            return this.VisiblePoints;
        }

        public int Width
        {
            get
            {
                return this.m_Width;
            }
        }

        public int Height
        {
            get
            {
                return this.m_Height;
            }
        }
    }
}
