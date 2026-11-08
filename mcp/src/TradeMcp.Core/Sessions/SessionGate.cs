namespace TradeMcp.Core.Sessions;

public sealed class SessionGate : IDisposable
{
    private readonly Dictionary<int, SemaphoreSlim> _locks = new();
    private readonly object _sync = new();
    private bool _disposed;

    public async Task<T> RunAsync<T>(int id, Func<Task<T>> action, CancellationToken ct = default)
    {
        var gate = Get(id);
        await gate.WaitAsync(ct).ConfigureAwait(false);
        try { return await action().ConfigureAwait(false); }
        finally { gate.Release(); }
    }

    public T Run<T>(int id, Func<T> action)
    {
        var gate = Get(id);
        gate.Wait();
        try { return action(); }
        finally { gate.Release(); }
    }

    public void Remove(int id)
    {
        lock (_sync)
        {
            if (_locks.Remove(id, out var sem))
                sem.Dispose();
        }
    }

    private SemaphoreSlim Get(int id)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        lock (_sync)
        {
            if (!_locks.TryGetValue(id, out var sem))
            {
                sem = new SemaphoreSlim(1, 1);
                _locks[id] = sem;
            }
            return sem;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        lock (_sync)
        {
            foreach (var s in _locks.Values) s.Dispose();
            _locks.Clear();
        }
    }
}
