using System.Collections.Generic;
using PixelMacroEngine.Core.Models;

namespace PixelMacroEngine.Core.Input;

public class SimpleTaskQueue
{
    private readonly object _lock = new();
    private readonly Queue<InputBatchCmd> _queue = new();

    public void Enqueue(InputBatchCmd batch)
    {
        lock (_lock)
        {
            _queue.Enqueue(batch);
        }
    }

    public InputBatchCmd? Dequeue()
    {
        lock (_lock)
        {
            return _queue.Count > 0 ? _queue.Dequeue() : null;
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _queue.Clear();
        }
    }

    public int Count
    {
        get { lock (_lock) return _queue.Count; }
    }
}