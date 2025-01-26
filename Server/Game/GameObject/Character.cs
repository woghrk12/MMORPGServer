using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using Microsoft.EntityFrameworkCore;

namespace Server.Game
{
    public class Character : Creature
    {
        #region Properties

        public ClientSession Session { set; get; }

        public int CharacterID { set; get; }

        #endregion Properties

        #region Constructor

        public Character()
        {
            ObjectType = EGameObjectType.Character;
            IsCollidable = true;
        }

        #endregion Constructor

        #region Methods

        public override bool CheckCanAttack(AttackStat attackStat)
        {
            if (CurState != ECreatureState.Idle) return false;

            return base.CheckCanAttack(attackStat);
        }

        #region Events

        public override void OnDamaged(GameObject attacker, int damage)
        {
            base.OnDamaged(attacker, damage);
        }

        public override void OnDead(GameObject attacker)
        {
            CreatureDeadBroadcast packet = new()
            {
                CreatureID = ID,
                AttackerID = attacker.ID
            };

            Room.Broadcast(packet);

            CurState = ECreatureState.Dead;
            IsCollidable = false;
        }

        public void OnLeftRoom()
        {
            using (AppDBContext db = new())
            {
                CharacterDB characterDB = new();

                characterDB.ID = CharacterID;
                characterDB.CurHp = CurHp;

                db.Entry(characterDB).State = EntityState.Unchanged;
                db.Entry(characterDB).Property(nameof(characterDB.CurHp)).IsModified = true;

                db.SaveChangesEx();
            }
        }

        #endregion Events

        #endregion Methods
    }
}