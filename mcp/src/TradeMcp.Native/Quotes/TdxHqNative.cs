using System.Runtime.InteropServices;
using TradeMcp.Native.Interop;
using TradeMcp.Native.Text;

namespace TradeMcp.Native.Quotes;

internal static class TdxHqNative
{
    public const string Dll = "HqApi.dll";

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern int TdxHq_Connect(byte[] ip, ushort port, byte[] account, byte[] password, byte[] result, byte[] err);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern void TdxHq_Disconnect(int id);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern bool TdxHq_GetSecurityCount(int id, byte market, ref ushort result, byte[] err);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern bool TdxHq_GetSecurityList(int id, byte market, ushort start, ref ushort count, byte[] result, byte[] err);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern bool TdxHq_GetSecurityBars(int id, byte category, byte market, byte[] code, ushort start, ref ushort count, byte[] result, byte[] err);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern bool TdxHq_GetIndexBars(int id, byte category, byte market, byte[] code, ushort start, ref ushort count, byte[] result, byte[] err);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern bool TdxHq_GetMinuteTimeData(int id, byte market, byte[] code, byte[] result, byte[] err);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern bool TdxHq_GetHistoryMinuteTimeData(int id, byte market, byte[] code, int date, byte[] result, byte[] err);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern bool TdxHq_GetTransactionData(int id, byte market, byte[] code, ushort start, ref ushort count, byte[] result, byte[] err);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern bool TdxHq_GetHistoryTransactionData(int id, byte market, byte[] code, ushort start, ref ushort count, int date, byte[] result, byte[] err);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern bool TdxHq_GetSecurityQuotes(int id, byte[] market, IntPtr[] codes, ref ushort count, byte[] result, byte[] err);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern bool TdxHq_GetSecurityQuotes10(int id, byte[] market, IntPtr[] codes, ref ushort count, byte[] result, byte[] err);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern bool TdxHq_GetXDXRInfo(int id, byte[] market, IntPtr[] codes, int count, byte[] result, byte[] err);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern bool TdxHq_GetFinanceInfo(int id, byte[] market, IntPtr[] codes, int count, byte[] result, byte[] err);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern bool TdxHq_GetCallAuctionData(int id, byte market, byte[] code, int start, ref int count, byte[] result, byte[] err);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern bool TdxHq_GetBuySellQueue(int id, byte[] market, IntPtr[] codes, int count, byte[] result, byte[] err);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern bool TdxHq_GetDetailTransactionData(int id, byte market, byte[] code, int start, ref ushort count, byte[] result, byte[] err);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern bool TdxHq_GetDetailOriginalTransactionData(int id, byte market, byte[] code, int start, ref ushort count, byte[] result, byte[] err);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern bool TdxHq_GetDetailOrderData(int id, byte market, byte[] code, int start, ref ushort count, byte[] result, byte[] err);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern bool TdxHq_ListBoards(int id, byte[] boardType, byte[] result, byte[] err);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern bool TdxHq_ListBoardMembers(int id, byte[] boardName, byte[] blockFile, byte[] result, byte[] err);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern bool TdxHq_GetMarketStat(int id, byte[] result, byte[] err);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern bool TdxHq_GetFundFlow(int id, byte market, byte[] code, byte[] result, byte[] err);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern bool TdxHq_GetHistoryFundFlow(int id, byte market, byte[] code, ushort start, ushort count, byte[] result, byte[] err);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern bool TdxHq_GetCompanyInfoCategory(int id, byte market, byte[] code, byte[] result, byte[] err);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern int TdxExHq_Connect(byte[] ip, int port, byte[] account, byte[] password, byte[] result, byte[] err);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern void TdxExHq_Disconnect(int id);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern bool TdxExHq_GetMarkets(int id, byte[] result, byte[] err);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern bool TdxExHq_GetInstrumentCount(int id, ref int result, byte[] err);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern bool TdxExHq_GetInstrumentInfo(int id, int start, short count, byte[] result, byte[] err);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern bool TdxExHq_GetInstrumentBars(int id, byte category, byte market, byte[] code, int start, ref short count, byte[] result, byte[] err);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern bool TdxExHq_GetMinuteTimeData(int id, byte market, byte[] code, byte[] result, byte[] err);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern bool TdxExHq_GetHistoryMinuteTimeData(int id, byte market, byte[] code, int date, byte[] result, byte[] err);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern bool TdxExHq_GetTransactionData(int id, byte market, byte[] code, int start, ref short count, byte[] result, byte[] err);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern bool TdxExHq_GetHistoryTransactionData(int id, byte market, byte[] code, int date, int start, ref short count, byte[] result, byte[] err);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern bool TdxExHq_GetInstrumentQuote(int id, byte market, byte[] code, byte[] result, byte[] err);
}

public sealed class QuoteApi
{
    private static int _prepared;

    public static void Prepare(string? expectedSha256 = null)
    {
        if (Interlocked.Exchange(ref _prepared, 1) == 1) return;
        var path = DllGuard.ResolveBesideExe(TdxHqNative.Dll);
        DllGuard.VerifySha256(path, expectedSha256);
        DllGuard.PrepareDirectory(path);
    }

    public static string AccountForLane(string lane, string user)
    {
        var u = (user ?? "").Trim();
        if (u.StartsWith("L1*", StringComparison.OrdinalIgnoreCase)) u = u[3..];
        if (u.StartsWith("L2*", StringComparison.OrdinalIgnoreCase)) u = u[3..];
        if (u.StartsWith("KZ*", StringComparison.OrdinalIgnoreCase)) u = u[3..];
        u = u.Trim();
        return lane.ToUpperInvariant() switch
        {
            "L1" => "L1*" + u,
            "EX" => "KZ*" + u,
            _ => u
        };
    }

    public NativeResult Connect(string ip, int port, string account, string password)
    {
        var result = NativeBuffers.Result();
        var err = NativeBuffers.Err();
        var id = TdxHqNative.TdxHq_Connect(Gbk.EncodeZ(ip), (ushort)port, Gbk.EncodeZ(account), Gbk.EncodeZ(password), result, err);
        var error = Gbk.DecodeZ(err);
        if (id < 0) return NativeResult.Fail(string.IsNullOrEmpty(error) ? "connect failed" : error);
        return NativeResult.Success(Gbk.DecodeZ(result), id);
    }

    public void Disconnect(int id) => TdxHqNative.TdxHq_Disconnect(id);

    public NativeResult ConnectEx(string ip, int port, string account, string password)
    {
        var result = NativeBuffers.Result();
        var err = NativeBuffers.Err();
        var id = TdxHqNative.TdxExHq_Connect(Gbk.EncodeZ(ip), port, Gbk.EncodeZ(account), Gbk.EncodeZ(password), result, err);
        var error = Gbk.DecodeZ(err);
        if (id < 0) return NativeResult.Fail(string.IsNullOrEmpty(error) ? "ex connect failed" : error);
        return NativeResult.Success(Gbk.DecodeZ(result), id);
    }

    public void DisconnectEx(int id) => TdxHqNative.TdxExHq_Disconnect(id);

    public NativeResult SecurityCount(int id, byte market)
    {
        ushort n = 0;
        var err = NativeBuffers.Err();
        var ok = TdxHqNative.TdxHq_GetSecurityCount(id, market, ref n, err);
        return ok ? NativeResult.Success($"数量\n{n}", count: n) : NativeResult.Fail(Gbk.DecodeZ(err));
    }

    public NativeResult SecurityList(int id, byte market, ushort start, ushort count)
    {
        var result = NativeBuffers.Result();
        var err = NativeBuffers.Err();
        var n = count == 0 ? (ushort)1000 : count;
        var ok = TdxHqNative.TdxHq_GetSecurityList(id, market, start, ref n, result, err);
        return ok ? NativeResult.Success(Gbk.DecodeZ(result), count: n) : NativeResult.Fail(Gbk.DecodeZ(err));
    }

    public NativeResult Bars(int id, bool index, byte category, byte market, string code, ushort start, ushort count)
    {
        var result = NativeBuffers.Result();
        var err = NativeBuffers.Err();
        var n = count == 0 ? (ushort)30 : count;
        var z = Gbk.EncodeZ(code);
        var ok = index
            ? TdxHqNative.TdxHq_GetIndexBars(id, category, market, z, start, ref n, result, err)
            : TdxHqNative.TdxHq_GetSecurityBars(id, category, market, z, start, ref n, result, err);
        return ok ? NativeResult.Success(Gbk.DecodeZ(result), count: n) : NativeResult.Fail(Gbk.DecodeZ(err));
    }

    public NativeResult Minute(int id, byte market, string code)
    {
        var result = NativeBuffers.Result();
        var err = NativeBuffers.Err();
        var ok = TdxHqNative.TdxHq_GetMinuteTimeData(id, market, Gbk.EncodeZ(code), result, err);
        return ok ? NativeResult.Success(Gbk.DecodeZ(result)) : NativeResult.Fail(Gbk.DecodeZ(err));
    }

    public NativeResult HistoryMinute(int id, byte market, string code, int date)
    {
        var result = NativeBuffers.Result();
        var err = NativeBuffers.Err();
        var ok = TdxHqNative.TdxHq_GetHistoryMinuteTimeData(id, market, Gbk.EncodeZ(code), date, result, err);
        return ok ? NativeResult.Success(Gbk.DecodeZ(result)) : NativeResult.Fail(Gbk.DecodeZ(err));
    }

    public NativeResult Transactions(int id, byte market, string code, ushort start, ushort count)
    {
        var result = NativeBuffers.Result();
        var err = NativeBuffers.Err();
        var n = count == 0 ? (ushort)30 : count;
        var ok = TdxHqNative.TdxHq_GetTransactionData(id, market, Gbk.EncodeZ(code), start, ref n, result, err);
        return ok ? NativeResult.Success(Gbk.DecodeZ(result), count: n) : NativeResult.Fail(Gbk.DecodeZ(err));
    }

    public NativeResult HistoryTransactions(int id, byte market, string code, ushort start, ushort count, int date)
    {
        var result = NativeBuffers.Result();
        var err = NativeBuffers.Err();
        var n = count == 0 ? (ushort)30 : count;
        var ok = TdxHqNative.TdxHq_GetHistoryTransactionData(id, market, Gbk.EncodeZ(code), start, ref n, date, result, err);
        return ok ? NativeResult.Success(Gbk.DecodeZ(result), count: n) : NativeResult.Fail(Gbk.DecodeZ(err));
    }

    public NativeResult Quotes(int id, bool ten, IReadOnlyList<(byte Market, string Code)> items)
    {
        if (items.Count == 0) return NativeResult.Fail("codes required");
        var mk = new byte[items.Count];
        var ptrs = new IntPtr[items.Count];
        try
        {
            for (var i = 0; i < items.Count; i++)
            {
                mk[i] = items[i].Market;
                var b = Gbk.EncodeZ(items[i].Code);
                ptrs[i] = Marshal.AllocHGlobal(b.Length);
                Marshal.Copy(b, 0, ptrs[i], b.Length);
            }
            var result = NativeBuffers.Result();
            var err = NativeBuffers.Err();
            var n = (ushort)items.Count;
            var ok = ten
                ? TdxHqNative.TdxHq_GetSecurityQuotes10(id, mk, ptrs, ref n, result, err)
                : TdxHqNative.TdxHq_GetSecurityQuotes(id, mk, ptrs, ref n, result, err);
            return ok ? NativeResult.Success(Gbk.DecodeZ(result), count: n) : NativeResult.Fail(Gbk.DecodeZ(err));
        }
        finally
        {
            foreach (var p in ptrs)
                if (p != IntPtr.Zero) Marshal.FreeHGlobal(p);
        }
    }

    public NativeResult OneCodeTable(int id, byte market, string code, Func<int, byte[], IntPtr[], int, byte[], byte[], bool> call)
    {
        var ptrs = new IntPtr[1];
        try
        {
            var b = Gbk.EncodeZ(code);
            ptrs[0] = Marshal.AllocHGlobal(b.Length);
            Marshal.Copy(b, 0, ptrs[0], b.Length);
            var result = NativeBuffers.Result();
            var err = NativeBuffers.Err();
            var ok = call(id, [market], ptrs, 1, result, err);
            return ok ? NativeResult.Success(Gbk.DecodeZ(result)) : NativeResult.Fail(Gbk.DecodeZ(err));
        }
        finally
        {
            if (ptrs[0] != IntPtr.Zero) Marshal.FreeHGlobal(ptrs[0]);
        }
    }

    public NativeResult Xdxr(int id, byte market, string code) =>
        OneCodeTable(id, market, code, (cid, mk, ptrs, n, r, e) => TdxHqNative.TdxHq_GetXDXRInfo(cid, mk, ptrs, n, r, e));

    public NativeResult Finance(int id, byte market, string code) =>
        OneCodeTable(id, market, code, (cid, mk, ptrs, n, r, e) => TdxHqNative.TdxHq_GetFinanceInfo(cid, mk, ptrs, n, r, e));

    public NativeResult Queue(int id, byte market, string code) =>
        OneCodeTable(id, market, code, (cid, mk, ptrs, n, r, e) => TdxHqNative.TdxHq_GetBuySellQueue(cid, mk, ptrs, n, r, e));

    public NativeResult Auction(int id, byte market, string code, int count)
    {
        var result = NativeBuffers.Result();
        var err = NativeBuffers.Err();
        var n = count <= 0 ? 30 : count;
        var ok = TdxHqNative.TdxHq_GetCallAuctionData(id, market, Gbk.EncodeZ(code), 0, ref n, result, err);
        return ok ? NativeResult.Success(Gbk.DecodeZ(result), count: n) : NativeResult.Fail(Gbk.DecodeZ(err));
    }

    public NativeResult Detail(int id, byte market, string code, ushort count, int kind)
    {
        var result = NativeBuffers.Result();
        var err = NativeBuffers.Err();
        var n = count == 0 ? (ushort)30 : count;
        var z = Gbk.EncodeZ(code);
        var ok = kind switch
        {
            1 => TdxHqNative.TdxHq_GetDetailOriginalTransactionData(id, market, z, 0, ref n, result, err),
            2 => TdxHqNative.TdxHq_GetDetailOrderData(id, market, z, 0, ref n, result, err),
            _ => TdxHqNative.TdxHq_GetDetailTransactionData(id, market, z, 0, ref n, result, err)
        };
        return ok ? NativeResult.Success(Gbk.DecodeZ(result), count: n) : NativeResult.Fail(Gbk.DecodeZ(err));
    }

    public NativeResult Boards(int id, string boardType)
    {
        var result = NativeBuffers.Result();
        var err = NativeBuffers.Err();
        var ok = TdxHqNative.TdxHq_ListBoards(id, Gbk.EncodeZ(boardType), result, err);
        return ok ? NativeResult.Success(Gbk.DecodeZ(result)) : NativeResult.Fail(Gbk.DecodeZ(err));
    }

    public NativeResult Members(int id, string name, string file)
    {
        var result = NativeBuffers.Result();
        var err = NativeBuffers.Err();
        var ok = TdxHqNative.TdxHq_ListBoardMembers(id, Gbk.EncodeZ(name), Gbk.EncodeZ(file), result, err);
        return ok ? NativeResult.Success(Gbk.DecodeZ(result)) : NativeResult.Fail(Gbk.DecodeZ(err));
    }

    public NativeResult MarketStat(int id)
    {
        var result = NativeBuffers.Result();
        var err = NativeBuffers.Err();
        var ok = TdxHqNative.TdxHq_GetMarketStat(id, result, err);
        return ok ? NativeResult.Success(Gbk.DecodeZ(result)) : NativeResult.Fail(Gbk.DecodeZ(err));
    }

    public NativeResult FundFlow(int id, byte market, string code)
    {
        var result = NativeBuffers.Result();
        var err = NativeBuffers.Err();
        var ok = TdxHqNative.TdxHq_GetFundFlow(id, market, Gbk.EncodeZ(code), result, err);
        return ok ? NativeResult.Success(Gbk.DecodeZ(result)) : NativeResult.Fail(Gbk.DecodeZ(err));
    }

    public NativeResult HistoryFundFlow(int id, byte market, string code, ushort start, ushort count)
    {
        var result = NativeBuffers.Result();
        var err = NativeBuffers.Err();
        var ok = TdxHqNative.TdxHq_GetHistoryFundFlow(id, market, Gbk.EncodeZ(code), start, count == 0 ? (ushort)10 : count, result, err);
        return ok ? NativeResult.Success(Gbk.DecodeZ(result)) : NativeResult.Fail(Gbk.DecodeZ(err));
    }

    public NativeResult CompanyCategory(int id, byte market, string code)
    {
        var result = NativeBuffers.Result();
        var err = NativeBuffers.Err();
        var ok = TdxHqNative.TdxHq_GetCompanyInfoCategory(id, market, Gbk.EncodeZ(code), result, err);
        return ok ? NativeResult.Success(Gbk.DecodeZ(result)) : NativeResult.Fail(Gbk.DecodeZ(err));
    }

    public NativeResult ExMarkets(int id)
    {
        var result = NativeBuffers.Result();
        var err = NativeBuffers.Err();
        var ok = TdxHqNative.TdxExHq_GetMarkets(id, result, err);
        return ok ? NativeResult.Success(Gbk.DecodeUtf8Z(result)) : NativeResult.Fail(Gbk.DecodeZ(err));
    }

    public NativeResult ExCount(int id)
    {
        var n = 0;
        var err = NativeBuffers.Err();
        var ok = TdxHqNative.TdxExHq_GetInstrumentCount(id, ref n, err);
        return ok ? NativeResult.Success($"数量\n{n}", count: n) : NativeResult.Fail(Gbk.DecodeZ(err));
    }

    public NativeResult ExInfo(int id, int start, short count)
    {
        var result = NativeBuffers.Result();
        var err = NativeBuffers.Err();
        var n = count <= 0 ? (short)40 : count;
        var ok = TdxHqNative.TdxExHq_GetInstrumentInfo(id, start, n, result, err);
        return ok ? NativeResult.Success(Gbk.DecodeUtf8Z(result)) : NativeResult.Fail(Gbk.DecodeZ(err));
    }

    public NativeResult ExBars(int id, byte category, byte market, string code, int start, short count)
    {
        var result = NativeBuffers.Result();
        var err = NativeBuffers.Err();
        var n = count <= 0 ? (short)30 : count;
        var ok = TdxHqNative.TdxExHq_GetInstrumentBars(id, category, market, Gbk.EncodeZ(code), start, ref n, result, err);
        return ok ? NativeResult.Success(Gbk.DecodeUtf8Z(result), count: n) : NativeResult.Fail(Gbk.DecodeZ(err));
    }

    public NativeResult ExMinute(int id, byte market, string code)
    {
        var result = NativeBuffers.Result();
        var err = NativeBuffers.Err();
        var ok = TdxHqNative.TdxExHq_GetMinuteTimeData(id, market, Gbk.EncodeZ(code), result, err);
        return ok ? NativeResult.Success(Gbk.DecodeUtf8Z(result)) : NativeResult.Fail(Gbk.DecodeZ(err));
    }

    public NativeResult ExHistoryMinute(int id, byte market, string code, int date)
    {
        var result = NativeBuffers.Result();
        var err = NativeBuffers.Err();
        var ok = TdxHqNative.TdxExHq_GetHistoryMinuteTimeData(id, market, Gbk.EncodeZ(code), date, result, err);
        return ok ? NativeResult.Success(Gbk.DecodeUtf8Z(result)) : NativeResult.Fail(Gbk.DecodeZ(err));
    }

    public NativeResult ExTransactions(int id, byte market, string code, int start, short count)
    {
        var result = NativeBuffers.Result();
        var err = NativeBuffers.Err();
        var n = count <= 0 ? (short)30 : count;
        var ok = TdxHqNative.TdxExHq_GetTransactionData(id, market, Gbk.EncodeZ(code), start, ref n, result, err);
        return ok ? NativeResult.Success(Gbk.DecodeUtf8Z(result), count: n) : NativeResult.Fail(Gbk.DecodeZ(err));
    }

    public NativeResult ExHistoryTransactions(int id, byte market, string code, int date, int start, short count)
    {
        var result = NativeBuffers.Result();
        var err = NativeBuffers.Err();
        var n = count <= 0 ? (short)30 : count;
        var ok = TdxHqNative.TdxExHq_GetHistoryTransactionData(id, market, Gbk.EncodeZ(code), date, start, ref n, result, err);
        return ok ? NativeResult.Success(Gbk.DecodeUtf8Z(result), count: n) : NativeResult.Fail(Gbk.DecodeZ(err));
    }

    public NativeResult ExQuote(int id, byte market, string code)
    {
        var result = NativeBuffers.Result();
        var err = NativeBuffers.Err();
        var ok = TdxHqNative.TdxExHq_GetInstrumentQuote(id, market, Gbk.EncodeZ(code), result, err);
        return ok ? NativeResult.Success(Gbk.DecodeUtf8Z(result)) : NativeResult.Fail(Gbk.DecodeZ(err));
    }
}
