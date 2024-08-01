namespace ServerCore
{
    public struct TaskTimerElement : IComparable<TaskTimerElement>
    {
        public int execTick;
        public Action action;

        public TaskTimerElement(Action action, int execTick)
        {
            this.action = action;
            this.execTick = execTick;
        }

        public int CompareTo(TaskTimerElement other)
        {
            return other.execTick - execTick;
        }
    }

    public class TaskTimer
    {
        #region Variables

        private object lockObj = new();

        private PriorityQueue<TaskTimerElement> priorityQueue = new();

        #endregion Variables

        #region Properties

        public static TaskTimer Instance { get; } = new();

        #endregion Properties

        #region Methods

        public void Push(Action action, int tickAfter = 0)
        {
            TaskTimerElement task = new(action, System.Environment.TickCount + tickAfter);

            lock (lockObj)
            {
                priorityQueue.Push(task);
            }
        }

        public void Flush()
        {
            while (true)
            {
                int now = System.Environment.TickCount;

                TaskTimerElement task;

                lock (lockObj)
                {
                    if (priorityQueue.Count == 0) break;

                    task = priorityQueue.Peek();

                    if (task.execTick > now) break;

                    priorityQueue.Pop();
                }

                task.action.Invoke();
            }
        }

        #endregion Methods   
    }
}