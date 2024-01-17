using System;
using System.Collections.Generic;
using System.Linq;

class PriorityQueue<TPriority, TValue>
{
    private SortedDictionary<TPriority, Queue<TValue>> priority_queue;
    public PriorityQueue()
    {
        priority_queue = new SortedDictionary<TPriority, Queue<TValue>>();

            }

    public void Enqueue(TPriority priority, TValue value)
    {
        if (!priority_queue.ContainsKey(priority))
        {
            priority_queue[priority] = new Queue<TValue>();
        }

        priority_queue[priority].Enqueue(value);
    }

    public TValue Dequeue()
    {
        if (priority_queue.Count == 0)
        {
            throw new InvalidOperationException("Priority queue is empty");
        }
        var firstQueue = priority_queue.First();
        var value = firstQueue.Value.Dequeue();

        if (firstQueue.Value.Count == 0)
        {
            priority_queue.Remove(firstQueue.Key);
        }
        return value;
    }

    public TValue Front()
    {
        if (priority_queue.Count == 0)
        {
            throw new InvalidOperationException("Priority queue is empty");
        }
        var firstQueue = priority_queue.First();
        var value = firstQueue.Value.First();
        return value;
    }

    public bool IsEmpty()
    {
        return priority_queue.Count == 0;
    }

}
