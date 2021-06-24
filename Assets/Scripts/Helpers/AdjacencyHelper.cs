using System;
using Godot;
using JoyGodot.Assets.Scripts.JoyObject;

namespace JoyGodot.Assets.Scripts.Helpers
{
    public static class AdjacencyHelper
    {
        public static bool IsAdjacent(Vector2Int left, Vector2Int right)
        {
            Rect2 adjacency = new Rect2(left.x - 1, left.y - 1, 3, 3);
            return adjacency.HasPoint(right.ToVec2());
        }

        public static bool IsInRange(Vector2Int left, Vector2Int right, int range)
        {
            Rect2 rangeRect = new Rect2(left.ToVec2(), new Vector2(1, 1));
            rangeRect = rangeRect.Grow(range);
            return rangeRect.HasPoint(right.ToVec2());
        }
    }
}
