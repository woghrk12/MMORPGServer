using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class Arrow : Projectile
    {
        #region Methods

        protected override bool IsDestroyed(Pos frontPos)
        {
            GameRoom room = Room;
            if (ReferenceEquals(room, null) == true) return true;

            Creature owner = Owner;
            if (ReferenceEquals(owner, null) == true) return true;

            if (room.Map.Find(frontPos, out List<GameObject> objectList) == false) return false;

            foreach (GameObject obj in objectList)
            {
                if (obj.ID == owner.ID) continue;

                Creature damagable = obj as Creature;

                if (ReferenceEquals(damagable, null) == true) continue;
                if (damagable.CurState == ECreatureState.Dead) continue;

                damagable.OnDamaged(owner, owner.AttackPower * AttackStat.CoeffDictionary[1]);
            }

            return true;
        }

        #endregion Methods
    }
}