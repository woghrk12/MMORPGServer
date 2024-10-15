namespace Server.Game
{
    public interface ITaskQueue
    {
        public void Push(ITask task);
    }

    public class TaskQueue : ITaskQueue
    {
        #region Variables

        private object lockObj = new();

        private Queue<ITask> taskQueue = new();
        private bool isFlush = false;

        #endregion Variables

        #region Methods

        public void Push(Action action) { Push(new Task(action)); }
        public void Push<P1>(Action<P1> action, P1 p1) { Push(new Task<P1>(action, p1)); }
        public void Push<P1, P2>(Action<P1, P2> action, P1 p1, P2 p2) { Push(new Task<P1, P2>(action, p1, p2)); }
        public void Push<P1, P2, P3>(Action<P1, P2, P3> action, P1 p1, P2 p2, P3 p3) { Push(new Task<P1, P2, P3>(action, p1, p2, p3)); }

        public void Push(ITask task)
        {
            bool isFlush = false;

            lock (lockObj)
            {
                taskQueue.Enqueue(task);

                if (this.isFlush == false)
                {
                    isFlush = this.isFlush = true;
                }
            }

            if (isFlush == true)
            {
                Flush();
            }
        }

        public ITask Pop()
        {
            lock (lockObj)
            {
                if (taskQueue.TryDequeue(out ITask result) == false)
                {
                    isFlush = false;
                    return null;
                }

                return result;
            }
        }

        private void Flush()
        {
            while (true)
            {
                ITask task = Pop();

                if (ReferenceEquals(task, null)) return;

                task.Execute();
            }
        }

        #endregion Methods
    }

}