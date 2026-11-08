using System.ComponentModel;
using ModelContextProtocol.Server;
using TradeMcp.Core.Json;
using TradeMcp.Core.Security;
using TradeMcp.Core.Sessions;
using TradeMcp.Native.Interop;

namespace TradeMcp.Quotes.Server.Tools;

[McpServerToolType]
public sealed class QuoteTools(QuoteSessionManager sessions)
{
    [McpServerTool(Name = "quote_connect_from_prefs", ReadOnly = false, Destructive = false),
     Description("Recommended connect. Reads login_prefs.json saved by the HQ demo program after a successful connection (same folder as this exe by default), so no password goes through chat.")]
    public string ConnectFromPrefs(
        [Description("Lane saved in login_prefs.json: L1, L2, or Ex")] string lane = "L1",
        [Description("Optional full path of login_prefs.json; empty searches beside this exe, then the working directory")] string? prefsPath = null)
    {
        try
        {
            var path = TradeMcp.Core.Prefs.LoginPrefs.Locate(prefsPath);
            var a = TradeMcp.Core.Prefs.LoginPrefs.ParseQuote(File.ReadAllText(path), lane);
            return ResultJson.From(sessions.Connect(a.Lane, a.Ip, a.Port, a.Account, a.Password));
        }
        catch (Exception ex)
        {
            return ResultJson.Error("prefs_error", ex.Message);
        }
    }

    [McpServerTool(Name = "quote_save_profile", ReadOnly = false, Destructive = false),
     Description("Save a quote connection profile. L1 needs the licensed account and a non-empty password. L2 requires a real TDX account+password.")]
    public string SaveProfile(
        [Description("Profile name")] string name,
        [Description("Lane: L1, L2, or Ex")] string lane,
        [Description("Host IP or domain")] string ip,
        [Description("Port, e.g. 7709/7719/7727")] int port,
        [Description("Account. L1 licensed account (L1* added automatically); L2 bare TDX account; Ex may omit KZ*")] string account = "",
        [Description("Password, required")] string password = "")
    {
        try
        {
            var (user, pwd, note) = TradeMcp.Core.Quotes.QuoteCredentials.Resolve(lane, account, password);
            CredentialStore.SaveQuote(new QuoteProfile(name, lane, ip, port, user, pwd));
            return ResultJson.FromObject(new { ok = true, name, lane, ip, port, account = user, note });
        }
        catch (Exception ex)
        {
            return ResultJson.Error("invalid_args", ex.Message);
        }
    }

    [McpServerTool(Name = "quote_connect", ReadOnly = false, Destructive = false),
     Description("Connect HQ manually. Prefer quote_connect_from_prefs or profileName. All lanes require account+password; L1 adds L1* automatically.")]
    public string Connect(
        [Description("Optional saved profile name")] string? profileName = null,
        [Description("Lane if not using profile: L1/L2/Ex")] string lane = "L1",
        [Description("IP if not using profile")] string? ip = null,
        [Description("Port if not using profile")] int port = 7709,
        [Description("Account. L1 licensed account; L2 bare account; Ex KZ* account")] string? account = null,
        [Description("Password, required")] string? password = null)
    {
        if (!string.IsNullOrWhiteSpace(profileName))
        {
            var p = CredentialStore.LoadQuote(profileName)
                ?? throw new InvalidOperationException($"quote profile not found: {profileName}");
            return ResultJson.From(sessions.Connect(p.Lane, p.Ip, p.Port, p.Account, p.Password));
        }
        if (string.IsNullOrWhiteSpace(ip))
            return ResultJson.Error("invalid_args", "Use quote_connect_from_prefs, or provide profileName, or ip/account/password");
        try
        {
            return ResultJson.From(sessions.Connect(lane, ip, port, account, password));
        }
        catch (Exception ex)
        {
            return ResultJson.Error("invalid_args", ex.Message);
        }
    }

    [McpServerTool(Name = "quote_disconnect", ReadOnly = false, Destructive = false),
     Description("Disconnect an HQ or Ex connection id.")]
    public string Disconnect(
        [Description("Connection id")] int connectionId,
        [Description("Lane: L1/L2/Ex")] string lane = "L1")
        => ResultJson.From(sessions.Disconnect(lane, connectionId));

    [McpServerTool(Name = "quote_security_count", ReadOnly = true),
     Description("Get security count for a market. Market: 0 SZ, 1 SH, 2 BJ.")]
    public string SecurityCount(int connectionId, byte market)
        => ResultJson.From(sessions.WithHq(connectionId, api => api.SecurityCount(connectionId, market)));

    [McpServerTool(Name = "quote_security_list", ReadOnly = true),
     Description("Paged security list. Market 0/1/2.")]
    public string SecurityList(int connectionId, byte market, ushort start = 0, ushort count = 1000)
        => ResultJson.From(sessions.WithHq(connectionId, api => api.SecurityList(connectionId, market, start, count)));

    [McpServerTool(Name = "quote_bars", ReadOnly = true),
     Description("K-line bars. Use index=true for 999999/399xxx/880xxx. Category: 0=5m..4=day.")]
    public string Bars(int connectionId, string code, byte market, byte category = 4, bool index = false, ushort start = 0, ushort count = 30)
        => ResultJson.From(sessions.WithHq(connectionId, api => api.Bars(connectionId, index, category, market, code, start, count)));

    [McpServerTool(Name = "quote_minute", ReadOnly = true),
     Description("Intraday minute series.")]
    public string Minute(int connectionId, string code, byte market)
        => ResultJson.From(sessions.WithHq(connectionId, api => api.Minute(connectionId, market, code)));

    [McpServerTool(Name = "quote_history_minute", ReadOnly = true),
     Description("Historical minute series for date yyyyMMdd.")]
    public string HistoryMinute(int connectionId, string code, byte market, int date)
        => ResultJson.From(sessions.WithHq(connectionId, api => api.HistoryMinute(connectionId, market, code, date)));

    [McpServerTool(Name = "quote_transactions", ReadOnly = true),
     Description("Tick transactions.")]
    public string Transactions(int connectionId, string code, byte market, ushort start = 0, ushort count = 30)
        => ResultJson.From(sessions.WithHq(connectionId, api => api.Transactions(connectionId, market, code, start, count)));

    [McpServerTool(Name = "quote_history_transactions", ReadOnly = true),
     Description("Historical tick transactions for date yyyyMMdd.")]
    public string HistoryTransactions(int connectionId, string code, byte market, int date, ushort start = 0, ushort count = 30)
        => ResultJson.From(sessions.WithHq(connectionId, api => api.HistoryTransactions(connectionId, market, code, start, count, date)));

    [McpServerTool(Name = "quote_quotes", ReadOnly = true),
     Description("Batch L1 quotes. codesCsv like '600519,000001'; marketsCsv optional parallel list '1,0'. Max logical 80/pack (DLL auto-splits).")]
    public string Quotes(int connectionId, string codesCsv, string? marketsCsv = null, bool ten = false, byte defaultMarket = 1)
    {
        var codes = codesCsv.Split([',', ';', ' ', '，'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        byte[] markets;
        if (string.IsNullOrWhiteSpace(marketsCsv))
            markets = Enumerable.Repeat(defaultMarket, codes.Length).ToArray();
        else
            markets = marketsCsv.Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries).Select(byte.Parse).ToArray();
        if (markets.Length != codes.Length)
            return ResultJson.Error("invalid_args", "marketsCsv length must match codes");
        var items = codes.Select((c, i) => (markets[i], c)).ToList();
        return ResultJson.From(sessions.WithHq(connectionId, api => api.Quotes(connectionId, ten, items)));
    }

    [McpServerTool(Name = "quote_auction", ReadOnly = true), Description("Call auction data.")]
    public string Auction(int connectionId, string code, byte market, int count = 30)
        => ResultJson.From(sessions.WithHq(connectionId, api => api.Auction(connectionId, market, code, count)));

    [McpServerTool(Name = "quote_l2_queue", ReadOnly = true), Description("L2 buy/sell queue. Requires L2 host/account.")]
    public string L2Queue(int connectionId, string code, byte market)
        => ResultJson.From(sessions.WithHq(connectionId, api => api.Queue(connectionId, market, code)));

    [McpServerTool(Name = "quote_l2_detail", ReadOnly = true),
     Description("L2 detail. kind: 0 transaction, 1 original transaction, 2 order.")]
    public string L2Detail(int connectionId, string code, byte market, ushort count = 30, int kind = 0)
        => ResultJson.From(sessions.WithHq(connectionId, api => api.Detail(connectionId, market, code, count, kind)));

    [McpServerTool(Name = "quote_boards", ReadOnly = true),
     Description("List boards. boardType: concept/style/industry/index/gn/fg/zs.")]
    public string Boards(int connectionId, string boardType = "concept")
        => ResultJson.From(sessions.WithHq(connectionId, api => api.Boards(connectionId, boardType)));

    [McpServerTool(Name = "quote_board_members", ReadOnly = true),
     Description("Board members by GBK board name.")]
    public string BoardMembers(int connectionId, string boardName, string blockFile = "block_gn.dat")
        => ResultJson.From(sessions.WithHq(connectionId, api => api.Members(connectionId, boardName, blockFile)));

    [McpServerTool(Name = "quote_market_stat", ReadOnly = true), Description("A-share market breadth stats.")]
    public string MarketStat(int connectionId)
        => ResultJson.From(sessions.WithHq(connectionId, api => api.MarketStat(connectionId)));

    [McpServerTool(Name = "quote_fund_flow", ReadOnly = true), Description("Today fund flow from L1 ticks.")]
    public string FundFlow(int connectionId, string code, byte market)
        => ResultJson.From(sessions.WithHq(connectionId, api => api.FundFlow(connectionId, market, code)));

    [McpServerTool(Name = "quote_history_fund_flow", ReadOnly = true), Description("Historical daily fund flow.")]
    public string HistoryFundFlow(int connectionId, string code, byte market, ushort start = 0, ushort count = 10)
        => ResultJson.From(sessions.WithHq(connectionId, api => api.HistoryFundFlow(connectionId, market, code, start, count)));

    [McpServerTool(Name = "quote_xdxr", ReadOnly = true), Description("Ex-dividend / rights info.")]
    public string Xdxr(int connectionId, string code, byte market)
        => ResultJson.From(sessions.WithHq(connectionId, api => api.Xdxr(connectionId, market, code)));

    [McpServerTool(Name = "quote_finance", ReadOnly = true), Description("Finance info.")]
    public string Finance(int connectionId, string code, byte market)
        => ResultJson.From(sessions.WithHq(connectionId, api => api.Finance(connectionId, market, code)));

    [McpServerTool(Name = "quote_company_category", ReadOnly = true), Description("Company info category list.")]
    public string CompanyCategory(int connectionId, string code, byte market)
        => ResultJson.From(sessions.WithHq(connectionId, api => api.CompanyCategory(connectionId, market, code)));

    [McpServerTool(Name = "ex_markets", ReadOnly = true), Description("Extended HQ markets.")]
    public string ExMarkets(int connectionId)
        => ResultJson.From(sessions.WithEx(connectionId, api => api.ExMarkets(connectionId)));

    [McpServerTool(Name = "ex_instrument_count", ReadOnly = true), Description("Extended instrument count.")]
    public string ExCount(int connectionId)
        => ResultJson.From(sessions.WithEx(connectionId, api => api.ExCount(connectionId)));

    [McpServerTool(Name = "ex_instrument_info", ReadOnly = true), Description("Extended instrument info page.")]
    public string ExInfo(int connectionId, int start = 0, short count = 40)
        => ResultJson.From(sessions.WithEx(connectionId, api => api.ExInfo(connectionId, start, count)));

    [McpServerTool(Name = "ex_bars", ReadOnly = true), Description("Extended instrument bars.")]
    public string ExBars(int connectionId, string code, byte market, byte category = 4, int start = 0, short count = 30)
        => ResultJson.From(sessions.WithEx(connectionId, api => api.ExBars(connectionId, category, market, code, start, count)));

    [McpServerTool(Name = "ex_quote", ReadOnly = true), Description("Extended single instrument quote.")]
    public string ExQuote(int connectionId, string code, byte market)
        => ResultJson.From(sessions.WithEx(connectionId, api => api.ExQuote(connectionId, market, code)));

    [McpServerTool(Name = "ex_minute", ReadOnly = true), Description("Extended minute series.")]
    public string ExMinute(int connectionId, string code, byte market)
        => ResultJson.From(sessions.WithEx(connectionId, api => api.ExMinute(connectionId, market, code)));

    [McpServerTool(Name = "ex_transactions", ReadOnly = true), Description("Extended transactions.")]
    public string ExTx(int connectionId, string code, byte market, int start = 0, short count = 30)
        => ResultJson.From(sessions.WithEx(connectionId, api => api.ExTransactions(connectionId, market, code, start, count)));
}
