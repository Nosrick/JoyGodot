using System;
using System.Collections.Generic;
using Godot;
using JoyLib.Code.Rollers;

namespace JoyLib.Code.World.Generators.Interiors
{
    public delegate GeneratorTileType[,] PlaceRooms();

    public class DungeonRoomGenerator
    {
        protected const int LOOP_BREAK = 50;

        protected const int MIN_ROOM_SIZE = 5;
        protected const int MAX_ROOM_SIZE = 11;
        protected const int MIN_MAP_SIZE = 19;

        protected readonly int m_Size;
        protected readonly GeneratorTileType[,] m_Tiles;

        protected readonly int m_NumberOfRooms;
        protected int m_NumberRoomsPlaced;

        protected readonly List<Rect2Int> m_Rooms;
        
        protected RNG Roller { get; set; }

        public DungeonRoomGenerator(int size, RNG roller)
        {
            this.m_Size = size;
            this.Roller = roller;

            if (this.m_Size < MIN_MAP_SIZE)
            {
                this.m_Size = MIN_MAP_SIZE;
            }

            this.m_Tiles = new GeneratorTileType[this.m_Size, this.m_Size];
            this.m_NumberOfRooms = this.CalculateRooms();
            this.m_NumberRoomsPlaced = 0;

            this.m_Rooms = new List<Rect2Int>(this.m_NumberOfRooms);
        }

        public GeneratorTileType[,] GenerateRooms()
        {
            int loopCounter = 0;

            while (this.m_NumberRoomsPlaced <= this.m_NumberOfRooms && loopCounter < LOOP_BREAK)
            {
                Vector2Int topLeft = new Vector2Int();
                topLeft.x = this.Roller.Roll(1, this.m_Size);
                if (topLeft.x % 2 == 1)
                {
                    topLeft.x += 1;
                }

                topLeft.y = this.Roller.Roll(1, this.m_Size);
                if (topLeft.y % 2 == 1)
                {
                    topLeft.y += 1;
                }

                if (this.PlaceRoom(topLeft)) this.OpenRoom(this.m_Rooms[this.m_Rooms.Count - 1]);

                loopCounter += 1;
            }

            return this.m_Tiles;
        }

        private int CalculateRooms()
        {
            int dungeonArea = this.m_Size * this.m_Size;
            const int roomArea = MAX_ROOM_SIZE * MAX_ROOM_SIZE;

            return (dungeonArea / roomArea);
        }

        private bool PlaceRoom(Vector2Int topLeft)
        {
            int loopCounter = 0;
            while (loopCounter < LOOP_BREAK)
            {
                loopCounter += 1;

                if (topLeft.x < 1 || topLeft.y < 1 || topLeft.x > this.m_Size - 1 || topLeft.y > this.m_Size - 1)
                    return false;

                if (this.m_Tiles[topLeft.x, topLeft.y] == GeneratorTileType.Floor)
                    return false;

                if (this.m_NumberOfRooms == this.m_NumberRoomsPlaced)
                    return false;

                Vector2Int sizes = new Vector2Int(this.Roller.Roll(MIN_ROOM_SIZE, MAX_ROOM_SIZE), this.Roller.Roll(MIN_ROOM_SIZE, MAX_ROOM_SIZE));
                
                Rect2Int room = new Rect2Int(topLeft, sizes);

                if (!this.ValidateRoom(room))
                    return false;

                if (this.CheckForRoom(room))
                    return false;

                for (int i = room.x; i <= room.xMax; i++)
                {
                    for (int j = room.y; j <= room.yMax; j++)
                    {
                        if (i == room.x || i == room.xMax || j == room.y || j == room.yMax)
                        {
                            this.m_Tiles[i, j] = GeneratorTileType.Perimeter;
                        }
                        else
                        {
                            this.m_Tiles[i, j] = GeneratorTileType.Floor;
                        }
                    }
                }

                this.m_Rooms.Add(room);
                this.m_NumberRoomsPlaced += 1;
                return true;
            }
            return false;
        }

        private void OpenRoom(Rect2Int room)
        {
            int doors = this.CalculateDoors(room);

            List<Vector2Int> validDoors = new List<Vector2Int>();
            for (int i = room.x; i < room.xMax + 1; i++)
            {
                for (int j = room.y; j < room.yMax + 1; j++)
                {
                    if (this.m_Tiles[i, j] != GeneratorTileType.Perimeter)
                        continue;

                    if (((i != room.x && i != room.xMax) || (j != room.y && j != room.yMax)) &&
                        i > 1 && i < this.m_Size - 1 && j > 1 && j < this.m_Size - 1)
                    {
                        validDoors.Add(new Vector2Int(i, j));
                    }
                }
            }

            for (int i = 0; i < doors; i++)
            {
                int index = this.Roller.Roll(0, validDoors.Count);

                Vector2Int point = new Vector2Int(validDoors[index].x, validDoors[index].y);

                if (this.m_Tiles[point.x - 1, point.y] == GeneratorTileType.Entrance)
                    continue;

                if (this.m_Tiles[point.x + 1, point.y] == GeneratorTileType.Entrance)
                    continue;

                if (this.m_Tiles[point.x, point.y - 1] == GeneratorTileType.Entrance)
                    continue;

                if (this.m_Tiles[point.x, point.y + 1] == GeneratorTileType.Entrance)
                    continue;

                if (this.m_Tiles[point.x, point.y] == GeneratorTileType.Perimeter)
                {
                    this.m_Tiles[point.x, point.y] = GeneratorTileType.Entrance;
                }
                else
                {
                    i -= 1;
                }
            }
        }

        private int CalculateDoors(Rect2Int room)
        {
            int maxDoors = (int)Math.Sqrt(room.Area);

            return this.Roller.Roll(1, maxDoors);
        }

        private bool CheckForRoom(Rect2Int room)
        {
            for (int i = room.x; i <= room.xMax; i++)
            {
                for (int j = room.y; j <= room.yMax; j++)
                {
                    if (this.m_Tiles[i, j] != GeneratorTileType.None)
                        return true;
                }
            }

            return false;
        }

        private bool ValidateRoom(Rect2Int room)
        {
            if (room.xMax >= this.m_Size)
                return false;

            if (room.x < 1)
                return false;

            if (room.yMax >= this.m_Size)
                return false;

            if (room.y < 1)
                return false;

            if (room.width < MIN_ROOM_SIZE || room.width > MAX_ROOM_SIZE)
            {
                return false;
            }

            if (room.height < MIN_ROOM_SIZE || room.height > MAX_ROOM_SIZE)
            {
                return false;
            }

            return true;
        }

        public List<Rect2Int> rooms
        {
            get
            {
                return this.m_Rooms;
            }
        }

        public Vector2Int PlaceEndPoint()
        {
            List<Vector2Int> validPoints = new List<Vector2Int>();
            for (int i = 0; i < this.m_Rooms.Count; i++)
            {
                for (int j = (int) this.m_Rooms[i].x; j < (int) this.m_Rooms[i].xMax + 1; j++)
                {
                    for (int k = (int) this.m_Rooms[i].y; k < (int) this.m_Rooms[i].yMax + 1; k++)
                    {
                        if (this.m_Tiles[j, k] == GeneratorTileType.Floor)
                        {
                            validPoints.Add(new Vector2Int(j, k));
                        }
                    }
                }
            }

            if (validPoints.Count == 0)
                return new Vector2Int(this.m_Tiles.GetLength(0) / 2, this.m_Tiles.GetLength(1) / 2);

            int index = this.Roller.Roll(0, validPoints.Count - 1);
            return validPoints[index];
        }
    }
}
