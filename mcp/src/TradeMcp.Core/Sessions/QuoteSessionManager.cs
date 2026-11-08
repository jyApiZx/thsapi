using TradeMcp.Core.Quotes;
using TradeMcp.Native.Interop;
using TradeMcp.Native.Quotes;

namespace TradeMcp.Core.Sessions;

public sealed class QuoteSessionManager : IDisposable
{
    private readonly QuoteApi _api = new();
    private readonly SessionGate _hq = new();
    private readonly SessionGate _ex = new();
    private readonly HashSet<int> _hqIds = new();
    private readonly HashSet<int> _exIds = new();
    private readonly object _sync = new();

    public QuoteApi Api => _api;

    public NativeResult Connect(string lane, string ip, int port, string? account, string? password)
    {
        var (user, pwd, _) = QuoteCredentials.Resolve(lane, account, password);
        var acct = QuoteApi.AccountForLane(lane, user);
        if (lane.Equals("Ex", StringComparison.OrdinalIgnoreCase))
        {
            var r = _api.ConnectEx(ip, port, acct, pwd);
            if (r.Ok && r.Id is int id)
            {
                lock (_sync) _exIds.Add(id);
            }
            return r;
        }
        else
        {
            var r = _api.Connect(ip, port, acct, pwd);
            if (r.Ok && r.Id is int id)
            {
                lock (_sync) _hqIds.Add(id);
            }
            return r;
        }
    }

    public NativeResult Disconnect(string lane, int id)
    {
        if (lane.Equals("Ex", StringComparison.OrdinalIgnoreCase))
        {
            _api.DisconnectEx(id);
            lock (_sync) _exIds.Remove(id);
            _ex.Remove(id);
        }
        else
        {
            _api.Disconnect(id);
            lock (_sync) _hqIds.Remove(id);
            _hq.Remove(id);
        }
        return new NativeResult { Ok = true, Id = id, Text = "disconnected" };
    }

    public T WithHq<T>(int id, Func<QuoteApi, T> action) => _hq.Run(id, () => action(_api));
    public T WithEx<T>(int id, Func<QuoteApi, T> action) => _ex.Run(id, () => action(_api));

    public void Dispose()
    {
        lock (_sync)
        {
            foreach (var id in _hqIds.ToArray())
            {
                try { _api.Disconnect(id); } catch { /* ignore */ }
                _hq.Remove(id);
            }
            foreach (var id in _exIds.ToArray())
            {
                try { _api.DisconnectEx(id); } catch { /* ignore */ }
                _ex.Remove(id);
            }
            _hqIds.Clear();
            _exIds.Clear();
        }
        _hq.Dispose();
        _ex.Dispose();
    }
}
