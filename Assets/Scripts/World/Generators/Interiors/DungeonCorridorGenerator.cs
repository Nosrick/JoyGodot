using System.Collections.Generic;
using JoyLib.Code.Rollers;

namespace JoyLib.Code.World.Generators.Interiors
{
    public class DungeonCorridorGenerator
    {
        protected const int LOOP_BREAK = 50;

        protected GeneratorTileType[,] m_Tiles;
        protected List<Rect2Int> m_Rooms;
        protected readonly int m_CorridorBend;
        
        protected RNG Roller { get; set; }

        protected const int PASSES = 3;

        public DungeonCorridorGenerator(GeneratorTileType[,] tilesRef, List<Rect2Int> roomsRef, int bendRef, RNG roller)
        {
            this.Roller = roller;
            this.m_Tiles = tilesRef;
            this.m_Rooms = new List<Rect2Int>();

            this.m_Rooms = roomsRef;

            this.m_CorridorBend = bendRef;
        }

        public GeneratorTileType[,] GenerateCorridors()
        {
            for (int a = 0; a < PASSES; a++)
            {
                for (int i = 0; i < this.m_Rooms.Count; i++)
                {
                    for (int j = this.m_Rooms[i].x; j < this.m_Rooms[i].xMax; j++)
                    {
                        for (int k = this.m_Rooms[i].y; k < this.m_Rooms[i].yMax; k++)
                        {
                            if (this.HasFlag(this.m_Tiles[j, k], GeneratorTileType.Wall) || this.HasFlag(this.m_Tiles[j, k], GeneratorTileType.Perimeter) || this.HasFlag(this.m_Tiles[j, k], GeneratorTileType.Entrance))
                                continue;

                            this.Tunnel(new Vector2Int(j, k), (FacingDirection)(this.Roller.Roll(0, 4) * 2));
                        }
                    }
                }
            }

            this.ClipDeadEnds();

            return this.m_Tiles;
        }

        private void Tunnel(Vector2Int point, FacingDirection lastDirection)
        {
            while (true)
            {
                Vector2Int newPoint = point;
                FacingDirection newDirection = this.ChooseTunnelDirection(lastDirection);

                for (int i = 0; i < 2; i++)
                {
                    switch (newDirection)
                    {
                        case FacingDirection.North:
                            newPoint.y -= 1;
                            break;

                        case FacingDirection.East:
                            newPoint.x += 1;
                            break;

                        case FacingDirection.South:
                            newPoint.y += 1;
                            break;

                        case FacingDirection.West:
                            newPoint.x -= 1;
                            break;
                    }

                    if (!this.ValidateTunnel(newPoint))
                        return;

                    this.m_Tiles[newPoint.x, newPoint.y] = GeneratorTileType.Corridor;
                }

                point = newPoint;
                lastDirection = newDirection;
            }
        }

        public void ClipDeadEnds()
        {
            while (true)
            {
                List<Vector2Int> deadEnds = new List<Vector2Int>();

                for (int i = 1; i < this.m_Tiles.GetLength(0) - 1; i++)
                {
                    for (int j = 1; j < this.m_Tiles.GetLength(1) - 1; j++)
                    {
                        if (this.m_Tiles[i, j] != GeneratorTileType.Corridor)
                            continue;

                        int neighbours = 0;
                        //North
                        if (this.m_Tiles[i, j - 1] == GeneratorTileType.Corridor)
                        {
                            neighbours += 1;
                        }
                        //East
                        if (this.m_Tiles[i + 1, j] == GeneratorTileType.Corridor)
                        {
                            neighbours += 1;
                        }
                        //South
                        if (this.m_Tiles[i, j + 1] == GeneratorTileType.Corridor)
                        {
                            neighbours += 1;
                        }
                        //West
                        if (this.m_Tiles[i - 1, j] == GeneratorTileType.Corridor)
                        {
                            neighbours += 1;
                        }

                        if (neighbours <= 1)
                            deadEnds.Add(new Vector2Int(i, j));
                    }
                }

                for (int i = 0; i < deadEnds.Count; i++)
                {
                    this.m_Tiles[deadEnds[i].x, deadEnds[i].y] = GeneratorTileType.Wall;
                }

                if (deadEnds.Count == 0)
                    return;
            }
        }

        private bool ValidateTunnel(Vector2Int point)
        {
            if (point.x < 1 || point.x > this.m_Tiles.GetLength(0) - 2)
                return false;

            if (point.y < 1 || point.y > this.m_Tiles.GetLength(1) - 2)
                return false;

            if (this.HasFlag(this.m_Tiles[point.x, point.y], GeneratorTileType.Corridor))
                return false;

            return true;
        }

        private FacingDirection ChooseTunnelDirection(FacingDirection lastDirection)
        {
            int roll = this.Roller.Roll(0, 100);

            if (roll >= this.m_CorridorBend)
                return lastDirection;

            roll = this.Roller.Roll(0, 4) * 2;
            while (roll == (int)lastDirection)
            {
                roll = this.Roller.Roll(0, 4) * 2;
            }
            return (FacingDirection)roll;
        }

        private bool HasFlag(GeneratorTileType left, GeneratorTileType right)
        {
            return ((left & right) == right);
        }
    }
}