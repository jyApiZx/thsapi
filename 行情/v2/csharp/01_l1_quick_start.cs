// 2.0 行情示例（C#）
// 1) 运行「行情测试demo.exe」用 L1 连接一次
// 2) 与 HqApi.dll、login_prefs.json 同目录编译运行
// csc /platform:x64 01_l1_quick_start.cs

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

internal static class HqApi
{
    public const string Dll = "HqApi.dll";

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern int TdxHq_ConnectAlloc(
        string ip, ushort port, string account, string password,
        out IntPtr result, out IntPtr errInfo);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern void TdxHq_Disconnect(int connId);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern bool TdxHq_GetSecurityQuotesAlloc(
        int connId, byte[] markets, IntPtr[] codes, ref ushort count,
        out IntPtr result, out IntPtr errInfo);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern void TdxHq_FreeBuffer(IntPtr p);
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

    static string PickLaneL1(string json)
    {
        int lanes = json.IndexOf("\"lanes\"", StringComparison.Ordinal);
        if (lanes < 0) return "";
        int k = json.IndexOf("\"L1\"", lanes, StringComparison.Ordinal);
        if (k < 0) return "";
        int brace = json.IndexOf('{', k);
        if (brace < 0) return "";
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
            Console.Error.WriteLine("找不到 login_prefs.json。请先运行行情测试demo.exe 连接一次。");
            return;
        }
        string lane = PickLaneL1(File.ReadAllText("login_prefs.json", Encoding.UTF8));
        string ip = JsonStr(lane, "ip");
        ushort port = (ushort)JsonInt(lane, "port", 7709);
        string account = JsonStr(lane, "account");
        string password = JsonStr(lane, "password");

        IntPtr result, errInfo;
        int cid = HqApi.TdxHq_ConnectAlloc(ip, port, account, password, out result, out errInfo);
        if (cid < 0)
        {
            Console.Error.WriteLine("连接失败: " + PtrUtf8(errInfo));
            if (result != IntPtr.Zero) HqApi.TdxHq_FreeBuffer(result);
            if (errInfo != IntPtr.Zero) HqApi.TdxHq_FreeBuffer(errInfo);
            return;
        }
        if (result != IntPtr.Zero) HqApi.TdxHq_FreeBuffer(result);
        if (errInfo != IntPtr.Zero) HqApi.TdxHq_FreeBuffer(errInfo);
        Console.WriteLine("连接成功 ConnID=" + cid);

        byte[] markets = { 1, 0 };
        IntPtr[] codes = {
            Marshal.StringToHGlobalAnsi("600519"),
            Marshal.StringToHGlobalAnsi("000001")
        };
        ushort count = 2;
        bool ok = HqApi.TdxHq_GetSecurityQuotesAlloc(cid, markets, codes, ref count, out result, out errInfo);
        if (!ok) Console.Error.WriteLine("行情失败: " + PtrUtf8(errInfo));
        else Console.WriteLine(PtrUtf8(result));
        if (result != IntPtr.Zero) HqApi.TdxHq_FreeBuffer(result);
        if (errInfo != IntPtr.Zero) HqApi.TdxHq_FreeBuffer(errInfo);
        foreach (var c in codes) Marshal.FreeHGlobal(c);

        HqApi.TdxHq_Disconnect(cid);
    }
}
