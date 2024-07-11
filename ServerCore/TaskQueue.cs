using System;
using System.Collections.Generic;

namespace ServerCore
{
    public interface ITaskQueue
    {
        public void Push(Action task);
    }

    public class TaskQueue : ITaskQueue
    {
        #region Variables

        private object lockObj = new();

        private Queue<Action> taskQueue = new();
        private bool isFlush = false;

        #endregion Variables

        #region Methods

        public void Push(Action task)
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

        public Action Pop()
        {
            lock (lockObj)
            {
                if (taskQueue.TryDequeue(out Action result) == false)
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
                Action action = Pop();

                if (ReferenceEquals(action, null)) return;

                action.Invoke();
            }
        }

        #endregion Methods
    }
}