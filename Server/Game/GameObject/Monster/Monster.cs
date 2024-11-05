using Google.Protobuf.Protocol;
using Server.Game.MonsterState;

namespace Server.Game
{
    public class Monster : GameObject
    {
        #region Variables

        private long nextDetectionTicks = 0;

        #endregion Variables

        #region Properties

        public int PatrolRange { private set; get; }
        public Pos PatrolPos { set; get; }

        public int DetectionRange { private set; get; }
        public int ChaseRange { private set; get; }
        public Character Target { set; get; }

        #endregion Properties

        #region Constructor

        public Monster()
        {
            ObjectType = EGameObjectType.Monster;
            IsCollidable = true;

            stateDictionary.Add(ECreatureState.Idle, new IdleState(this));
            stateDictionary.Add(ECreatureState.Move, new MoveState(this));
            stateDictionary.Add(ECreatureState.Attack, new AttackState(this));
            stateDictionary.Add(ECreatureState.Dead, new DeadState(this));

            if (DataManager.MonsterStatDictionary.TryGetValue(1, out Data.MonsterStat statData) == false) return;

            // TODO : The stat needs to be adjusted based on the monster's level
            Stat.CurHP = Stat.MaxHP = statData.MaxHpDictionary[1];
            Stat.AttackPower = statData.AttackPowerDictionary[1];
            PatrolRange = statData.PatrolRange;
            DetectionRange = statData.DetectionRange;
            ChaseRange = statData.ChaseRange;

            CurState = ECreatureState.Idle;

            Updated += DetectPlayer;
        }

        #endregion Constructor

        #region Methods

        private void DetectPlayer()
        {
            if (nextDetectionTicks > Environment.TickCount64) return;
            nextDetectionTicks = Environment.TickCount64 + 1000;

            GameRoom room = this.Room;
            if (ReferenceEquals(room, null) == true) return;

            Character target = room.FindCharacter(p =>
            {
                return Utility.CalculateDistance(p.Position, Position) <= DetectionRange;
            });

            if (ReferenceEquals(target, null) == true) return;

            Target = target;
        }

        #region Events

        public override void OnDamaged(GameObject attacker, int damage)
        {
            base.OnDamaged(attacker, damage);
        }

        #endregion Events

        #endregion Methods
    }
}