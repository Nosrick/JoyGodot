namespace JoyGodot.Assets.Scripts.JoyObject
{
    public struct Rect2Int
    {
        public int x;
        public int y;

        public int width;
        public int height;

        public int xMax => this.x + this.width;
        public int yMax => this.y + this.height;

        public int Area => this.width * this.height;

        public Rect2Int(Vector2Int position, Vector2Int sizes)
            : this(position.x, position.y,
                sizes.x, sizes.y)
        {
        }

        public Rect2Int(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            
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