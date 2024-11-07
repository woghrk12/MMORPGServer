using Google.Protobuf.Protocol;
using Server.Data;

namespace Server.Game
{
    public class Creature : GameObject
    {
        #region Variables

        protected Dictionary<ECreatureState, State> stateDictionary = new();
        protected State curState = null;

        private EMoveDirection moveDirection = EMoveDirection.None;

        private CreatureStat stat = new();
        private event Action<int> levelModified = null;
        private event Action<int> curHpModified = null;
        private event Action<int> maxHpModified = null;
        private event Action<int> attackPowerModified = null;

        private long attackEndTicks = 0;

        #endregion Variables

        #region Properties

        public ECreatureState CurState
        {
            set
            {
                if (stateDictionary.ContainsKey(value) == false) return;

                if (ReferenceEquals(curState, null) == false)
                {
                    if (curState.StateID == value) return;

                    curState.OnExit();
                }

                curState = stateDictionary[value];
                curState.OnEnter();

                GameRoom room = Room;
                if (ReferenceEquals(room, null) == true) return;

                UpdateObjectStateBroadcast packet = new()
                {
                    ObjectID = ID,
                    NewState = curState.StateID
                };

                room.Broadcast(packet);
            }
            get => ReferenceEquals(curState, null) == false ? curState.StateID : ECreatureState.Idle;
        }

        public EMoveDirection MoveDirection
        {
            set
            {
                if (moveDirection == value) return;

                moveDirection = value;

                if (moveDirection != EMoveDirection.None)
                {
                    FacingDirection = moveDirection;
                }
            }
            get => moveDirection;
        }

        public EMoveDirection FacingDirection { set; get; } = EMoveDirection.Right;

        public int MoveSpeed { set; get; } = 5;

        public int Level
        {
            set
            {
                stat.Level = Math.Max(value, 1);
                levelModified?.Invoke(stat.Level);
            }
            get => stat.Level;
        }

        public event Action<int> LevelModified { add { levelModified += value; } remove { levelModified -= value; } }

        public int CurHp
        {
            set
            {
                stat.CurHP = Math.Clamp(value, 0, stat.MaxHP);
                curHpModified?.Invoke(stat.CurHP);
            }
            get => stat.CurHP;
        }

        public event Action<int> CurHpModified { add { curHpModified += value; } remove { curHpModified -= value; } }

        public int MaxHp
        {
            set
            {
                stat.MaxHP = Math.Max(value, 1);
                maxHpModified?.Invoke(stat.MaxHP);
            }
            get => stat.MaxHP;
        }

        public event Action<int> MaxHpModified { add { maxHpModified += value; } remove { maxHpModified -= value; } }

        public int AttackPower
        {
            set
            {
                stat.AttackPower = Math.Max(value, 0);
                attackPowerModified?.Invoke(stat.AttackPower);
            }
            get => stat.AttackPower;
        }

        public event Action<int> AttackPowerModified { add { attackPowerModified += value; } remove { attackPowerModified -= value; } }

        public AttackStat AttackStat { set; get; } = null;

        #endregion Properties

        #region Methods

        public void BeginAttackPostDelayCheck(long attackEndTicks)
        {
            this.attackEndTicks = attackEndTicks;

            Updated += CheckAttackPostDelay;
        }

        private void CheckAttackPostDelay()
        {
            if (attackEndTicks > Environment.TickCount64) return;

            CurState = ECreatureState.Idle;

            AttackCompleteBroadcast packet = new()
            {
                ObjectID = ID
            };

            foreach (Character character in Room.CharacterDictionary.Values)
            {
                character.Session.Send(packet);
            }

            Updated -= CheckAttackPostDelay;
        }

        #region Events

        public sealed override void OnUpdate()
        {
            base.OnUpdate();

            curState?.OnUpdate();
        }

        public virtual void OnDamaged(GameObject attacker, int damage)
        {
            CurHp -= damage;

            GameRoom room = Room;
            if (ReferenceEquals(room, null) == false)
            {
                HitBroadcast packet = new()
                {
                    ObjectID = ID,
                    CurHp = CurHp,
                    Damage = damage
                };

                room.Broadcast(packet);
            }

            if (CurHp <= 0)
            {
                OnDead(attacker);
            }
        }

        public virtual void OnDead(GameObject attacker) { }

        #endregion Events

        #endregion Methods
    }
}