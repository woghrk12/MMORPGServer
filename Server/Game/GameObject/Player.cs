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

            if (DataManager.ObjectStatDictionary.TryGetValue(1, out Data.ObjectStat statData) == false) return;

            // TODO : The stat needs to be adjusted based on the player's level
            Stat.CurHP = Stat.MaxHP = statData.MaxHpDictionary[1];
            Stat.AttackPower = statData.AttackPowerDictionary[1];
        }

        #endregion Constructor

        #region Methods

        #region Events

        public override void OnDamaged(GameObject attacker, int damage)
        {
            base.OnDamaged(attacker, damage);
        }

        #endregion Events

        #endregion Methods
    }
}