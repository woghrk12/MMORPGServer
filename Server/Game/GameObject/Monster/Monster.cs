using Google.Protobuf.Protocol;
using Server.Data;

namespace Server.Game
{
    public enum EMonsterState
    {
        IDLE = 0,
        PATROL = 1,
        CHASING = 2,
        ATTACK = 3
    }

    public class Monster : Creature
    {
        #region Variables

        private Dictionary<EMonsterState, MonsterState> stateDictionary = new();
        private MonsterState monsterState = null;

        private long nextDetectionTicks = 0;
        private int detectionRange = 0;

        #endregion Variables

        #region Properties

        public EMonsterState MonsterState
        {
            set
            {
                if (stateDictionary.ContainsKey(value) == false) return;

                if (ReferenceEquals(monsterState, null) == false)
                {
                    if (monsterState.MonsterStateID == value) return;

                    monsterState.OnExit();
                }

                monsterState = stateDictionary[value];
                monsterState.OnEnter();
            }
            get => ReferenceEquals(monsterState, null) == false ? monsterState.MonsterStateID : EMonsterState.IDLE;
        }

        public Character Target { set; get; }

        #endregion Properties

        #region Constructor

        public Monster()
        {
            ObjectType = EGameObjectType.Monster;
            IsCollidable = true;

            if (DataManager.MonsterStatDictionary.TryGetValue(1, out Data.MonsterStat statData) == false) return;

            // TODO : The stat needs to be adjusted based on the monster's level
            CurHp = MaxHp = statData.MaxHpDictionary[1];
            AttackPower = statData.AttackPowerDictionary[1];

            detectionRange = statData.DetectionRange;

            stateDictionary.Add(EMonsterState.IDLE, new IdleState(this));
            stateDictionary.Add(EMonsterState.PATROL, new PatrolState(this, statData.PatrolRange));
            stateDictionary.Add(EMonsterState.CHASING, new ChaseState(this, statData.ChaseRange));
            stateDictionary.Add(EMonsterState.ATTACK, new AttackState(this, statData.AttackID));

            MonsterState = EMonsterState.IDLE;

            Updated += DetectPlayer;
        }

        #endregion Constructor

        #region Methods

        private void DetectPlayer()
        {
            if (nextDetectionTicks > Environment.TickCount64) return;
            nextDetectionTicks = Environment.TickCount64 + 1000;

            GameRoom room = Room;
            if (ReferenceEquals(room, null) == true) return;

            Character target = room.FindCharacter(p =>
            {
                return room.Map.FindPath(Position, p.Position, out List<Pos> path, detectionRange);
            });

            if (ReferenceEquals(target, null) == true) return;

            Target = target;
        }

        #region Events

        protected sealed override void OnUpdate()
        {
            base.OnUpdate();

            monsterState?.OnUpdate();
        }

        public override void OnDamaged(GameObject attacker, int damage)
        {
            base.OnDamaged(attacker, damage);
        }

        #endregion Events

        #endregion Methods
    }
}