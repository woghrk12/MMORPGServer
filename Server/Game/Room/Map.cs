using Google.Protobuf.Protocol;

namespace Server.Game
{
    public struct Pos
    {
        public Pos(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X;
        public int Y;

        public static Pos Up => new Pos(0, 1);
        public static Pos Down => new Pos(0, -1);
        public static Pos Left => new Pos(-1, 0);
        public static Pos Right => new Pos(1, 0);
        public static Pos Zero => new Pos(0, 0);

        public static Pos operator +(Pos a, Pos b) => new Pos(a.X + b.X, a.Y + b.Y);
        public static Pos operator -(Pos a, Pos b) => new Pos(a.X - b.X, a.Y - b.Y);
        public static Pos operator *(Pos a, int b) => new Pos(a.X * b, a.Y * b);
        public static Pos operator *(int a, Pos b) => new Pos(a * b.X, a * b.Y);
        public static bool operator ==(Pos a, Pos b) => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(Pos a, Pos b) => a.X != b.X || a.Y != b.Y;
    }

    public struct PQNode : IComparable<PQNode>
    {
        public int F;
        public int G;
        public int X;
        public int Y;

        public int CompareTo(PQNode other)
        {
            if (F == other.F) return 0;

            return F < other.F ? 1 : -1;
        }
    }

    public class Map
    {
        #region Variables

        private Dictionary<int, GameObject>[,] collision = null;

        private int minX = 0;
        private int maxX = 0;
        private int minY = 0;
        private int maxY = 0;

        private int width = 0;
        private int height = 0;

        #endregion Variables

        #region Methods

        public void LoadMap(int mapId, string pathPrefix = "../../../Resources/MapData")
        {
            string mapName = "Map_" + mapId.ToString("000");

            string text = File.ReadAllText($"{pathPrefix}/{mapName}.txt");
            StringReader reader = new(text);

            minX = int.Parse(reader.ReadLine());
            maxX = int.Parse(reader.ReadLine());
            minY = int.Parse(reader.ReadLine());
            maxY = int.Parse(reader.ReadLine());

            width = maxX - minX + 1;
            height = maxY - minY + 1;

            collision = new Dictionary<int, GameObject>[height, width];
            for (int y = height - 1; y >= 0; y--)
            {
                string line = reader.ReadLine();
                for (int x = 0; x < width; x++)
                {
                    collision[y, x] = new Dictionary<int, GameObject>();

                    if (line[x] == '1')
                    {
                        collision[y, x].Add(-1, null);
                    }
                }
            }
        }

        public void AddObject(GameObject gameObject)
        {
            if (ReferenceEquals(gameObject, null) == true) return;

            Vector2Int cellPos = ConvertPosToCell(gameObject.Position);
            collision[cellPos.Y, cellPos.X].Add(gameObject.ID, gameObject);
        }

        public void RemoveObject(GameObject gameObject)
        {
            if (ReferenceEquals(gameObject, null) == true) return;

            Vector2Int cellPos = ConvertPosToCell(gameObject.Position);
            collision[cellPos.Y, cellPos.X].Remove(gameObject.ID);
        }

        public bool Find(Pos position, out List<GameObject> objectList)
        {
            objectList = null;

            if (position.X < minX || position.X > maxX || position.Y < minY || position.Y > maxY) return false;

            objectList = new List<GameObject>();
            Vector2Int cellPos = ConvertPosToCell(position);

            foreach (GameObject gameObject in collision[cellPos.Y, cellPos.X].Values)
            {
                if (ReferenceEquals(gameObject, null) == true) continue;

                objectList.Add(gameObject);
            }

            return objectList.Count > 0;
        }

        public void MoveObject(GameObject gameObject, EMoveDirection moveDirection)
        {
            if (ReferenceEquals(gameObject, null) == true) return;
            if (moveDirection == EMoveDirection.None) return;

            Vector2Int curCellPos = ConvertPosToCell(gameObject.Position);
            if (collision[curCellPos.Y, curCellPos.X].ContainsKey(gameObject.ID) == false) return;

            Vector2Int targetCellPos = curCellPos;
            switch (moveDirection)
            {
                case EMoveDirection.Up:
                    targetCellPos += Vector2Int.Up;
                    break;

                case EMoveDirection.Down:
                    targetCellPos += Vector2Int.Down;
                    break;

                case EMoveDirection.Left:
                    targetCellPos += Vector2Int.Left;
                    break;

                case EMoveDirection.Right:
                    targetCellPos += Vector2Int.Right;
                    break;
            }

            if (CheckCanMove(targetCellPos) == false) return;

            collision[curCellPos.Y, curCellPos.X].Remove(gameObject.ID);
            collision[targetCellPos.Y, targetCellPos.X].Add(gameObject.ID, gameObject);

            gameObject.Position = ConvertCellToPos(targetCellPos);
        }

        private bool CheckCanMove(Vector2Int cellPos, bool isIgnoreWall = false, bool isIgnoreObject = false)
        {
            if (cellPos.X < 0 || cellPos.X >= width || cellPos.Y < 0 || cellPos.Y >= height) return false;

            foreach (KeyValuePair<int, GameObject> pair in collision[cellPos.Y, cellPos.X])
            {
                if (pair.Key == -1 && isIgnoreWall == false) return false;
                if (pair.Value.IsCollidable == true && isIgnoreObject == false) return false;
            }

            return true;
        }

        #region A* PathFinding

        private int[] dx = { 1, -1, 0, 0 };
        private int[] dy = { 0, 0, -1, 1 };
        private int[] cost = { 10, 10, 10, 10 };

        public bool FindPath(Pos startPos, Pos destPos, out List<Pos> path)
        {
            bool[,] closedArray = new bool[height, width];
            int[,] openArray = new int[height, width];
            Pos[,] parentArray = new Pos[height, width];
            PriorityQueue<PQNode> pq = new();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    closedArray[y, x] = false;
                    openArray[y, x] = Int32.MaxValue;
                    parentArray[y, x] = new Pos(-1, -1);
                }
            }

            Vector2Int startCellPos = ConvertPosToCell(startPos);
            Vector2Int destCellPos = ConvertPosToCell(destPos);

            openArray[startCellPos.Y, startCellPos.X] = Utility.CalculateDistance(startCellPos, destCellPos);

            pq.Push(new PQNode() { F = openArray[startCellPos.Y, startCellPos.X], G = 0, X = startCellPos.X, Y = startCellPos.Y });

            while (pq.Count > 0)
            {
                PQNode node = pq.Pop();

                if (node.X == destCellPos.X && node.Y == destCellPos.Y) break;
                if (closedArray[node.Y, node.X] == true) continue;

                closedArray[node.Y, node.X] = true;

                for (int i = 0; i < dx.Length; i++)
                {
                    Vector2Int nextCellPos = new Vector2Int(node.X + dx[i], node.Y + dy[i]);

                    if (nextCellPos.X == destCellPos.X && nextCellPos.Y == destCellPos.Y)
                    {
                        pq.Push(new PQNode() { F = 0, G = 0, X = nextCellPos.X, Y = nextCellPos.Y });
                        parentArray[nextCellPos.Y, nextCellPos.X] = new Pos(node.X, node.Y);
                        break;
                    }

                    if (CheckCanMove(nextCellPos, true) == false) continue;
                    if (closedArray[nextCellPos.Y, nextCellPos.X] == true) continue;

                    int g = node.G + cost[i];
                    int h = Utility.CalculateDistance(nextCellPos, destCellPos);

                    if (openArray[nextCellPos.Y, nextCellPos.X] < g + h) continue;

                    openArray[destCellPos.Y, destCellPos.X] = g + h;
                    pq.Push(new PQNode() { F = g + h, G = g, X = nextCellPos.X, Y = nextCellPos.Y });
                    parentArray[nextCellPos.Y, nextCellPos.X] = new Pos(node.X, node.Y);
                }
            }

            path = new List<Pos>();
            int tx = destCellPos.X;
            int ty = destCellPos.Y;

            while (parentArray[ty, tx].X >= 0 && parentArray[ty, tx].Y >= 0)
            {
                path.Add(ConvertCellToPos(new Vector2Int(tx, ty)));

                Pos parent = parentArray[ty, tx];
                tx = parent.X;
                ty = parent.Y;
            }

            if (tx != startCellPos.X || ty != startCellPos.Y) return false;

            path.Add(ConvertCellToPos(new Vector2Int(tx, ty)));
            path.Reverse();

            return true;
        }

        private Pos ConvertCellToPos(Vector2Int cell) => new Pos(cell.X + minX, cell.Y + minY);

        private Vector2Int ConvertPosToCell(Pos pos) => new Vector2Int(pos.X - minX, pos.Y - minY);

        #endregion A* PathFinding

        #endregion Methods
    }
}