namespace Server.Game
{
    public abstract class TaskBase : IComparable<TaskBase>
    {
        #region Properties

        public long ExecTick { protected set; get; }

        #endregion Properties

        #region Methods

        public abstract void Execute();

        public int CompareTo(TaskBase other)
        {
            long diff = other.ExecTick - ExecTick;

            return diff != 0 ? (diff > 0 ? 1 : -1) : 0;
        }

        #endregion Methods
    }

    public class Task : TaskBase
    {
        #region Variables

        private Action action = null;

        #endregion Variables

        #region Constructor

        public Task(Action action, long execTick = 0)
        {
            this.action = action;

            ExecTick = execTick;
        }

        #endregion Constructor

        #region Methods

        public override void Execute()
        {
            action?.Invoke();
        }

        #endregion Methods
    }

    public class Task<P1> : TaskBase
    {
        #region Variables

        private Action<P1> action = null;
        private P1 p1 = default;

        #endregion Variables

        #region Constructor

        public Task(Action<P1> action, P1 p1, long execTick = 0)
        {
            this.action = action;
            this.p1 = p1;

            ExecTick = execTick;
        }

        #endregion Constructor

        #region Methods

        public override void Execute()
        {
            action?.Invoke(p1);
        }

        #endregion Methods
    }

    public class Task<P1, P2> : TaskBase
    {
        #region Variables

        private Action<P1, P2> action = null;
        private P1 p1 = default;
        private P2 p2 = default;

        #endregion Variables

        #region Constructor

        public Task(Action<P1, P2> action, P1 p1, P2 p2, long execTick = 0)
        {
            this.action = action;
            this.p1 = p1;
            this.p2 = p2;

            ExecTick = execTick;
        }

        #endregion Constructor

        #region Methods

        public override void Execute()
        {
            action?.Invoke(p1, p2);
        }

        #endregion Methods
    }

    public class Task<P1, P2, P3> : TaskBase
    {
        #region Variables

        private Action<P1, P2, P3> action = null;
        private P1 p1 = default;
        private P2 p2 = default;
        private P3 p3 = default;

        #endregion Variables

        #region Constructor

        public Task(Action<P1, P2, P3> action, P1 p1, P2 p2, P3 p3, long execTick = 0)
        {
            this.action = action;
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;

            ExecTick = execTick;
        }

        #endregion Constructor

        #region Methods

        public override void Execute()
        {
            action?.Invoke(p1, p2, p3);
        }

        #endregion Methods
    }
}