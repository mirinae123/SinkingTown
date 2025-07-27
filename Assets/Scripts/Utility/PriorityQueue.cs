using System;
using System.Collections.Generic;
using UnityEngine.Rendering.UI;

public class PriorityQueue<T, S> where S : IComparable<S>
{
    public int Count { get => _pq.Count; }

    private List<(T, S)> _pq;

    public PriorityQueue() {
        _pq = new List<(T, S)>();
    }

    public void Enqueue(T element, S priority)
    {
        _pq.Add((element, priority));

        int current = _pq.Count - 1;

        while (current > 0)
        {
            int parent = (current - 1) / 2;

            if (_pq[current].Item2.CompareTo(_pq[parent].Item2) > 0)
            {
                break;
            }

            (_pq[current], _pq[parent]) = (_pq[parent], _pq[current]);
            current = parent;
        }
    }

    public T Dequeue()
    {
        if (_pq.Count == 0)
        {
            throw new IndexOutOfRangeException();
        }

        (T, S) result = _pq[0];

        _pq[0] = _pq[_pq.Count - 1];
        _pq.RemoveAt(_pq.Count - 1);

        int current = 0;

        while (true)
        {
            int left = current * 2 + 1;
            int right = current * 2 + 2;

            int smallest = current;

            if (left < _pq.Count && _pq[left].Item2.CompareTo(_pq[smallest].Item2) < 0)
            {
                smallest = left;
            }

            if (right < _pq.Count && _pq[right].Item2.CompareTo(_pq[smallest].Item2) < 0)
            {
                smallest = right;
            }

            if (smallest == current)
            {
                break;
            }

            (_pq[current], _pq[smallest]) = (_pq[smallest], _pq[current]);
            current = smallest;
        }

        return result.Item1;
    }
}
