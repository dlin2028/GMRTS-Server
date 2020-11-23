using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSServerCore.SimClasses
{
    //Based on https://github.com/GreatMindsRobotics/DataStructuresCurriculum/wiki/Heap-Tree but with a Dictionary to allow for more flexibility and O(1) contains
    class FancyHeap<TKey, TPriority> where TPriority : IComparable<TPriority>
    {
        List<TKey> things = new List<TKey>();
        Dictionary<TKey, TPriority> priorities = new Dictionary<TKey, TPriority>();

        public bool Contains(TKey thing)
        {
            return priorities.ContainsKey(thing);
        }

        public int Count
        {
            get => things.Count;
        }

        public void Enqueue(TKey thing, TPriority priority)
        {
            priorities.Add(thing, priority);

            things.Add(thing);

            HeapifyUp(Count - 1);
        }

        private void HeapifyUp(int n)
        {
            if (n == 0)
            {
                return;
            }

            int parentInd = (n - 1) / 2;
            
            if (priorities[things[parentInd]].CompareTo(priorities[things[n]]) <= 0)
            {
                return;
            }

            TKey thing = things[n];
            things[n] = things[parentInd];
            things[parentInd] = thing;

            HeapifyUp(parentInd);
        }

        public TKey Dequeue()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("Empty heap thingy");
            }

            TKey toRet = things[0];

            things[0] = things[Count - 1];
            things.RemoveAt(Count - 1);

            priorities.Remove(toRet);

            HeapifyDown(0);

            return toRet;
        }

        private void HeapifyDown(int n)
        {
            int left = n * 2 + 1;

            if (left >= Count)
            {
                return;
            }

            int right = left + 1;

            int ind = left;

            if (right < Count && priorities[things[right]].CompareTo(priorities[things[left]]) < 0)
            {
                ind = right;
            }

            if (priorities[things[ind]].CompareTo(priorities[things[n]]) > 0)
            {
                return;
            }

            TKey thing = things[ind];
            things[ind] = things[n];
            things[n] = thing;

            HeapifyDown(ind);

        }
    }
}
