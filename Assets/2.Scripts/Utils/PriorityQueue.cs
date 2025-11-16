using System.Collections.Generic;
using UnityEngine;

namespace PriorityQueue
{
    public class PriorityQueue<T>
    {
        private readonly List<(int priority, T item)> _heap = new();

        public void Enqueue(T item, int priority)
        {
            _heap.Add((priority, item));
            int i = _heap.Count - 1;

            while (i > 0)
            {
                int parent = (i - 1) / 2;
                if (_heap[parent].priority <= _heap[i].priority)
                    break;

                (_heap[parent], _heap[i]) = (_heap[i], _heap[parent]);
                i = parent;
            }
        }

        public T Dequeue()
        {
            var root = _heap[0].item;
            _heap[0] = _heap[^1];
            _heap.RemoveAt(_heap.Count - 1);

            int i = 0;
            while (true)
            {
                int left = i * 2 + 1;
                int right = i * 2 + 2;
                int smallest = i;

                if (left < _heap.Count && _heap[left].priority < _heap[smallest].priority)
                    smallest = left;

                if (right < _heap.Count && _heap[right].priority < _heap[smallest].priority)
                    smallest = right;

                if (smallest == i) break;

                (_heap[smallest], _heap[i]) = (_heap[i], _heap[smallest]);
                i = smallest;
            }

            return root;
        }

        public int Count => _heap.Count;
    }
}

