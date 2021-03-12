using System.Collections.Generic;
using Godot;

namespace JoyLib.Code.Entities.AI
{
    public interface IPathfinder
    {
        void FindPathStop();

        Queue<Vector2Int> FindPath(Vector2Int fromPoint, Vector2Int toPoint, byte[,] grid, Rect2 sizes);

        string DetermineSector(Vector2Int fromPoint, Vector2Int toPoint);

        bool Stopped
        {
            get;
        }

        bool Diagonals
        {
            get;
            set;
        }

        bool HeavyDiagonals
        {
            get;
            set;
        }

        int HeuristicEstimate
        {
            get;
            set;
        }

        bool PunishChangeDirection
        {
            get;
            set;
        }

        bool ReopenCloseNodes
        {
            get;
            set;
        }

        bool TieBreaker
        {
            get;
            set;
        }

        int SearchLimit
        {
            get;
            set;
        }

        double CompletedTime
        {
            get;
            set;
        }

        bool DebugProgress
        {
            get;
            set;
        }

        bool DebugFoundPath
        {
            get;
            set;
        }
    }
}
