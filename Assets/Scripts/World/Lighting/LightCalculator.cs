using System;
using System.Collections.Generic;
using JoyLib.Code.Entities.AI.LOS;
using JoyLib.Code.Entities.Items;

namespace JoyLib.Code.World.Lighting
{
    public class LightCalculator
    {
        protected LightBoard m_Board;

        protected static readonly Vector2Int[] DIAGONALS = { new Vector2Int(1, -1), new Vector2Int(1, 1), new Vector2Int(-1, 1), new Vector2Int(-1, -1) };

        public LightBoard Do(IEnumerable<IItemInstance> items, IWorldInstance world, Vector2Int dimensions,
            IEnumerable<Vector2Int> walls)
        {
            this.m_Board = new LightBoard(dimensions.x, dimensions.y, walls);
            foreach (IItemInstance item in items)
            {
                this.m_Board.ClearVisited();
                this.Light.DiffuseLight(item.WorldPosition, item.ItemType.LightLevel);
                this.DoAdjacent(item.WorldPosition);
            }

            return this.m_Board;
        }

        protected void CastLight(IItemInstance item, IWorldInstance world, Vector2Int origin, int row, float start, float end, int xx, int xy, int yx, int yy)
        {
            float newStart = 0.0f;
            if(start < end)
            {
                return;
            }

            bool blocked = false;

            for(int distance = row; distance <= item.ItemType.LightLevel && blocked == false; distance++)
            {
                int deltaY = -distance;
                for(int deltaX = -distance; deltaX <= 0; deltaX++)
                {
                    int currentX = origin.x + deltaX * xx + deltaY * xy;
                    int currentY = origin.y + deltaX * yx + deltaY * yy;
                    float leftSlope = (deltaX - 0.5f) / (deltaY + 0.5f);
                    float rightSlope = (deltaX + 0.5f) / (deltaY - 0.5f);

                    int lightLevel = item.ItemType.LightLevel - distance;

                    if (!this.Contains(new Vector2Int(currentX, currentY)) || start < rightSlope)
                    {
                        continue;
                    }

                    if (end > leftSlope)
                    {
                        break;
                    }

                    Vector2Int currentPosition = new Vector2Int(currentX, currentY);
                    if (Math.Sqrt(deltaX * deltaX + deltaY * deltaY) <= item.ItemType.LightLevel)
                    {
                        this.m_Board.AddLight(currentPosition, lightLevel);
                    }

                    if (blocked)
                    {
                        this.m_Board.DiffuseLight(currentPosition, lightLevel);
                        if(this.m_Board.IsObstacle(currentPosition))
                        {
                            newStart = rightSlope;
                        }
                        else
                        {
                            blocked = false;
                            start = newStart;
                        }
                    }
                    else
                    {
                        this.m_Board.DiffuseLight(currentPosition, lightLevel);
                        if (!this.m_Board.IsObstacle(currentPosition) || distance >= item.ItemType.LightLevel)
                        {
                            continue;
                        }
                        
                        blocked = true;
                        this.CastLight(item, world, origin, distance + 1, start, leftSlope, xx, xy, yx, yy);
                        newStart = rightSlope;
                    }
                }
            }
        }

        protected void DoDiffuse(Vector2Int point)
        {
            this.m_Board.ClearVisited();
            this.DoAdjacent(point);
        }

        protected void DoAdjacent(Vector2Int position)
        {
            int lightLevel = this.m_Board.GetLight(position);
            if (lightLevel <= 1)
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
            foreach (Vector2Int point in adjacent)
            { 
                if (!this.Contains(point))
                {
                    continue;
                }
                int neighbourLight = this.m_Board.GetLight(point);
                if (neighbourLight < lightLevel - 1)
                {
                    this.m_Board.DiffuseLight(point, lightLevel - 1);
                    this.DoAdjacent(point);
                }
            }
        }

        protected bool Contains(Vector2Int point)
        {
            return point.x >= 0
                   && point.x < this.m_Board.Width
                   && point.y >= 0
                   && point.y < this.m_Board.Height;
        }

        public LightBoard Light => this.m_Board;
    }
}