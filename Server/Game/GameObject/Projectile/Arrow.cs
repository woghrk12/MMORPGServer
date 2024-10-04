using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class Arrow : Projectile
    {
        #region Variables

        private long nextMoveTicks = 0;

        #endregion Variables

        #region Constructor

        public Arrow() : base()
        {
            Updated += Move;
        }

        #endregion Constructor

        #region Methods

        public void Move()
        {
            if (ReferenceEquals(Room, null) == true || ReferenceEquals(Owner, null) == true) return;
            if (nextMoveTicks >= Environment.TickCount64) return;

            nextMoveTicks = Environment.TickCount64 + 50;

            Pos destPos = GetFrontPos();
            if (Room.Map.CheckCanMove(destPos) == true)
            {
                PerformMoveBroadcast packet = new()
                {
                    ObjectID = ID,
                    CurPosX = Position.X,
                    CurPosY = Position.Y,
                    TargetPosX = destPos.X,
                    TargetPosY = destPos.Y
                };

                Room.Broadcast(packet);

                Position = destPos;
            }
            else
            {
                if (Room.Map.Find(destPos, out List<GameObject> objectList) == true)
                {
                    List<GameObject> damagableList = objectList.FindAll((obj) => obj.ObjectType != EGameObjectType.Projectile);

                    foreach (GameObject damagable in damagableList)
                    {
                        // TODO : hit detection
                    }
                }

                Room.RemoveObject(ID);
            }
        }

        #endregion Methods
    }
}