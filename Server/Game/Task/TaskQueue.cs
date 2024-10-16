namespace Server.Game
{
    public class TaskQueue
    {
        #region Variables

        private object lockObj = new();

        private TaskTimer taskTimer = new();

        private Queue<ITask> taskQueue = new();
        private bool isFlush = false;

        #endregion Variables

        #region Methods

        public void Push(Action action, int tickAfter = 0)
        {
            Task task = new Task(action);

            if (tickAfter > 0)
            {
                PushAfter(task, tickAfter);
            }
            else
            {
                Push(task);
            }
        }

        public void Push<P1>(Action<P1> action, P1 p1, int tickAfter = 0)
        {
            Task<P1> task = new Task<P1>(action, p1);

            if (tickAfter > 0)
            {
                PushAfter(task, tickAfter);
            }
            else
            {
                Push(task);
            }
        }

        public void Push<P1, P2>(Action<P1, P2> action, P1 p1, P2 p2, int tickAfter = 0)
        {
            Task<P1, P2> task = new Task<P1, P2>(action, p1, p2);

            if (tickAfter > 0)
            {
                PushAfter(task, tickAfter);
            }
            else
            {
                Push(task);
            }
        }

        public void Push<P1, P2, P3>(Action<P1, P2, P3> action, P1 p1, P2 p2, P3 p3, int tickAfter = 0)
        {
            Task<P1, P2, P3> task = new Task<P1, P2, P3>(action, p1, p2, p3);

            if (tickAfter > 0)
            {
                PushAfter(task, tickAfter);
            }
            else
            {
                Push(task);
            }
        }

        private void Push(ITask task)
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

        private void PushAfter(ITask task, int tickAfter)
        {
            taskTimer.Push(task, tickAfter);
        }

        private ITask Pop()
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
            taskTimer.Flush();

            while (taskQueue.Count > 0)
            {
                ITask task = Pop();

                task.Execute();
            }
        }

        #endregion Methods
    }

}