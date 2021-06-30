using System;
using System.Collections.Generic;
using JoyGodot.Assets.Scripts.JoyObject;

namespace JoyGodot.Assets.Scripts.Entities.AI.LOS.Boards
{
    [Serializable]
    public class FOVArrayBoard : IFOVBoard
    {
         
        protected bool[,] Visited { get; set; } 
        
         
        protected bool[,] VisiblePoints { get; set; }
        
         
        protected HashSet<Vector2Int> Walls { get; set; }

         
        protected int m_Width, m_Height;

        public FOVArrayBoard(int width, int height, IEnumerable<Vector2Int> walls)
        {
            this.m_Width = width;
            this.m_Height = height;

            this.Visited = new bool[width, height];
            this.VisiblePoints =  new bool[width, height];
            this.Walls = new HashSet<Vector2Int>(walls);
        }

        public bool HasVisited(int x, int y)
        {
            return this.Visited[x, y];
        }

        public void Visit(int x, int y)
        {
            if (x >= 0 && x < this.m_Width 
               && y >= 0 && y < this.m_Height 
               && this.Visited[x, y] == false)
            {
                this.Visited[x, y] = true;
            }
        }

        public void Visible(int x, int y)
        {
            if (x >= 0 && x < this.m_Width 
                && y >= 0 && y < this.m_Height)
            {
                this.Visit(x, y);
                this.VisiblePoints[x, y] = true;
            }
        }

        public bool IsVisible(int x, int y)
        {
            return this.VisiblePoints[x, y];
        }

        public void Block(int x, int y)
        {
            if (x >= 0 && x < this.m_Width 
                && y >= 0 && y < this.m_Height)
            {
                this.Visit(x, y);
                this.VisiblePoints[x, y] = false;
            }
        }

        public bool IsBlocked(int x, int y)
        {
            if (x >= 0 && x < this.m_Width 
                && y >= 0 && y < this.m_Height)
            {
                return this.VisiblePoints[x, y] == false || this.IsObstacle(x, y);
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
            this.VisiblePoints = new bool[this.Width, this.Height];
            this.Visited = new bool[this.Width, this.Height];
        }

        public bool Contains(int x, int y)
        {
            return (x >= 0 && x < this.m_Width && y >= 0 && y < this.m_Height);
        }

        public IEnumerable<Vector2Int> GetVision()
        {
            HashSet<Vector2Int> visible = new HashSet<Vector2Int>();
            for (int i = 0; i < this.VisiblePoints.GetLength(0); i++)
            {
                for (int j = 0; j < this.VisiblePoints.GetLength(1); j++)
                {
                    if (this.VisiblePoints[i, j])
                    {
                        visible.Add(new Vector2Int(i, j));
                    }
                }
            }
            return visible;
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