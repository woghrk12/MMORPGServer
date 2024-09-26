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

        public bool CheckCanMove(Pos position, bool isIgnoreObject = false)
        {
            if (position.X < minX || position.X > maxX) return false;
            if (position.Y < minY || position.Y > maxY) return false;

            Vector2Int cellPos = ConvertPosToCell(position);

            if (collision[cellPos.Y, cellPos.X].ContainsKey(-1) == true) return false;
            if (isIgnoreObject == false && collision[cellPos.Y, cellPos.X].Count > 0) return false;

            return true;
        }

        public bool CheckCanMove(Vector2Int cellPos, bool isIgnoreObject = false)
        {
            if (cellPos.X < 0 || cellPos.X >= width || cellPos.Y < 0 || cellPos.Y >= height) return false;

            if (collision[cellPos.Y, cellPos.X].ContainsKey(-1) == true) return false;
            if (isIgnoreObject == false && collision[cellPos.Y, cellPos.X].Count > 0) return false;

            return true;
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

        #region A* PathFinding

        private int[] dx = { 1, -1, 0, 0 };
        private int[] dy = { 0, 0, -1, 1 };
        private int[] cost = { 10, 10, 10, 10 };

        public bool FindPath(Vector2Int startCellPos, Vector2Int destCellPos, out List<Vector2Int> path)
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

            Pos start = ConvertCellToPos(startCellPos);
            Pos dest = ConvertCellToPos(destCellPos);

            openArray[start.Y, start.X] = Utility.CalculateDistance(start, dest);

            pq.Push(new PQNode() { F = openArray[start.Y, start.X], G = 0, X = start.X, Y = start.Y });

            while (pq.Count > 0)
            {
                PQNode node = pq.Pop();

                if (node.X == dest.X && node.Y == dest.Y) break;
                if (closedArray[node.Y, node.X] == true) continue;

                closedArray[node.Y, node.X] = true;

                for (int i = 0; i < dx.Length; i++)
                {
                    Pos next = new Pos(node.X + dx[i], node.Y + dy[i]);

                    if (next.X == dest.X && next.Y == dest.Y)
                    {
                        pq.Push(new PQNode() { F = 0, G = 0, X = next.X, Y = next.Y });
                        parentArray[next.Y, next.X] = new Pos(node.X, node.Y);
                        break;
                    }

                    if (CheckCanMove(next, true) == false) continue;
                    if (closedArray[next.Y, next.X] == true) continue;

                    int g = node.G + cost[i];
                    int h = Utility.CalculateDistance(next, dest);

                    if (openArray[next.Y, next.X] < g + h) continue;

                    openArray[dest.Y, dest.X] = g + h;
                    pq.Push(new PQNode() { F = g + h, G = g, X = next.X, Y = next.Y });
                    parentArray[next.Y, next.X] = new Pos(node.X, node.Y);
                }
            }

            path = new List<Vector2Int>();
            int tx = dest.X;
            int ty = dest.Y;

            while (parentArray[ty, tx].X >= 0 && parentArray[ty, tx].Y >= 0)
            {
                path.Add(ConvertPosToCell(new Pos(tx, ty)));

                Pos parent = parentArray[ty, tx];
                tx = parent.X;
                ty = parent.Y;
            }

            if (tx != start.X || ty != start.Y) return false;

            path.Add(ConvertPosToCell(new Pos(tx, ty)));
            path.Reverse();

            return true;
        }

        private Pos ConvertCellToPos(Vector2Int cell) => new Pos(cell.X + minX, cell.Y + minY);

        private Vector2Int ConvertPosToCell(Pos pos) => new Vector2Int(pos.X - minX, pos.Y - minY);

        #endregion A* PathFinding

        #endregion Methods
    }
}