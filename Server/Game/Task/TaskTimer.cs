namespace Server.Game
{
    public struct TaskTimerElement : IComparable<TaskTimerElement>
    {
        public ITask task = null;
        public int execTick = 0;

        public TaskTimerElement(ITask task, int execTick)
        {
            this.task = task;
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

        #region Methods

        public void Push(ITask task, int tickAfter = 0)
        {
            TaskTimerElement element = new(task, Environment.TickCount + tickAfter);

            lock (lockObj)
            {
                priorityQueue.Push(element);
            }
        }

        public void Flush()
        {
            while (true)
            {
                int now = Environment.TickCount;

                TaskTimerElement element;

                lock (lockObj)
                {
                    if (priorityQueue.Count == 0) break;

                    element = priorityQueue.Peek();

                    if (element.execTick > now) break;

                    priorityQueue.Pop();
                }

                element.task.Execute();
            }
        }

        #endregion Methods   
    }
}