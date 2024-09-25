namespace Server.Game
{
    public class Utility
    {
        public static int CalculateDistance(Pos a, Pos b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        public static int CalculateDistance(Vector2Int a, Vector2Int b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }
    }
}