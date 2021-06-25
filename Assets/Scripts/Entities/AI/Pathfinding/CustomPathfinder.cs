using System;
using System.Collections.Generic;
using System.Linq;
using JoyGodot.Assets.Scripts.Collections;
using JoyGodot.Assets.Scripts.JoyObject;

namespace JoyGodot.Assets.Scripts.Entities.AI.Pathfinding
{
    public struct PathFinderNode
    {
        public int F;
        public int G;
        public int H;
        public int X;
        public int Y;
        //Parent
        public int PX;
        public int PY;
    }

    public class CustomPathfinder : IPathfinder
    {
        public bool Stopped { get; set; }
        public bool Diagonals { get; set; }
        public bool HeavyDiagonals { get; set; }
        public int HeuristicEstimate { get; set; }
        public bool PunishChangeDirection { get; set; }
        public bool ReopenCloseNodes { get; set; }
        public bool TieBreaker { get; set; }
        public int SearchLimit { get; set; }
        
        public double CompletedTime { get; set; }
        
        public bool DebugProgress { get; set; }
        
        public bool DebugFoundPath { get; set; }
        protected int PunishmentValue { get; set; }

        public CustomPathfinder()
        {
            this.Diagonals = true;
            this.HeavyDiagonals = false;
            this.HeuristicEstimate = 2;
            this.PunishChangeDirection = false;
            this.ReopenCloseNodes = false;
            this.TieBreaker = false;
            this.SearchLimit = 100;
        }

        public Queue<Vector2Int> FindPath(Vector2Int fromPoint, Vector2Int toPoint, byte[,] grid, Rect2Int sizes)
        {
            Queue<Vector2Int> path = new Queue<Vector2Int>();

            sbyte[,] direction;
            if (this.Diagonals)
            {
                direction = new sbyte[8, 2] { { 0, -1 }, { 1, 0 }, { 0, 1 }, { -1, 0 }, { 1, -1 }, { 1, 1 }, { -1, 1 }, { -1, -1 } };
            }
            else
            {
                direction = new sbyte[4, 2] { { 0, -1 }, { 1, 0 }, { 0, 1 }, { -1, 0 } };
            }

            PathFinderNode parentNode = new PathFinderNode
            {
                G = 0, 
                H = this.HeuristicEstimate
            };
            parentNode.F = parentNode.G + parentNode.H;
            parentNode.X = fromPoint.x;
            parentNode.Y = fromPoint.y;
            parentNode.PX = parentNode.X;
            parentNode.PY = parentNode.Y;

            PriorityQueue<PathFinderNode> openList = new PriorityQueue<PathFinderNode>(new PathfinderNodeComparer());
            List<PathFinderNode> closedList = new List<PathFinderNode>();

            openList.Push(parentNode);

            bool found = false;

            int loopBreak = 0;
            while(openList.Count > 0 && loopBreak < this.SearchLimit)
            {
                parentNode = openList.Pop();

                if(parentNode.X == toPoint.x && parentNode.Y == toPoint.y)
                {
                    closedList.Add(parentNode);
                    found = true;
                    break;
                }

                if(closedList.Count > this.SearchLimit)
                {
                    return null;
                }

                if(this.PunishChangeDirection)
                {
                    this.PunishmentValue = parentNode.X - parentNode.PX;
                }

                for(int i = 0; i < (this.Diagonals == true ? 8 : 4); i++)
                {
                    PathFinderNode newNode = new PathFinderNode
                    {
                        X = parentNode.X + direction[i, 0], 
                        Y = parentNode.Y + direction[i, 1]
                    };

                    if(newNode.X < 0 || newNode.Y < 0 || newNode.X >= sizes.xMax || newNode.Y >= sizes.yMax)
                    {
                        continue;
                    }

                    //If it's impassible, skip
                    if(grid[newNode.X, newNode.Y] == byte.MaxValue)
                    {
                        continue;
                    }

                    int newG = parentNode.G + grid[newNode.X, newNode.Y];
                    if(this.HeavyDiagonals && i > 3)
                    {
                        newG += 2;
                    }

                    if(this.PunishChangeDirection)
                    {
                        if ((newNode.X - parentNode.X) != 0)
                        {
                            if (this.PunishmentValue == 0)
                            {
                                newG += this.PunishmentValue;
                            }
                        }
                        if ((newNode.Y - parentNode.Y) != 0)
                        {
                            if (this.PunishmentValue != 0)
                            {
                                newG += this.PunishmentValue;
                            }
                        }
                    }

                    int foundInOpenIndex = -1;
                    for (int j = 0; j < openList.Count; j++)
                    {
                        if (openList[j].X == newNode.X && openList[j].Y == newNode.Y)
                        {
                            foundInOpenIndex = j;
                            break;
                        }
                    }
                    if (foundInOpenIndex != -1 && openList[foundInOpenIndex].G <= newG)
                    {
                        continue;
                    }

                    int foundInCloseIndex = -1;
                    for (int j = 0; j < closedList.Count; j++)
                    {
                        if (closedList[j].X == newNode.X && closedList[j].Y == newNode.Y)
                        {
                            foundInCloseIndex = j;
                            break;
                        }
                    }
                    if (foundInCloseIndex != -1 && (this.ReopenCloseNodes || closedList[foundInCloseIndex].G <= newG))
                    {
                        continue;
                    }

                    newNode.PX = parentNode.X;
                    newNode.PY = parentNode.Y;
                    newNode.G = newG;

                    int hDiagonal = Math.Min(Math.Abs(newNode.X - toPoint.x), Math.Abs(newNode.Y - toPoint.y));
                    int hStraight = Math.Abs(newNode.X - toPoint.x) + Math.Abs(newNode.Y - toPoint.y);
                    newNode.H = (this.HeuristicEstimate * 2) * hDiagonal + this.HeuristicEstimate * (hStraight - 2 * hDiagonal);

                    newNode.F = newNode.G + newNode.H;

                    openList.Push(newNode);
                }

                closedList.Add(parentNode);

                loopBreak++;
            }

            if(found)
            {
                PathFinderNode node = closedList[closedList.Count - 1];

                for(int i = closedList.Count - 1; i >= 0; i--)
                {
                    if (node.PX == closedList[i].X && node.PY == closedList[i].Y || i == closedList.Count - 1)
                    {
                        path.Enqueue(new Vector2Int(node.X, node.Y));
                        node = closedList[i];
                    }
                    else
                    {
                        closedList.RemoveAt(i);
                    }
                }
                //There's always a copy of the last node, so get rid of it
                path.Dequeue();
            }

            this.Stopped = true;
            Queue<Vector2Int> returnPath = new Queue<Vector2Int>(path.Reverse());
            return returnPath;
        }

        public string DetermineSector(Vector2Int from, Vector2Int to)
        {
            float xDiff = to.x - from.x;
            float yDiff = to.y - from.y;
            double angle = Math.Atan2(yDiff, xDiff) * (180 / Math.PI);
            angle += 90;

            if (angle < 0)
            {
                angle += 360;
            }

            if ((angle >= 0 && angle <= 22.5) || (angle <= 360 && angle >= 337.5))
            {
                return "north";
            }
            else if (angle <= 67.5)
            {
                return "north east";
            }
            else if (angle <= 112.5)
            {
                return "east";
            }
            else if (angle <= 157.5)
            {
                return "south east";
            }
            else if (angle <= 202.5)
            {
                return "south";
            }
            else if (angle <= 247.5)
            {
                return "south west";
            }
            else if (angle <= 292.5)
            {
                return "west";
            }
            else if (angle <= 337.5)
            {
                return "north west";
            }

            return "nearby";
        }

        protected internal class PathfinderNodeComparer : IComparer<PathFinderNode>
        {
            public int Compare(PathFinderNode left, PathFinderNode right)
            {
                if (left.F > right.F)
                {
                    return 1;
                }
                if (left.F < right.F)
                {
                    return -1;
                }
                return 0;
            }
        }
    }
}
