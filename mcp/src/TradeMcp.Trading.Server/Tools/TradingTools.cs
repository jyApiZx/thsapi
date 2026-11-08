using System.ComponentModel;
using ModelContextProtocol.Server;
using TradeMcp.Core.Json;
using TradeMcp.Core.Query;
using TradeMcp.Core.Security;
using TradeMcp.Core.Sessions;
using TradeMcp.Native.Interop;

namespace TradeMcp.Trading.Server.Tools;

[McpServerToolType]
public sealed class TradingTools(TradeSessionManager sessions, TradeGuardrails guardrails)
{
    [McpServerTool(Name = "trade_save_profile"),
     Description("Save a trading profile into Windows Credential Manager. Prefer this over passing passwords into later tool calls.")]
    public string SaveProfile(
        string name, string ip, short port, string accountNo, string jyPassword,
        string tradeAccount = "", string txPassword = "", string config = "", short yybId = 0)
    {
        CredentialStore.SaveTrade(new TradeProfile(name, ip, port, config, yybId, accountNo,
            string.IsNullOrEmpty(tradeAccount) ? accountNo : tradeAccount, jyPassword, txPassword));
        return ResultJson.FromObject(new { ok = true, name, ip, port, accountNo, hint = "Use trade_logon(profileName=...) afterwards." });
    }

    [McpServerTool(Name = "trade_logon"),
     Description("Logon. Prefer profileName from trade_save_profile so passwords stay out of chat. Returns clientId, status, and reconnect hint.")]
    public string Logon(
        string? profileName = null,
        string? ip = null,
        short port = 8002,
        string config = "",
        short yybId = 0,
        string? accountNo = null,
        string? tradeAccount = null,
        string? jyPassword = null,
        string? txPassword = null)
    {
        if (string.IsNullOrWhiteSpace(profileName) && (ip is null || accountNo is null || jyPassword is null))
            return ResultJson.Error("invalid_args", "Prefer profileName. Or pass ip/accountNo/jyPassword.");

        if (!string.IsNullOrWhiteSpace(profileName))
        {
            var p = CredentialStore.LoadTrade(profileName);
            if (p is null)
                return ResultJson.Error("profile_not_found", $"trade profile not found: {profileName}. Call trade_save_profile first.");
            var r = sessions.Logon(p.Name, p.Ip, p.Port, p.Config, p.YybId, p.AccountNo, p.TradeAccount, p.JyPassword, p.TxPassword);
            return LogonPayload(r, p.Name);
        }

        var adhoc = sessions.Logon("adhoc", ip!, port, config, yybId, accountNo!, tradeAccount ?? accountNo!, jyPassword!, txPassword ?? "");
        return LogonPayload(adhoc, null);
    }

    [McpServerTool(Name = "trade_logon_from_prefs"),
     Description("Recommended logon. Reads login_prefs.json saved by xdDemo.exe after a successful login (same folder as this exe by default), so no password goes through chat. Credit accounts get the X prefix automatically.")]
    public string LogonFromPrefs(
        [Description("Optional account key in login_prefs.json; empty uses the last logged-in account")] string? account = null,
        [Description("Optional full path of login_prefs.json; empty searches beside this exe, then the working directory")] string? prefsPath = null)
    {
        TradeMcp.Core.Prefs.TradeLogonArgs a;
        try
        {
            var path = TradeMcp.Core.Prefs.LoginPrefs.Locate(prefsPath);
            a = TradeMcp.Core.Prefs.LoginPrefs.ParseTrade(File.ReadAllText(path), account);
        }
        catch (Exception ex)
        {
            return ResultJson.Error("prefs_error", ex.Message);
        }
        var r = sessions.Logon("prefs:" + a.Account, a.Ip, a.Port, a.Config, a.YybId, a.AccountNo, a.TradeAccount, a.JyPassword, a.TxPassword);
        return LogonPayload(r, "prefs:" + a.Account);
    }

    private static string LogonPayload(NativeResult r, string? profile)
    {
        if (!r.Ok || r.Id is not int id)
            return ResultJson.FromObject(new { ok = false, error = r.Error, profile });
        return ResultJson.FromObject(new
        {
            ok = true,
            clientId = id,
            status = r.Text,
            profile,
            hint = "Same clientId must be used serially. If status becomes 已断开/正在重连, call trade_status then trade_logon again."
        });
    }

    [McpServerTool(Name = "trade_logoff"), Description("Logoff a clientId and release the session.")]
    public string Logoff(int clientId) => ResultJson.From(sessions.Logoff(clientId));

    [McpServerTool(Name = "trade_status", ReadOnly = true),
     Description("Connection status for clientId. Typical values: 正常 / 正在重连 / 已断开 / 已登出 / 无效ClientID. If not 正常, re-logon.")]
    public string Status(int clientId)
    {
        var status = sessions.Status(clientId);
        var ok = status.Contains("正常", StringComparison.Ordinal);
        return ResultJson.FromObject(new
        {
            ok,
            clientId,
            status,
            reconnectSuggested = !ok,
            hint = ok ? null : "Session unhealthy. Call trade_logon (prefer profileName) to reconnect; old clientId is invalid after logoff/disconnect."
        });
    }

    [McpServerTool(Name = "trade_query", ReadOnly = true),
     Description("Today QueryData. category alias or id: funds/0, positions/1, orders/2, fills/3, cancelable/4, shareholders/5, credit 6/7/8, ipo/12, ipo_quota/13, allotment/14, winning/15.")]
    public string Query(int clientId, string category = "funds")
    {
        if (!CategoryAlias.TryResolveToday(category, out var cat, out var error))
            return ResultJson.Error("invalid_category", error!);
        return ResultJson.From(sessions.WithClient(clientId, api => api.QueryData(clientId, cat)));
    }

    [McpServerTool(Name = "trade_query_history", ReadOnly = true),
     Description("Historical query with automatic <=30-day chunking and merge. category: orders/fills/delivery/fund/statement (or 0-4). Provide startDate+endDate (yyyyMMdd), or days/months; default last 30 days. Use offset/limit to page rows; summaryOnly=true for counts only; exportPath writes full JSON/TSV.")]
    public string QueryHistory(
        int clientId,
        string category = "orders",
        string? startDate = null,
        string? endDate = null,
        int? days = null,
        int? months = null,
        int chunkDays = 30,
        int offset = 0,
        int limit = 100,
        bool summaryOnly = false,
        string? exportPath = null)
    {
        if (!CategoryAlias.TryResolveHistory(category, out var cat, out var error))
            return ResultJson.Error("invalid_category", error!);

        DateRange range;
        try
        {
            range = DateRange.Resolve(startDate, endDate, days, months, defaultDays: 30);
        }
        catch (Exception ex)
        {
            return ResultJson.Error("invalid_dates", ex.Message);
        }

        var result = sessions.WithClient(clientId, api =>
            HistoryQuery.Execute(api, clientId, cat, range, chunkDays, offset, limit, summaryOnly, exportPath));
        return ResultJson.FromObject(HistoryQuery.ToPayload(result));
    }

    [McpServerTool(Name = "trade_shareholders", ReadOnly = true), Description("Shareholder codes.")]
    public string Shareholders(int clientId)
        => ResultJson.From(sessions.WithClient(clientId, api => api.GetShareholders(clientId)));

    [McpServerTool(Name = "trade_quote", ReadOnly = true), Description("Broker five-level quote for a code.")]
    public string Quote(int clientId, string zqdm)
        => ResultJson.From(sessions.WithClient(clientId, api => api.GetQuote(clientId, zqdm)));

    [McpServerTool(Name = "trade_tradable_qty", ReadOnly = true),
     Description("Tradable quantity. category 0 buy / 1 sell; priceType 0 limit.")]
    public string TradableQty(int clientId, string gddm, string zqdm, float price, int category = 0, int priceType = 0)
        => ResultJson.From(sessions.WithClient(clientId, api => api.GetTradableQuantity(clientId, category, priceType, gddm, zqdm, price)));

    [McpServerTool(Name = "trade_query_bank", ReadOnly = true), Description("Bank list.")]
    public string QueryBank(int clientId)
        => ResultJson.From(sessions.WithClient(clientId, api => api.QueryBank(clientId)));

    [McpServerTool(Name = "trade_query_bank_balance", ReadOnly = true), Description("Bank balance.")]
    public string QueryBankBalance(int clientId)
        => ResultJson.From(sessions.WithClient(clientId, api => api.QueryBankBalance(clientId)));

    [McpServerTool(Name = "trade_query_bank_flow", ReadOnly = true),
     Description("Bank transfer flow. Dates optional yyyyMMdd. Prefer ranges within broker limits (often <=30 days per call).")]
    public string QueryBankFlow(int clientId, string startDate = "", string endDate = "")
        => ResultJson.From(sessions.WithClient(clientId, api => api.QueryBankFlow(clientId, startDate, endDate)));

    [McpServerTool(Name = "trade_drain_push", ReadOnly = true), Description("Drain buffered push notifications (fills/status).")]
    public string DrainPush(int max = 50)
        => ResultJson.FromObject(new { ok = true, events = sessions.DrainPush(max) });

    [McpServerTool(Name = "trade_prepare_order", Destructive = true),
     Description("Stage 1: validate and prepare an order. Returns confirmationToken (short TTL). Does not send the order.")]
    public string PrepareOrder(
        int clientId, int category, int priceType, string gddm, string zqdm, int exchangeId,
        float price, int quantity, string idempotencyKey)
    {
        var en = guardrails.EnsureTradingEnabled();
        if (!en.ok) return ResultJson.Error("trading_disabled", en.error!);
        var v = guardrails.ValidateOrder(zqdm, quantity, price);
        if (!v.ok) return ResultJson.Error("validation", v.error!);
        var payload = new { clientId, category, priceType, gddm, zqdm, exchangeId, price, quantity };
        try
        {
            var ticket = guardrails.Prepare("order", idempotencyKey, payload);
            return ResultJson.FromObject(new
            {
                ok = true,
                confirmationToken = ticket.Token,
                expiresAt = ticket.ExpiresAt,
                action = ticket.Action,
                idempotencyKey,
                preview = payload
            });
        }
        catch (Exception ex)
        {
            return ResultJson.Error("prepare_failed", ex.Message);
        }
    }

    [McpServerTool(Name = "trade_confirm_order", Destructive = true),
     Description("Stage 2: confirm and send order using confirmationToken + same parameters + idempotencyKey.")]
    public string ConfirmOrder(
        string confirmationToken, string idempotencyKey,
        int clientId, int category, int priceType, string gddm, string zqdm, int exchangeId,
        float price, int quantity)
    {
        var payload = new { clientId, category, priceType, gddm, zqdm, exchangeId, price, quantity };
        var (_, error) = guardrails.Consume(confirmationToken, "order", idempotencyKey, payload);
        if (error is not null) return ResultJson.Error("confirm_failed", error);
        var result = sessions.WithClient(clientId, api => api.SendOrderEx(clientId, category, priceType, gddm, zqdm, exchangeId, price, quantity));
        guardrails.Audit("executed", "order", idempotencyKey, result.Ok ? "ok" : result.Error ?? "fail");
        return ResultJson.From(result);
    }

    [McpServerTool(Name = "trade_prepare_cancel", Destructive = true),
     Description("Stage 1: prepare cancel. exchangeId: SH '1', SZ '0'.")]
    public string PrepareCancel(int clientId, string gddm, string exchangeId, string entrustId, string idempotencyKey)
    {
        var en = guardrails.EnsureTradingEnabled();
        if (!en.ok) return ResultJson.Error("trading_disabled", en.error!);
        var payload = new { clientId, gddm, exchangeId, entrustId };
        try
        {
            var ticket = guardrails.Prepare("cancel", idempotencyKey, payload);
            return ResultJson.FromObject(new { ok = true, confirmationToken = ticket.Token, expiresAt = ticket.ExpiresAt, preview = payload });
        }
        catch (Exception ex) { return ResultJson.Error("prepare_failed", ex.Message); }
    }

    [McpServerTool(Name = "trade_confirm_cancel", Destructive = true),
     Description("Stage 2: confirm cancel.")]
    public string ConfirmCancel(string confirmationToken, string idempotencyKey, int clientId, string gddm, string exchangeId, string entrustId)
    {
        var payload = new { clientId, gddm, exchangeId, entrustId };
        var (_, error) = guardrails.Consume(confirmationToken, "cancel", idempotencyKey, payload);
        if (error is not null) return ResultJson.Error("confirm_failed", error);
        var result = sessions.WithClient(clientId, api => api.CancelOrderEx(clientId, gddm, exchangeId, entrustId));
        return ResultJson.From(result);
    }

    [McpServerTool(Name = "trade_prepare_bank_transfer", Destructive = true),
     Description("Stage 1: prepare bank transfer. direction 0 bank->security, 1 security->bank.")]
    public string PrepareBankTransfer(
        int clientId, int direction, string bankCode, string bankAccount, string amount,
        string idempotencyKey, string bankPwd = "", string fundPwd = "", string moneyType = "R", string gddm = "")
    {
        var en = guardrails.EnsureBankEnabled();
        if (!en.ok) return ResultJson.Error("bank_disabled", en.error!);
        var v = guardrails.ValidateBankAmount(amount);
        if (!v.ok) return ResultJson.Error("validation", v.error!);
        var payload = new { clientId, direction, bankCode, bankAccount, amount, moneyType, gddm };
        try
        {
            var ticket = guardrails.Prepare("bank", idempotencyKey, payload);
            return ResultJson.FromObject(new { ok = true, confirmationToken = ticket.Token, expiresAt = ticket.ExpiresAt, preview = payload });
        }
        catch (Exception ex) { return ResultJson.Error("prepare_failed", ex.Message); }
    }

    [McpServerTool(Name = "trade_confirm_bank_transfer", Destructive = true),
     Description("Stage 2: confirm bank transfer. Re-supply bankPwd/fundPwd; they are not stored in the token.")]
    public string ConfirmBankTransfer(
        string confirmationToken, string idempotencyKey,
        int clientId, int direction, string bankCode, string bankAccount, string amount,
        string bankPwd = "", string fundPwd = "", string moneyType = "R", string gddm = "")
    {
        var payload = new { clientId, direction, bankCode, bankAccount, amount, moneyType, gddm };
        var (_, error) = guardrails.Consume(confirmationToken, "bank", idempotencyKey, payload);
        if (error is not null) return ResultJson.Error("confirm_failed", error);
        var result = sessions.WithClient(clientId, api => api.BankTransfer(clientId, direction, bankCode, bankAccount, amount, bankPwd, fundPwd, moneyType, string.IsNullOrEmpty(gddm) ? null : gddm));
        return ResultJson.From(result);
    }

    [McpServerTool(Name = "trade_prepare_repay", Destructive = true), Description("Stage 1: prepare margin repay.")]
    public string PrepareRepay(int clientId, string amount, string idempotencyKey)
    {
        var en = guardrails.EnsureTradingEnabled();
        if (!en.ok) return ResultJson.Error("trading_disabled", en.error!);
        var payload = new { clientId, amount };
        try
        {
            var ticket = guardrails.Prepare("repay", idempotencyKey, payload);
            return ResultJson.FromObject(new { ok = true, confirmationToken = ticket.Token, expiresAt = ticket.ExpiresAt, preview = payload });
        }
        catch (Exception ex) { return ResultJson.Error("prepare_failed", ex.Message); }
    }

    [McpServerTool(Name = "trade_confirm_repay", Destructive = true), Description("Stage 2: confirm repay.")]
    public string ConfirmRepay(string confirmationToken, string idempotencyKey, int clientId, string amount)
    {
        var payload = new { clientId, amount };
        var (_, error) = guardrails.Consume(confirmationToken, "repay", idempotencyKey, payload);
        if (error is not null) return ResultJson.Error("confirm_failed", error);
        return ResultJson.From(sessions.WithClient(clientId, api => api.Repay(clientId, amount)));
    }

    [McpServerTool(Name = "trade_prepare_fund_order", Destructive = true),
     Description("Stage 1: prepare fund/ETF order. category 0/1 on-exchange, 2/3 ETF.")]
    public string PrepareFundOrder(int clientId, int category, string gddm, string zqdm, int exchangeId, string amount, string idempotencyKey)
    {
        var en = guardrails.EnsureTradingEnabled();
        if (!en.ok) return ResultJson.Error("trading_disabled", en.error!);
        var payload = new { clientId, category, gddm, zqdm, exchangeId, amount };
        try
        {
            var ticket = guardrails.Prepare("fund", idempotencyKey, payload);
            return ResultJson.FromObject(new { ok = true, confirmationToken = ticket.Token, expiresAt = ticket.ExpiresAt, preview = payload });
        }
        catch (Exception ex) { return ResultJson.Error("prepare_failed", ex.Message); }
    }

    [McpServerTool(Name = "trade_confirm_fund_order", Destructive = true), Description("Stage 2: confirm fund/ETF order.")]
    public string ConfirmFundOrder(string confirmationToken, string idempotencyKey, int clientId, int category, string gddm, string zqdm, int exchangeId, string amount)
    {
        var payload = new { clientId, category, gddm, zqdm, exchangeId, amount };
        var (_, error) = guardrails.Consume(confirmationToken, "fund", idempotencyKey, payload);
        if (error is not null) return ResultJson.Error("confirm_failed", error);
        return ResultJson.From(sessions.WithClient(clientId, api => api.FundOrder(clientId, category, gddm, zqdm, exchangeId, amount)));
    }

    [McpServerTool(Name = "trade_prepare_ipo", Destructive = true),
     Description("Stage 1: prepare one-click IPO. Prefer dryRun=true first.")]
    public string PrepareIpo(int clientId, bool dryRun, string idempotencyKey)
    {
        var en = guardrails.EnsureTradingEnabled();
        if (!en.ok) return ResultJson.Error("trading_disabled", en.error!);
        var payload = new { clientId, dryRun };
        try
        {
            var ticket = guardrails.Prepare("ipo", idempotencyKey, payload);
            return ResultJson.FromObject(new { ok = true, confirmationToken = ticket.Token, expiresAt = ticket.ExpiresAt, preview = payload });
        }
        catch (Exception ex) { return ResultJson.Error("prepare_failed", ex.Message); }
    }

    [McpServerTool(Name = "trade_confirm_ipo", Destructive = true), Description("Stage 2: confirm one-click IPO.")]
    public string ConfirmIpo(string confirmationToken, string idempotencyKey, int clientId, bool dryRun)
    {
        var payload = new { clientId, dryRun };
        var (_, error) = guardrails.Consume(confirmationToken, "ipo", idempotencyKey, payload);
        if (error is not null) return ResultJson.Error("confirm_failed", error);
        return ResultJson.From(sessions.WithClient(clientId, api => api.OneClickIpo(clientId, dryRun)));
    }

    [McpServerTool(Name = "trade_options", ReadOnly = true), Description("Show current trading guardrail options (no secrets).")]
    public string Options()
    {
        var o = guardrails.Options;
        return ResultJson.FromObject(new
        {
            ok = true,
            o.TradingEnabled,
            o.BankTransferEnabled,
            o.ConfirmTtlSeconds,
            o.MaxOrderQuantity,
            o.MaxOrderAmount,
            o.MaxBankTransferAmount,
            o.AllowedSymbols,
            o.EnforceTradingSession,
            history = new
            {
                autoChunkDays = 30,
                note = "trade_query_history auto-splits ranges into <=30-day chunks because many brokers reject longer windows."
            }
        });
    }
}
