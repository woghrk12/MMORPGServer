namespace ServerCore
{
    public class PriorityQueue<T> where T : IComparable<T>
    {
        #region Variables

        private List<T> heap = new();

        #endregion Variables

        #region Properties

        public int Count => heap.Count;

        #endregion Properties

        #region Methods

        public void Push(T item)
        {
            heap.Add(item);

            int index = heap.Count - 1;
            while (index > 0)
            {
                int parent = (index - 1) / 2;

                if (heap[index].CompareTo(heap[parent]) < 0) break;

                T temp = heap[index];
                heap[index] = heap[parent];
                heap[parent] = temp;

                index = parent;
            }
        }

        public T Pop()
        {
            T result = heap[0];

            int lastIndex = heap.Count - 1;
            heap[0] = heap[lastIndex];
            heap.RemoveAt(lastIndex);
            lastIndex--;

            int index = 0;
            while (true)
            {
                int left = 2 * index + 1;
                int right = 2 * index + 2;

                int next = index;
                if (left <= lastIndex && heap[next].CompareTo(heap[left]) < 0)
                {
                    next = left;
                }
                if (right <= lastIndex && heap[next].CompareTo(heap[right]) < 0)
                {
                    next = right;
                }

                if (next == index) break;

                T temp = heap[index];
                heap[index] = heap[next];
                heap[next] = temp;

                index = next;
            }

            return result;
        }

        public T Peek()
        {
            if (heap.Count == 0) return default(T);

            return heap[0];
        }

        #endregion Methods
    }
}