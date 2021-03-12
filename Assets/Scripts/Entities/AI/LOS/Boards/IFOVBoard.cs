using System.Collections.Generic;

namespace JoyLib.Code.Entities.AI.LOS
{
    public interface IFOVBoard
    {
        IEnumerable<Vector2Int> GetVision();

        bool Contains(int x, int y);

        void ClearBoard();

        bool HasVisited(int x, int y);

        void Visit(int x, int y);

        void Visible(int x, int y);

        void Block(int x, int y);

        bool IsBlocked(int x, int y);

        bool IsObstacle(int x, int y);
        bool IsVisible(int x, int y);

        double Radius(int deltaX, int deltaY);
    }
}
