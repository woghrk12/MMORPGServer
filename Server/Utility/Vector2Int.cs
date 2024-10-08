namespace Server.Game
{
    public struct Vector2Int
    {
        public Vector2Int(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X;
        public int Y;

        public int SqrMagnitude => X * X + Y * Y;

        public static Vector2Int Up => new Vector2Int(0, 1);
        public static Vector2Int Down => new Vector2Int(0, -1);
        public static Vector2Int Left => new Vector2Int(-1, 0);
        public static Vector2Int Right => new Vector2Int(1, 0);
        public static Vector2Int Zero => new Vector2Int(0, 0);

        public static Vector2Int operator +(Vector2Int a, Vector2Int b) => new Vector2Int(a.X + b.X, a.Y + b.Y);
        public static Vector2Int operator -(Vector2Int a, Vector2Int b) => new Vector2Int(a.X - b.X, a.Y - b.Y);
        public static Vector2Int operator *(Vector2Int a, int b) => new Vector2Int(a.X * b, a.Y * b);
        public static Vector2Int operator *(int a, Vector2Int b) => new Vector2Int(a * b.X, a * b.Y);
        public static bool operator ==(Vector2Int a, Vector2Int b) => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(Vector2Int a, Vector2Int b) => a.X != b.X || a.Y == b.Y;
    }
}