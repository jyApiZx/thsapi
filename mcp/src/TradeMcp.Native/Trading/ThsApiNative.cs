using System.Runtime.InteropServices;
using TradeMcp.Native.Interop;
using TradeMcp.Native.Text;

namespace TradeMcp.Native.Trading;

public static class ThsApiNative
{
    public const string Dll = "thsapi.dll";

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern void OpenTdx();

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern void CloseTdx();

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern int Logon(string ip, short port, string config, short yybId, string accountNo, string tradeAccount, string jyPassword, string txPassword, byte[] errInfo);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern void Logoff(int clientId);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern IntPtr IsConnectOK(int clientId);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern int QueryData(int clientId, int category, byte[] result, byte[] errInfo);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern void QueryHistoryData(int clientId, int category, string startDate, string endDate, byte[] result, byte[] errInfo);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern void SendOrderEx(int clientId, int category, int priceType, string gddm, string zqdm, int exchangeId, float price, int quantity, byte[] result, byte[] errInfo);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern void CancelOrderEx(int clientId, string gddm, string exchangeId, string entrustId, byte[] result, byte[] errInfo);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern void GetQuote(int clientId, string zqdm, byte[] result, byte[] errInfo);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern int GetTradableQuantity(int clientId, int category, int priceType, string gddm, string zqdm, float price, byte[] result, byte[] errInfo);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern void GetShareholderCodes(int clientId, byte[] result, byte[] errInfo);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern void QueryBank(int clientId, byte[] result, byte[] errInfo);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern void QueryBankBalance(int clientId, byte[] result, byte[] errInfo);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern void QueryBankFlow(int clientId, string startDate, string endDate, byte[] result, byte[] errInfo);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern void BankTransfer(int clientId, int direction, string bankCode, string bankAccount, string amount, string bankPwd, string fundPwd, string moneyType, byte[] result, byte[] errInfo);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern void BankTransferEx(int clientId, int direction, string bankCode, string bankAccount, string amount, string bankPwd, string fundPwd, string moneyType, string gddm, byte[] result, byte[] errInfo);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern void Repay(int clientId, string amount, byte[] result, byte[] errInfo);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern int OneClickIpo(int clientId, int dryRun, byte[] result, byte[] errInfo);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern void FundOrder(int clientId, int category, string gddm, string zqdm, int exchangeId, string amount, byte[] result, byte[] errInfo);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern void EnableResultColumnOrder(int enable);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern void EnableResultColumnIdSuffix(int enable);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern int SetPushCallback(int clientId, ThsPushCallback? callback, IntPtr userData);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate void ThsPushCallback(int clientId, int functionId, IntPtr tsv, int tsvLen, IntPtr userData);
}

public sealed class TradeApi
{
    private static int _prepared;
    private static int _opened;

    public static void Prepare(string? expectedSha256 = null)
    {
        if (Interlocked.Exchange(ref _prepared, 1) == 1) return;
        var path = DllGuard.ResolveBesideExe(ThsApiNative.Dll);
        DllGuard.VerifySha256(path, expectedSha256);
        DllGuard.PrepareDirectory(path);
    }

    public void Open()
    {
        if (Interlocked.Exchange(ref _opened, 1) == 1) return;
        ThsApiNative.OpenTdx();
        ThsApiNative.EnableResultColumnIdSuffix(1);
    }

    public void Close()
    {
        if (Interlocked.Exchange(ref _opened, 0) == 0) return;
        ThsApiNative.CloseTdx();
    }

    public NativeResult Logon(string ip, short port, string config, short yybId, string accountNo, string tradeAccount, string jyPassword, string txPassword)
    {
        Open();
        var err = NativeBuffers.Err();
        var id = ThsApiNative.Logon(ip, port, config ?? "", yybId, accountNo, tradeAccount ?? accountNo, jyPassword, txPassword ?? "", err);
        var error = Gbk.DecodeZ(err);
        if (id < 0) return NativeResult.Fail(string.IsNullOrEmpty(error) ? "logon failed" : error);
        var status = IsConnectOk(id);
        return new NativeResult { Ok = true, Id = id, Text = string.IsNullOrEmpty(status) ? "ok" : status };
    }

    public void Logoff(int clientId) => ThsApiNative.Logoff(clientId);

    public string IsConnectOk(int clientId)
    {
        var ptr = ThsApiNative.IsConnectOK(clientId);
        if (ptr == IntPtr.Zero) return "";
        // DLL returns GBK; decode explicitly so non-Chinese Windows code pages don't corrupt status.
        var len = 0;
        while (Marshal.ReadByte(ptr, len) != 0 && len < 256) len++;
        var bytes = new byte[len + 1];
        if (len > 0) Marshal.Copy(ptr, bytes, 0, len);
        return Gbk.DecodeZ(bytes);
    }

    public NativeResult QueryData(int clientId, int category)
    {
        var result = NativeBuffers.Result();
        var err = NativeBuffers.Err();
        var rc = ThsApiNative.QueryData(clientId, category, result, err);
        var error = Gbk.DecodeZ(err);
        if (rc != 0 || !string.IsNullOrEmpty(error))
            return NativeResult.Fail(string.IsNullOrEmpty(error) ? $"QueryData failed rc={rc}" : error);
        return NativeResult.Success(Gbk.DecodeZ(result));
    }

    public NativeResult QueryHistory(int clientId, int category, string startDate, string endDate)
    {
        var result = NativeBuffers.Result();
        var err = NativeBuffers.Err();
        ThsApiNative.QueryHistoryData(clientId, category, startDate, endDate, result, err);
        var error = Gbk.DecodeZ(err);
        if (!string.IsNullOrEmpty(error)) return NativeResult.Fail(error);
        return NativeResult.Success(Gbk.DecodeZ(result));
    }

    public NativeResult SendOrderEx(int clientId, int category, int priceType, string gddm, string zqdm, int exchangeId, float price, int quantity)
    {
        var result = NativeBuffers.Result();
        var err = NativeBuffers.Err();
        ThsApiNative.SendOrderEx(clientId, category, priceType, gddm, zqdm, exchangeId, price, quantity, result, err);
        var error = Gbk.DecodeZ(err);
        if (!string.IsNullOrEmpty(error)) return NativeResult.Fail(error);
        return NativeResult.Success(Gbk.DecodeZ(result));
    }

    public NativeResult CancelOrderEx(int clientId, string gddm, string exchangeId, string entrustId)
    {
        var result = NativeBuffers.Result();
        var err = NativeBuffers.Err();
        ThsApiNative.CancelOrderEx(clientId, gddm, exchangeId, entrustId, result, err);
        var error = Gbk.DecodeZ(err);
        if (!string.IsNullOrEmpty(error)) return NativeResult.Fail(error);
        return NativeResult.Success(Gbk.DecodeZ(result));
    }

    public NativeResult GetQuote(int clientId, string zqdm)
    {
        var result = NativeBuffers.Result();
        var err = NativeBuffers.Err();
        ThsApiNative.GetQuote(clientId, zqdm, result, err);
        var error = Gbk.DecodeZ(err);
        if (!string.IsNullOrEmpty(error)) return NativeResult.Fail(error);
        return NativeResult.Success(Gbk.DecodeZ(result));
    }

    public NativeResult GetTradableQuantity(int clientId, int category, int priceType, string gddm, string zqdm, float price)
    {
        var result = NativeBuffers.Result();
        var err = NativeBuffers.Err();
        var qty = ThsApiNative.GetTradableQuantity(clientId, category, priceType, gddm, zqdm, price, result, err);
        var error = Gbk.DecodeZ(err);
        if (qty < 0 || !string.IsNullOrEmpty(error))
            return NativeResult.Fail(string.IsNullOrEmpty(error) ? $"tradable qty failed ({qty})" : error);
        var text = Gbk.DecodeZ(result);
        var nr = NativeResult.Success(text, count: qty);
        return nr;
    }

    public NativeResult GetShareholders(int clientId)
    {
        var result = NativeBuffers.Result();
        var err = NativeBuffers.Err();
        ThsApiNative.GetShareholderCodes(clientId, result, err);
        var error = Gbk.DecodeZ(err);
        if (!string.IsNullOrEmpty(error)) return NativeResult.Fail(error);
        return NativeResult.Success(Gbk.DecodeZ(result));
    }

    public NativeResult QueryBank(int clientId)
    {
        var result = NativeBuffers.Result();
        var err = NativeBuffers.Err();
        ThsApiNative.QueryBank(clientId, result, err);
        var error = Gbk.DecodeZ(err);
        if (!string.IsNullOrEmpty(error)) return NativeResult.Fail(error);
        return NativeResult.Success(Gbk.DecodeZ(result));
    }

    public NativeResult QueryBankBalance(int clientId)
    {
        var result = NativeBuffers.Result();
        var err = NativeBuffers.Err();
        ThsApiNative.QueryBankBalance(clientId, result, err);
        var error = Gbk.DecodeZ(err);
        if (!string.IsNullOrEmpty(error)) return NativeResult.Fail(error);
        return NativeResult.Success(Gbk.DecodeZ(result));
    }

    public NativeResult QueryBankFlow(int clientId, string startDate, string endDate)
    {
        var result = NativeBuffers.Result();
        var err = NativeBuffers.Err();
        ThsApiNative.QueryBankFlow(clientId, startDate ?? "", endDate ?? "", result, err);
        var error = Gbk.DecodeZ(err);
        if (!string.IsNullOrEmpty(error)) return NativeResult.Fail(error);
        return NativeResult.Success(Gbk.DecodeZ(result));
    }

    public NativeResult BankTransfer(int clientId, int direction, string bankCode, string bankAccount, string amount, string bankPwd, string fundPwd, string moneyType, string? gddm = null)
    {
        var result = NativeBuffers.Result();
        var err = NativeBuffers.Err();
        if (string.IsNullOrEmpty(gddm))
            ThsApiNative.BankTransfer(clientId, direction, bankCode, bankAccount, amount, bankPwd ?? "", fundPwd ?? "", moneyType ?? "R", result, err);
        else
            ThsApiNative.BankTransferEx(clientId, direction, bankCode, bankAccount, amount, bankPwd ?? "", fundPwd ?? "", moneyType ?? "R", gddm, result, err);
        var error = Gbk.DecodeZ(err);
        if (!string.IsNullOrEmpty(error)) return NativeResult.Fail(error);
        return NativeResult.Success(Gbk.DecodeZ(result));
    }

    public NativeResult Repay(int clientId, string amount)
    {
        var result = NativeBuffers.Result();
        var err = NativeBuffers.Err();
        ThsApiNative.Repay(clientId, amount, result, err);
        var error = Gbk.DecodeZ(err);
        if (!string.IsNullOrEmpty(error)) return NativeResult.Fail(error);
        return NativeResult.Success(Gbk.DecodeZ(result));
    }

    public NativeResult OneClickIpo(int clientId, bool dryRun)
    {
        var result = NativeBuffers.Result();
        var err = NativeBuffers.Err();
        var n = ThsApiNative.OneClickIpo(clientId, dryRun ? 1 : 0, result, err);
        var error = Gbk.DecodeZ(err);
        if (n < 0 || !string.IsNullOrEmpty(error))
            return NativeResult.Fail(string.IsNullOrEmpty(error) ? "ipo failed" : error);
        return NativeResult.Success(Gbk.DecodeZ(result), count: n);
    }

    public NativeResult FundOrder(int clientId, int category, string gddm, string zqdm, int exchangeId, string amount)
    {
        var result = NativeBuffers.Result();
        var err = NativeBuffers.Err();
        ThsApiNative.FundOrder(clientId, category, gddm, zqdm, exchangeId, amount, result, err);
        var error = Gbk.DecodeZ(err);
        if (!string.IsNullOrEmpty(error)) return NativeResult.Fail(error);
        return NativeResult.Success(Gbk.DecodeZ(result));
    }

    public int SetPushCallback(int clientId, ThsApiNative.ThsPushCallback? callback)
        => ThsApiNative.SetPushCallback(clientId, callback, IntPtr.Zero);
}
