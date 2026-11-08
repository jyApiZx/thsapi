// 2.0 交易示例（C#）
// 1) 运行 xdDemo.exe 登录自己的券商或模拟盘一次
// 2) 与 thsapi.dll、login_prefs.json 同目录编译运行（位数一致，优先 x64）
// 3) Alloc 结果 UTF-8，用完 FreeApiBuffer
// 编译示例：csc /platform:x64 01_quick_query.cs

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

internal static class ThsApi
{
    public const string Dll = "thsapi.dll";

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern void OpenTdx();

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern void CloseTdx();

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern int Logon(
        string ip, short port, string config, short yybId,
        string accountNo, string tradeAccount,
        string jyPassword, string txPassword, StringBuilder errInfo);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern void Logoff(int clientId);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern int QueryDataAlloc(int clientId, int category, out IntPtr result, out IntPtr errInfo);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern void FreeApiBuffer(IntPtr p);
}

internal static class Program
{
    static string JsonStr(string obj, string key)
    {
        var m = Regex.Match(obj, "\"" + Regex.Escape(key) + "\"\\s*:\\s*\"((?:\\\\.|[^\"\\\\])*)\"");
        return m.Success ? Regex.Unescape(m.Groups[1].Value) : "";
    }

    static int JsonInt(string obj, string key, int defv = 0)
    {
        var m = Regex.Match(obj, "\"" + Regex.Escape(key) + "\"\\s*:\\s*(-?\\d+)");
        return m.Success ? int.Parse(m.Groups[1].Value) : defv;
    }

    static bool JsonBool(string obj, string key)
    {
        var m = Regex.Match(obj, "\"" + Regex.Escape(key) + "\"\\s*:\\s*\"?(true|1)", RegexOptions.IgnoreCase);
        return m.Success;
    }

    static string JsonObjRaw(string obj, string key)
    {
        var m = Regex.Match(obj, "\"" + Regex.Escape(key) + "\"\\s*:\\s*\\{");
        if (!m.Success) return "";
        int start = m.Index + m.Length - 1;
        int depth = 0;
        for (int i = start; i < obj.Length; i++)
        {
            if (obj[i] == '{') depth++;
            else if (obj[i] == '}')
            {
                depth--;
                if (depth == 0) return obj.Substring(start, i - start + 1);
            }
        }
        return "";
    }

    static string PickAccount(string json)
    {
        string last = JsonStr(json, "lastAccount");
        int accounts = json.IndexOf("\"accounts\"", StringComparison.Ordinal);
        if (accounts < 0) return "";
        if (!string.IsNullOrEmpty(last))
        {
            string key = "\"" + last + "\"";
            int k = json.IndexOf(key, accounts, StringComparison.Ordinal);
            if (k >= 0)
            {
                int brace = json.IndexOf('{', k + key.Length);
                if (brace >= 0)
                {
                    int depth = 0;
                    for (int i = brace; i < json.Length; i++)
                    {
                        if (json[i] == '{') depth++;
                        else if (json[i] == '}')
                        {
                            depth--;
                            if (depth == 0) return json.Substring(brace, i - brace + 1);
                        }
                    }
                }
            }
        }
        int first = json.IndexOf('{', accounts + 10);
        if (first < 0) return "";
        int d = 0;
        for (int i = first; i < json.Length; i++)
        {
            if (json[i] == '{') d++;
            else if (json[i] == '}')
            {
                d--;
                if (d == 0) return json.Substring(first, i - first + 1);
            }
        }
        return "";
    }

    static string PtrUtf8(IntPtr p)
    {
        if (p == IntPtr.Zero) return "";
        int len = 0;
        while (Marshal.ReadByte(p, len) != 0) len++;
        var bytes = new byte[len];
        Marshal.Copy(p, bytes, 0, len);
        return Encoding.UTF8.GetString(bytes);
    }

    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        if (!File.Exists("login_prefs.json"))
        {
            Console.Error.WriteLine("找不到 login_prefs.json。请先运行 xdDemo.exe 登录一次。");
            return;
        }
        string json = File.ReadAllText("login_prefs.json", Encoding.UTF8);
        string acc = PickAccount(json);
        string ip = JsonStr(acc, "serverIp");
        short port = (short)JsonInt(acc, "serverPort");
        short yyb = (short)JsonInt(acc, "yybId");
        string account = JsonStr(acc, "account");
        string trade = JsonStr(acc, "tradeAccount");
        string jy = JsonStr(acc, "jyPassword");
        string tx = JsonStr(acc, "txPassword");
        string config = JsonObjRaw(acc, "config");
        if (string.IsNullOrEmpty(config)) config = JsonStr(acc, "config");
        if (string.IsNullOrEmpty(config))
        {
            string qs = JsonStr(acc, "apiQsid");
            if (qs == "") qs = JsonInt(acc, "apiQsid").ToString();
            if (qs == "0") qs = JsonInt(acc, "websiteQsid").ToString();
            string at = JsonStr(acc, "accountTypeCode");
            if (at == "") at = "k";
            config = "{\"qs_id\":" + qs + ",\"account_type\":\"" + at + "\"}";
        }
        // 信用账户：登录账号前加 X；未单独保存交易账号时同样加 X
        bool credit = JsonBool(acc, "isCredit");
        bool tradeGiven = !string.IsNullOrEmpty(trade);
        if (!tradeGiven) trade = account;
        if (credit && !account.StartsWith("X", StringComparison.OrdinalIgnoreCase)) account = "X" + account;
        if (credit && !tradeGiven && !trade.StartsWith("X", StringComparison.OrdinalIgnoreCase)) trade = "X" + trade;

        ThsApi.OpenTdx();
        var err = new StringBuilder(1024);
        int cid = ThsApi.Logon(ip, port, config, yyb, account, trade, jy, tx, err);
        if (cid < 0)
        {
            Console.Error.WriteLine("登录失败: " + err);
            ThsApi.CloseTdx();
            return;
        }
        Console.WriteLine("登录成功 ClientID=" + cid);

        IntPtr result, errInfo;
        ThsApi.QueryDataAlloc(cid, 0, out result, out errInfo); // 0=资金
        string e = PtrUtf8(errInfo);
        string r = PtrUtf8(result);
        if (!string.IsNullOrEmpty(e)) Console.Error.WriteLine("查询失败: " + e);
        else Console.WriteLine("资金表:\n" + r);
        if (result != IntPtr.Zero) ThsApi.FreeApiBuffer(result);
        if (errInfo != IntPtr.Zero) ThsApi.FreeApiBuffer(errInfo);

        ThsApi.Logoff(cid);
        ThsApi.CloseTdx();
    }
}
