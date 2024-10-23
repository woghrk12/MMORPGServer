using Google.Protobuf.Protocol;
using Server.Game.MonsterState;

namespace Server.Game
{
    public class Monster : GameObject
    {
        #region Properties

        public int PatrolRange { private set; get; }

        public int DetectionRange { private set; get; }

        public int ChaseRange { private set; get; }

        public Player Target { set; get; } = null;

        #endregion Properties

        #region Constructor

        public Monster()
        {
            ObjectType = EGameObjectType.Monster;
            IsCollidable = true;

            stateDictionary.Add(EObjectState.Idle, new IdleState(this));
            stateDictionary.Add(EObjectState.Move, new MoveState(this));
            stateDictionary.Add(EObjectState.Attack, new AttackState(this));
            stateDictionary.Add(EObjectState.Dead, new DeadState(this));

            if (DataManager.MonsterStatDictionary.TryGetValue(1, out Data.MonsterStat statData) == false) return;

            // TODO : The stat needs to be adjusted based on the monster's level
            Stat.CurHP = Stat.MaxHP = statData.MaxHpDictionary[1];
            Stat.AttackPower = statData.AttackPowerDictionary[1];
            PatrolRange = statData.PatrolRange;
            DetectionRange = statData.DetectionRange;
            ChaseRange = statData.ChaseRange;

            CurState = EObjectState.Idle;
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