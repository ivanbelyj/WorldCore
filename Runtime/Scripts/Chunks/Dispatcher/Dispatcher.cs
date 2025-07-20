using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Dispatcher : MonoBehaviour, IDispatcher
{
    private readonly Queue<Action> queue = new Queue<Action>();
    private readonly object _lock = new object();

    private void Enqueue(Action action) {
        lock(_lock) {
            queue.Enqueue(action);
        }
    }

    public Task Execute(Action action)
    {
        var tcs = new TaskCompletionSource<object>();
        Enqueue(() =>
        {
            try
            {
                action();
                tcs.SetResult(null);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        return tcs.Task;
    }

    private void Update()
    {
        while (true)
        {
            Action action;
            lock (_lock)
            {
                if (queue.Count == 0)
                    return;
                action = queue.Dequeue();
            }
            action();
        }
    }

    public Task<T> Execute<T>(Func<T> func)
    {
        var tcs = new TaskCompletionSource<T>();
        Enqueue(() =>
        {
            try
            {
                T result = func();
                tcs.SetResult(result);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        return tcs.Task;
    }
}
