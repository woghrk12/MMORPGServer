using Google.Protobuf.Protocol;

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

        public static Pos GetFrontPos(Pos curPos, EMoveDirection facingDirection, int distance = 1)
        {
            Pos frontPos = new Pos(curPos.X, curPos.Y);

            switch (facingDirection)
            {
                case EMoveDirection.Up:
                    frontPos += Pos.Up * distance;
                    break;

                case EMoveDirection.Down:
                    frontPos += Pos.Down * distance;
                    break;

                case EMoveDirection.Left:
                    frontPos += Pos.Left * distance;
                    break;

                case EMoveDirection.Right:
                    frontPos += Pos.Right * distance;
                    break;
            }

            return frontPos;
        }

        public static EMoveDirection GetDirection(Pos from, Pos to)
        {
            Vector2Int vector = new Vector2Int(to.X - from.X, to.Y - from.Y);

            if (vector == Vector2Int.Zero) return EMoveDirection.None;

            if (vector.X != 0 && vector.Y == 0)
            {
                return vector.X > 0 ? EMoveDirection.Right : EMoveDirection.Left;
            }
            else if (vector.X == 0 && vector.Y != 0)
            {
                return vector.Y > 0 ? EMoveDirection.Up : EMoveDirection.Down;
            }

            return EMoveDirection.None;
        }
    }
}