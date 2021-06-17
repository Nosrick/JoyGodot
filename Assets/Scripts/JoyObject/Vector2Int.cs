using System;
using Godot;
using Godot.Collections;
using JoyGodot.Assets.Scripts.Base_Interfaces;
using JoyGodot.Assets.Scripts.Helpers;

namespace JoyGodot.Assets.Scripts.JoyObject
{
    public struct Vector2Int : ISerialisationHandler
    {
        public int x;
        public int y;

        /*
        public Vector2Int()
        {
            this.x = 0;
            this.y = 0;
        }
        */
        
        public Vector2Int(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2Int(float x, float y, bool roundDown = true)
        {
            if (roundDown)
            {
                this.x = (int) Math.Floor(x);
                this.y = (int) Math.Floor(y);
            }
            else
            {
                this.x = (int) Math.Round(x);
                this.y = (int) Math.Round(y);
            }
        }

        public Vector2Int(Vector2 vector2, bool roundDown = true)
            : this(vector2.x, vector2.y, roundDown)
        { }

        public Vector2Int(Dictionary data)
        {
            JSONValueExtractor valueExtractor = GlobalConstants.GameManager.SettingsManager.ValueExtractor;

            this.x = valueExtractor.GetValueFromDictionary<int>(data, "x");
            this.y = valueExtractor.GetValueFromDictionary<int>(data, "y");
        }

        public static bool operator ==(Vector2Int left, Vector2Int right)
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

        public override string ToString()
        {
            return "{ " + this.x + " : " + this.y + " }";
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector2Int other)
            {
                return this.x == other.x && this.y == other.y;
            }

            return false;
        }

        public bool Equals(Vector2Int other)
        {
            return this.x == other.x && this.y == other.y;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (this.x * 397) ^ this.y;
            }
        }

        public static Vector2Int Zero => new Vector2Int(0, 0);

        public Vector2 ToVec2()
        {
            return new Vector2(this.x, this.y);
        }

        public Dictionary Save()
        {
            Dictionary saveDict = new Dictionary
            {
                {"x", this.x}, 
                {"y", this.y}
            };

            return saveDict;
        }

        public void Load(Dictionary data)
        {
            GD.PushError("Using the Load of Vector2Int! DOES NOT FUNCTION!");
            JSONValueExtractor valueExtractor = GlobalConstants.GameManager.SettingsManager.ValueExtractor;

            this.x = valueExtractor.GetValueFromDictionary<int>(data, "x");
            this.y = valueExtractor.GetValueFromDictionary<int>(data, "y");
        }
    }
}