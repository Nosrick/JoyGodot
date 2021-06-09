using System;
using Godot;
using Godot.Collections;
using JoyLib.Code.Helpers;

namespace JoyLib.Code
{
    public struct Vector2Int : ISerialisationHandler
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

        public static Vector2Int Zero => new Vector2Int(0, 0);

        public Vector2 ToVec2 => new Vector2(this.x, this.y);

        public Dictionary Save()
        {
            Dictionary saveDict = new Dictionary
            {
                {"x", this.x}, 
                {"y", this.y}
            };

            return saveDict;
        }

        public void Load(string data)
        {
            JSONValueExtractor valueExtractor = GlobalConstants.GameManager.SettingsManager.ValueExtractor;

            JSONParseResult result = JSON.Parse(data);

            if (result.Error != Error.Ok)
            {
                GlobalConstants.ActionLog.Log("Could not parse JSON for Vector2Int", LogLevel.Warning);
                return;
            }

            if (!(result.Result is Dictionary dictionary))
            {
                GlobalConstants.ActionLog.Log("Could not parse JSON to Dictionary for Vector2Int", LogLevel.Warning);
                return;
            }

            this.x = valueExtractor.GetValueFromDictionary<int>(dictionary, "x");
            this.y = valueExtractor.GetValueFromDictionary<int>(dictionary, "y");
        }
    }
}