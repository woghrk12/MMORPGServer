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
            if (ReferenceEquals(Room, null) == true || ReferenceEquals(Owner, null) == true || ReferenceEquals(AttackStat, null) == true) return;
            if (nextMoveTicks >= Environment.TickCount64) return;

            nextMoveTicks = Environment.TickCount64 + (long)(1000f / MoveSpeed);

            Pos destPos = Utility.GetFrontPos(Position, MoveDirection);

            if (Room.Map.CheckCanMove(destPos) == true)
            {
                PerformMoveBroadcast packet = new()
                {
                    ObjectID = ID,
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
                    foreach (GameObject obj in objectList)
                    {
                        if (obj.ID == Owner.ID) continue;

                        Creature damagable = obj as Creature;

                        if (ReferenceEquals(damagable, null) == true) continue;
                        if (damagable.CurState == ECreatureState.Dead) continue;

                        // TODO : The attack coefficient needs to be adjusted based on the attacker's level
                        damagable.OnDamaged(Owner, Owner.AttackPower * AttackStat.CoeffDictionary[1]);
                    }
                }

                Room.RemoveObject(ID);
            }
        }

        #endregion Methods
    }
}