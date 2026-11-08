using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using TradeMcp.Native.Interop;
using TradeMcp.Native.Text;
using TradeMcp.Native.Trading;

namespace TradeMcp.Core.Sessions;

public sealed class TradeSessionManager : IDisposable
{
    private readonly TradeApi _api = new();
    private readonly SessionGate _gate = new();
    private readonly ConcurrentDictionary<int, string> _profiles = new();
    private readonly ConcurrentQueue<Dictionary<string, object?>> _pushEvents = new();
    private ThsApiNative.ThsPushCallback? _pushKeepAlive;

    public TradeApi Api => _api;

    public NativeResult Logon(string profile, string ip, short port, string config, short yybId, string accountNo, string tradeAccount, string jyPassword, string txPassword)
    {
        var r = _api.Logon(ip, port, config, yybId, accountNo, tradeAccount, jyPassword, txPassword);
        if (r.Ok && r.Id is int id)
        {
            _profiles[id] = profile;
            _pushKeepAlive ??= OnPush;
            _api.SetPushCallback(id, _pushKeepAlive);
        }
        return r;
    }

    public NativeResult Logoff(int clientId)
    {
        _api.SetPushCallback(clientId, null);
        _api.Logoff(clientId);
        _profiles.TryRemove(clientId, out _);
        _gate.Remove(clientId);
        return new NativeResult { Ok = true, Id = clientId, Text = "logoff" };
    }

    public string Status(int clientId) => _api.IsConnectOk(clientId);

    public T WithClient<T>(int clientId, Func<TradeApi, T> action) => _gate.Run(clientId, () => action(_api));

    public IReadOnlyList<object> DrainPush(int max = 100)
    {
        var list = new List<object>();
        while (list.Count < max && _pushEvents.TryDequeue(out var item))
            list.Add(item);
        return list;
    }

    private void TrimPushQueue()
    {
        while (_pushEvents.Count > 64)
        {
            if (!_pushEvents.TryDequeue(out var dropped))
                break;
            _ = dropped;
        }
    }

    private void OnPush(int clientId, int functionId, IntPtr tsv, int tsvLen, IntPtr _)
    {
        var text = string.Empty;
        if (tsv != IntPtr.Zero && tsvLen > 0)
        {
            var bytes = new byte[tsvLen + 1];
            Marshal.Copy(tsv, bytes, 0, tsvLen);
            text = Gbk.DecodeZ(bytes);
        }
        _pushEvents.Enqueue(new Dictionary<string, object?>
        {
            ["clientId"] = clientId,
            ["functionId"] = functionId,
            ["text"] = text,
            ["at"] = DateTimeOffset.Now
        });
        TrimPushQueue();
    }

    public void Dispose()
    {
        foreach (var id in _profiles.Keys.ToArray())
        {
            try { Logoff(id); } catch { /* ignore */ }
        }
        try { _api.Close(); } catch { /* ignore */ }
        _gate.Dispose();
    }
}
