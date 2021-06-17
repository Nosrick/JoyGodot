using System;
using System.Collections.Generic;
using JoyGodot.Assets.Scripts.JoyObject;
using JoyGodot.Assets.Scripts.World;

namespace JoyGodot.Assets.Scripts.Entities.AI.LOS.Boards
{
    [Serializable]
    public class FOVShadowCasting : IFOVHandler
    {
        protected FOVArrayBoard m_Board;

        protected static readonly Vector2Int[] DIAGONALS = { new Vector2Int(1, -1), new Vector2Int(1, 1), new Vector2Int(-1, 1), new Vector2Int(-1, -1) };

        public IFOVBoard Do(IEntity viewer, IWorldInstance world, Vector2Int dimensions,
            IEnumerable<Vector2Int> walls)
        {
            Vector2Int viewerPos = viewer.WorldPosition;

            this.m_Board = new FOVArrayBoard(dimensions.x, dimensions.y, walls);
            this.m_Board.Visible(viewerPos.x, viewerPos.y);
            foreach(Vector2Int direction in DIAGONALS)
            {
                this.CastLight(viewer, world, viewerPos, viewer.VisionMod, 1, 1, 0, 0, direction.x, direction.y, 0);
                this.CastLight(viewer, world, viewerPos, viewer.VisionMod, 1, 1, 0, direction.x, 0, 0, direction.y);
            }

            return this.m_Board;
        }

        private void CastLight(IEntity viewer, IWorldInstance world, Vector2Int origin, int sightMod, int row, float start, float end, int xx, int xy, int yx, int yy)
        {
            float newStart = 0.0f;
            if(start < end)
            {
                return;
            }

            bool blocked = false;

            for(int distance = row; distance <= sightMod && blocked == false; distance++)
            {
                int deltaY = -distance;
                for(int deltaX = -distance; deltaX <= 0; deltaX++)
                {
                    int currentX = origin.x + deltaX * xx + deltaY * xy;
                    int currentY = origin.y + deltaX * yx + deltaY * yy;
                    float leftSlope = (deltaX - 0.5f) / (deltaY + 0.5f);
                    float rightSlope = (deltaX + 0.5f) / (deltaY - 0.5f);

                    if (!(currentX >= 0 && currentY >= 0 && currentX < this.m_Board.Width && currentY < this.m_Board.Height) || start < rightSlope)
                    {
                        continue;
                    }

                    if (end > leftSlope)
                    {
                        break;
                    }

                    if (Math.Sqrt(deltaX * deltaX + deltaY * deltaY) <= sightMod)
                    {
                        this.m_Board.Visible(currentX, currentY);
                    }

                    if (blocked)
                    {
                        this.m_Board.Visible(currentX, currentY);
                        if(this.m_Board.IsObstacle(currentX, currentY))
                        {
                            newStart = rightSlope;
                            //m_Board.Block(currentX, currentY);
                        }
                        else
                        {
                            blocked = false;
                            start = newStart;
                        }
                    }
                    else
                    {
                        this.m_Board.Visible(currentX, currentY);
                        if (!this.m_Board.IsObstacle(currentX, currentY) || distance >= sightMod)
                        {
                            continue;
                        }
                        
                        blocked = true;
                        this.CastLight(viewer, world, origin, sightMod, distance + 1, start, leftSlope, xx, xy, yx, yy);
                        newStart = rightSlope;
                    }
                }
            }
        }

        public LinkedList<Vector2Int> HasLOS(Vector2Int origin, Vector2Int target)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Vector2Int> Vision => this.m_Board.GetVision();
    }
}
