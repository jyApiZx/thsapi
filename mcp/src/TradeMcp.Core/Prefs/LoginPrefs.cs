using System.Text.Json;
using System.Text.Json.Nodes;

namespace TradeMcp.Core.Prefs;

public sealed record TradeLogonArgs(
    string Account, string Ip, short Port, string Config, short YybId,
    string AccountNo, string TradeAccount, string JyPassword, string TxPassword, bool Credit);

public sealed record QuoteLaneArgs(string Lane, string Ip, int Port, string Account, string Password);

/// <summary>
/// Reads login_prefs.json written by the demo programs after a successful login.
/// Field mapping and the credit "X" prefix rule match the thsapi Python SDK.
/// </summary>
public static class LoginPrefs
{
    public const string FileName = "login_prefs.json";

    public static string Locate(string? explicitPath)
    {
        if (!string.IsNullOrWhiteSpace(explicitPath))
        {
            if (!File.Exists(explicitPath))
                throw new FileNotFoundException($"{FileName} not found: {explicitPath}");
            return Path.GetFullPath(explicitPath);
        }
        foreach (var dir in new[] { AppContext.BaseDirectory, Environment.CurrentDirectory })
        {
            var p = Path.Combine(dir, FileName);
            if (File.Exists(p)) return Path.GetFullPath(p);
        }
        throw new FileNotFoundException(
            $"{FileName} not found. Log in once with the demo program and put this exe in the same folder.");
    }

    public static TradeLogonArgs ParseTrade(string json, string? account = null)
    {
        var root = JsonNode.Parse(json) as JsonObject ?? throw new InvalidDataException("login_prefs is not a JSON object");
        var accounts = root["accounts"] as JsonObject;
        if (accounts is null || accounts.Count == 0)
            throw new InvalidDataException("login_prefs has no accounts. Log in once with xdDemo.exe.");

        var key = PickAccountKey(accounts, (account ?? "").Trim(), Str(root, "lastAccount"));
        var acc = accounts[key] as JsonObject ?? throw new InvalidDataException($"account entry is not an object: {key}");

        var ip = Str(acc, "serverIp");
        if (ip.Length == 0) throw new InvalidDataException("login_prefs is missing serverIp");
        var port = Int(acc, "serverPort");
        if (port <= 0) throw new InvalidDataException("login_prefs is missing serverPort");
        var accountNo = Str(acc, "account");
        if (accountNo.Length == 0) throw new InvalidDataException("login_prefs is missing account");
        var jyPassword = RawStr(acc, "jyPassword");
        if (jyPassword.Length == 0) throw new InvalidDataException("login_prefs is missing jyPassword");

        var tradeRaw = Str(acc, "tradeAccount");
        var credit = Bool(acc, "isCredit");
        if (credit && !accountNo.StartsWith("X", StringComparison.OrdinalIgnoreCase))
            accountNo = "X" + accountNo;
        var trade = tradeRaw.Length > 0 ? tradeRaw : accountNo;

        var yyb = Int(acc, "yybId");
        if (yyb == 0) yyb = Int(acc, "branchId");

        return new TradeLogonArgs(key, ip, (short)port, Config(acc), (short)yyb,
            accountNo, trade, jyPassword, RawStr(acc, "txPassword"), credit);
    }

    public static QuoteLaneArgs ParseQuote(string json, string lane = "L1")
    {
        var root = JsonNode.Parse(json) as JsonObject ?? throw new InvalidDataException("login_prefs is not a JSON object");
        var lanes = root["lanes"] as JsonObject
            ?? throw new InvalidDataException("login_prefs has no lanes. Connect once with the HQ demo program.");
        var name = string.IsNullOrWhiteSpace(lane) ? "L1" : lane.Trim();
        var entry = lanes.FirstOrDefault(kv => kv.Key.Equals(name, StringComparison.OrdinalIgnoreCase)).Value as JsonObject
            ?? throw new InvalidDataException($"login_prefs has no saved {name} connection. Connect {name} once in the demo.");

        var ip = Str(entry, "ip");
        if (ip.Length == 0) throw new InvalidDataException($"login_prefs {name} is missing ip");
        var port = Int(entry, "port");
        if (port <= 0) port = name.Equals("Ex", StringComparison.OrdinalIgnoreCase) ? 7727 : 7709;
        return new QuoteLaneArgs(name, ip, port, Str(entry, "account"), RawStr(entry, "password"));
    }

    private static string PickAccountKey(JsonObject accounts, string wanted, string last)
    {
        var key = wanted.Length > 0 ? wanted : last;
        if (key.Length > 0 && accounts.ContainsKey(key)) return key;
        if (key.Length > 0)
        {
            var bare = StripX(key);
            foreach (var kv in accounts)
                if (StripX(kv.Key) == bare) return kv.Key;
        }
        return accounts.First().Key;
    }

    private static string StripX(string s) => s.StartsWith("X", StringComparison.OrdinalIgnoreCase) ? s[1..] : s;

    private static string Config(JsonObject acc)
    {
        var node = acc["config"];
        if (node is JsonObject obj && obj.Count > 0)
            return obj.ToJsonString();
        if (node is JsonValue v && v.TryGetValue<string>(out var s) && !string.IsNullOrWhiteSpace(s))
            return s.Trim();

        var cfg = new JsonObject();
        var qs = Str(acc, "apiQsid");
        if (qs.Length == 0) qs = Str(acc, "websiteQsid");
        if (qs.Length > 0)
            cfg["qs_id"] = int.TryParse(qs, out var n) ? n : qs;
        var at = Str(acc, "accountTypeCode");
        cfg["account_type"] = at.Length > 0 ? at : "k";
        return cfg.ToJsonString();
    }

    private static string RawStr(JsonObject o, string key)
        => o[key] is JsonValue v ? (v.TryGetValue<string>(out var s) ? s : v.ToJsonString()) : "";

    private static string Str(JsonObject o, string key) => RawStr(o, key).Trim();

    private static int Int(JsonObject o, string key)
    {
        if (o[key] is not JsonValue v) return 0;
        if (v.TryGetValue<int>(out var i)) return i;
        if (v.TryGetValue<string>(out var s) && int.TryParse(s.Trim(), out var j)) return j;
        return 0;
    }

    private static bool Bool(JsonObject o, string key)
    {
        if (o[key] is not JsonValue v) return false;
        if (v.TryGetValue<bool>(out var b)) return b;
        if (v.TryGetValue<int>(out var i)) return i != 0;
        if (v.TryGetValue<string>(out var s)) return s.Trim() is "1" or "true" or "True";
        return false;
    }
}
