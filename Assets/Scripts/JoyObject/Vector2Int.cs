using System;
using Godot;

namespace JoyLib.Code
{
    public struct Vector2Int
    {
        public int x;
        public int y;

        public Vector2Int(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2Int(float x, float y, bool roundDown = true)
        {
            if (roundDown)
            {
                this.x = (int)Math.Floor(x);
                this.y = (int)Math.Floor(y);
            }
            else
            {
                this.x = (int) Math.Round(x);
                this.y = (int) Math.Round(y);
            }
        }

        public Vector2Int(Vector2 vector2, bool roundDown = true)
        : this(vector2.x, vector2.y, roundDown)
        {
        }
        
        public static bool operator == (Vector2Int left, Vector2Int right)
        {
            return left.x == right.x && left.y == right.y;
        }

        public static bool operator !=(Vector2Int left, Vector2Int right)
        {
            return left.x != right.x || left.y != right.y;
        }

        public static Vector2Int operator *(Vector2Int left, int right)
        {
            return new Vector2Int(left.x * right, left.y * right);
        }

        public static Vector2Int Zero => new Vector2Int(0, 0);

        public Vector2 ToVec2 => new Vector2(this.x, this.y);
    }
}