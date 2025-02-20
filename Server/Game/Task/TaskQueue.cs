namespace Server.Game
{
    public class TaskQueue
    {
        #region Variables

        private object lockObj = new();

        private PriorityQueue<TaskBase> priorityQueue = new();

        #endregion Variables

        #region Methods

        public void Push(Action action, long afterTick = 0)
        {
            Task task = new Task(action, Environment.TickCount64 + afterTick);

            Push(task);
        }

        public void Push<P1>(Action<P1> action, P1 p1, long afterTick = 0)
        {
            Task<P1> task = new Task<P1>(action, p1, Environment.TickCount64 + afterTick);

            Push(task);
        }

        public void Push<P1, P2>(Action<P1, P2> action, P1 p1, P2 p2, long afterTick = 0)
        {
            Task<P1, P2> task = new Task<P1, P2>(action, p1, p2, Environment.TickCount64 + afterTick);

            Push(task);
        }

        public void Push<P1, P2, P3>(Action<P1, P2, P3> action, P1 p1, P2 p2, P3 p3, long afterTick = 0)
        {
            Task<P1, P2, P3> task = new Task<P1, P2, P3>(action, p1, p2, p3, Environment.TickCount64 + afterTick);

            Push(task);
        }

        public void Push<P1, P2, P3, P4>(Action<P1, P2, P3, P4> action, P1 p1, P2 p2, P3 p3, P4 p4, long afterTick = 0)
        {
            Task<P1, P2, P3, P4> task = new Task<P1, P2, P3, P4>(action, p1, p2, p3, p4, Environment.TickCount64 + afterTick);

            Push(task);
        }

        public void Flush()
        {
            TaskBase task = null;

            while (true)
            {
                lock (lockObj)
                {
                    if (priorityQueue.Count <= 0) break;

                    task = priorityQueue.Peek();
                    long now = Environment.TickCount64;

                    if (task.ExecTick > now) break;

                    priorityQueue.Pop();
                }

                task?.Execute();
            }
        }

        private void Push(TaskBase task)
        {
            lock (lockObj)
            {
                priorityQueue.Push(task);
            }
        }

        #endregion Methods
    }

}