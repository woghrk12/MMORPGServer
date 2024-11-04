using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class Player : GameObject
    {
        #region Properties

        public ClientSession Session { set; get; }

        #endregion Properties

        #region Constructor

        public Player()
        {
            ObjectType = EGameObjectType.Player;
            IsCollidable = true;
        }

        #endregion Constructor

        #region Methods

        #region Events

        public override void OnDamaged(GameObject attacker, int damage)
        {
            base.OnDamaged(attacker, damage);
        }

        public override void OnDead(GameObject attacker)
        {
            ObjectDeadBroadcast packet = new()
            {
                ObjectID = ID,
                AttackerID = attacker.ID
            };

            Room.Broadcast(packet);

            CurState = EObjectState.Dead;
            IsCollidable = false;
        }

        #endregion Events

        #endregion Methods
    }
}