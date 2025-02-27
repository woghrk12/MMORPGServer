using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;

namespace Server.Game
{
    public class Character : Creature
    {
        #region Properties

        public ClientSession Session { set; get; }

        /// <summary>
        /// The character ID used in the database.
        /// </summary>
        public int CharacterID { set; get; }

        public Inventory Inventory { private set; get; } = new();

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
            DBTransaction.SavePlayerStatus(this, Room);
        }

        #endregion Events

        #endregion Methods
    }
}