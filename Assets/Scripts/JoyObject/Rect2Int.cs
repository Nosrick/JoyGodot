namespace JoyLib.Code
{
    public struct Rect2Int
    {
        public int x;
        public int y;

        public int width;
        public int height;

        public int xMax => this.x + this.width;
        public int yMax => this.y + this.height;

        public int Area => this.width * height;

        public Rect2Int(Vector2Int position, Vector2Int sizes)
        {
            this.x = position.x;
            this.y = position.y;
            this.width = sizes.x;
            this.height = sizes.y;
        }

        public bool Contains(Vector2Int point)
        {
            if (point.x < this.x || point.x > this.x)
            {
                return false;
            }
            if (point.y < this.y || point.y > this.y)
            {
                return false;
            }

            return true;
        }
    }
}