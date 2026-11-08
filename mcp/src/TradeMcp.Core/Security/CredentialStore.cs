using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace TradeMcp.Core.Security;

public sealed record TradeProfile(
    string Name,
    string Ip,
    short Port,
    string Config,
    short YybId,
    string AccountNo,
    string TradeAccount,
    string JyPassword,
    string TxPassword);

public sealed record QuoteProfile(
    string Name,
    string Lane,
    string Ip,
    int Port,
    string Account,
    string Password);

/// <summary>Windows Credential Manager backed profile store. Target names: TradeMcp/trade/{name}, TradeMcp/quote/{name}.</summary>
public static class CredentialStore
{
    private const string TradePrefix = "TradeMcp/trade/";
    private const string QuotePrefix = "TradeMcp/quote/";

    public static void SaveTrade(TradeProfile profile)
    {
        var payload = JsonSerializer.Serialize(profile);
        Write(TradePrefix + profile.Name, payload);
    }

    public static void SaveQuote(QuoteProfile profile)
    {
        var payload = JsonSerializer.Serialize(profile);
        Write(QuotePrefix + profile.Name, payload);
    }

    public static TradeProfile? LoadTrade(string name)
    {
        var json = Read(TradePrefix + name);
        return json is null ? null : JsonSerializer.Deserialize<TradeProfile>(json);
    }

    public static QuoteProfile? LoadQuote(string name)
    {
        var json = Read(QuotePrefix + name);
        return json is null ? null : JsonSerializer.Deserialize<QuoteProfile>(json);
    }

    public static bool DeleteTrade(string name) => Delete(TradePrefix + name);
    public static bool DeleteQuote(string name) => Delete(QuotePrefix + name);

    private static void Write(string target, string secret)
    {
        var bytes = Encoding.UTF8.GetBytes(secret);
        var blob = Marshal.AllocHGlobal(bytes.Length);
        try
        {
            Marshal.Copy(bytes, 0, blob, bytes.Length);
            var cred = new CREDENTIAL
            {
                Type = 1, // generic
                TargetName = target,
                CredentialBlobSize = (uint)bytes.Length,
                CredentialBlob = blob,
                Persist = 2, // local machine
                UserName = Environment.UserName
            };
            if (!CredWrite(ref cred, 0))
                throw new InvalidOperationException($"CredWrite failed: {Marshal.GetLastWin32Error()}");
        }
        finally
        {
            Marshal.FreeHGlobal(blob);
        }
    }

    private static string? Read(string target)
    {
        if (!CredRead(target, 1, 0, out var ptr))
            return null;
        try
        {
            var cred = Marshal.PtrToStructure<CREDENTIAL>(ptr);
            if (cred.CredentialBlob == IntPtr.Zero || cred.CredentialBlobSize == 0)
                return null;
            var bytes = new byte[cred.CredentialBlobSize];
            Marshal.Copy(cred.CredentialBlob, bytes, 0, bytes.Length);
            return Encoding.UTF8.GetString(bytes);
        }
        finally
        {
            CredFree(ptr);
        }
    }

    private static bool Delete(string target) => CredDelete(target, 1, 0);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredWrite(ref CREDENTIAL userCredential, uint flags);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredRead(string target, int type, int reservedFlag, out IntPtr credentialPtr);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredDelete(string target, int type, int flags);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern void CredFree(IntPtr buffer);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct CREDENTIAL
    {
        public uint Flags;
        public uint Type;
        public string TargetName;
        public string? Comment;
        public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
        public uint CredentialBlobSize;
        public IntPtr CredentialBlob;
        public uint Persist;
        public uint AttributeCount;
        public IntPtr Attributes;
        public string? TargetAlias;
        public string? UserName;
    }
}
