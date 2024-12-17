using Google.Protobuf.Protocol;
using Server.Game.MonsterAI;

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

        private State curMonsterState = null;
        private Dictionary<EMonsterState, State> stateDictionary = new();
        private Dictionary<EMonsterState, Transition> transitionDictionary = new();

        #endregion Variables

        #region Properties

        public EMonsterState CurMonsterState
        {
            set
            {
                if (stateDictionary.ContainsKey(value) == false) return;

                if (ReferenceEquals(curMonsterState, null) == false)
                {
                    if (curMonsterState.MonsterStateID == value) return;

                    curMonsterState.OnExit();
                }

                curMonsterState = stateDictionary[value];
                curMonsterState.OnEnter();
            }
            get => ReferenceEquals(curMonsterState, null) == false ? curMonsterState.MonsterStateID : EMonsterState.IDLE;
        }

        public Character Target { set; get; }
        public Pos TargetPos { set; get; }

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

            stateDictionary.Add(EMonsterState.IDLE, new IdleState(this));
            stateDictionary.Add(EMonsterState.PATROL, new PatrolState(this, statData.PatrolRange));
            stateDictionary.Add(EMonsterState.CHASING, new ChaseState(this, statData.ChaseRange));
            stateDictionary.Add(EMonsterState.ATTACK, new AttackState(this, statData.AttackID));

            transitionDictionary.Add(EMonsterState.IDLE, new Transition(this, EMonsterState.PATROL, new List<Decision>
            {
                new NoTargetDecision(this, EMonsterState.PATROL, statData.DetectionRange),
                new TargetInRangeDecision(this, EMonsterState.ATTACK, 1),
                new ReachableTargetDecision(this, EMonsterState.CHASING, statData.DetectionRange),
            }));
            transitionDictionary.Add(EMonsterState.PATROL, new Transition(this, EMonsterState.PATROL, new List<Decision>
            {
                new NoTargetDecision(this, EMonsterState.PATROL, statData.DetectionRange),
                new TargetInRangeDecision(this, EMonsterState.ATTACK, 1),
                new ReachableTargetDecision(this, EMonsterState.CHASING, statData.DetectionRange)
            }));
            transitionDictionary.Add(EMonsterState.CHASING, new Transition(this, EMonsterState.IDLE, new List<Decision>
            {
                new TargetInRangeDecision(this, EMonsterState.ATTACK, 1),
                new ReachableTargetDecision(this, EMonsterState.CHASING, statData.DetectionRange)
            }));
            transitionDictionary.Add(EMonsterState.ATTACK, new Transition(this, EMonsterState.IDLE, new List<Decision>
            {
                new TargetInRangeDecision(this, EMonsterState.ATTACK, 1),
                new ReachableTargetDecision(this, EMonsterState.CHASING, statData.DetectionRange)
            }));

            CurMonsterState = EMonsterState.IDLE;
        }

        #endregion Constructor

        #region Methods

        #region Events

        protected sealed override void OnUpdate()
        {
            base.OnUpdate();

            if (curMonsterState?.IsTransitionBlocked == false
                && transitionDictionary.TryGetValue(curMonsterState.MonsterStateID, out Transition transition) == true)
            {
                EMonsterState newState = transition.Evaluate();

                if (newState != curMonsterState.MonsterStateID)
                {
                    CurMonsterState = newState;
                    return;
                }
            }

            curMonsterState?.OnUpdate();
        }

        public override void OnDamaged(GameObject attacker, int damage)
        {
            base.OnDamaged(attacker, damage);
        }

        #endregion Events

        #endregion Methods
    }
}