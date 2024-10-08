namespace Server.Game
{
    public struct Vector2
    {
        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float X;
        public float Y;

        public float SqrMagnitude => X * X + Y * Y;
        public Vector2 Normalized => new Vector2(X, Y) / (float)Math.Sqrt(X * X + Y * Y);

        public static Vector2 Up => new Vector2(0f, 1f);
        public static Vector2 Down => new Vector2(0f, -1f);
        public static Vector2 Left => new Vector2(-1f, 0f);
        public static Vector2 Right => new Vector2(1f, 0f);
        public static Vector2 Zero => new Vector2(0f, 0f);

        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.X + b.X, a.Y + b.Y);
        public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.X - b.X, a.Y - b.Y);
        public static Vector2 operator *(Vector2 a, float b) => new Vector2(a.X * b, a.Y * b);
        public static Vector2 operator *(float a, Vector2 b) => new Vector2(a * b.X, a * b.Y);
        public static Vector2 operator /(Vector2 a, float b) => new Vector2(a.X / b, a.Y / b);
    }
}