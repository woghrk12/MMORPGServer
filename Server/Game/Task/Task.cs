namespace Server.Game
{
    public interface ITask
    {
        public void Execute();
    }

    public class Task : ITask
    {
        #region Variables

        private Action action = null;

        #endregion Variables

        #region Constructor

        public Task(Action action)
        {
            this.action = action;
        }

        #endregion Constructor

        #region Methods

        public void Execute()
        {
            action?.Invoke();
        }

        #endregion Methods
    }

    public class Task<P1> : ITask
    {
        #region Variables

        private Action<P1> action = null;
        private P1 p1 = default;

        #endregion Variables

        #region Constructor

        public Task(Action<P1> action, P1 p1)
        {
            this.action = action;
            this.p1 = p1;
        }

        #endregion Constructor

        #region Methods

        public void Execute()
        {
            action?.Invoke(p1);
        }

        #endregion Methods
    }

    public class Task<P1, P2> : ITask
    {
        #region Variables

        private Action<P1, P2> action = null;
        private P1 p1 = default;
        private P2 p2 = default;

        #endregion Variables

        #region Constructor

        public Task(Action<P1, P2> action, P1 p1, P2 p2)
        {
            this.action = action;
            this.p1 = p1;
            this.p2 = p2;
        }

        #endregion Constructor

        #region Methods

        public void Execute()
        {
            action?.Invoke(p1, p2);
        }

        #endregion Methods
    }

    public class Task<P1, P2, P3> : ITask
    {
        #region Variables

        private Action<P1, P2, P3> action = null;
        private P1 p1 = default;
        private P2 p2 = default;
        private P3 p3 = default;

        #endregion Variables

        #region Constructor

        public Task(Action<P1, P2, P3> action, P1 p1, P2 p2, P3 p3)
        {
            this.action = action;
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }

        #endregion Constructor

        #region Methods

        public void Execute()
        {
            action?.Invoke(p1, p2, p3);
        }

        #endregion Methods
    }
}