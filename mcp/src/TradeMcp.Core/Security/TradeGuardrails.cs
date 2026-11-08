using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace TradeMcp.Core.Security;

public sealed class TradeOptions
{
    public bool TradingEnabled { get; set; }
    public bool BankTransferEnabled { get; set; }
    public int ConfirmTtlSeconds { get; set; } = 60;
    public int MaxOrderQuantity { get; set; } = 100_000;
    public decimal MaxOrderAmount { get; set; } = 1_000_000m;
    public decimal MaxBankTransferAmount { get; set; } = 200_000m;
    public string[] AllowedSymbols { get; set; } = [];
    public bool EnforceTradingSession { get; set; } = true;
}

public sealed record ConfirmTicket(
    string Token,
    string Action,
    string IdempotencyKey,
    string CanonicalJson,
    DateTimeOffset ExpiresAt,
    bool Consumed);

public sealed class TradeGuardrails
{
    private readonly TradeOptions _options;
    private readonly ILogger<TradeGuardrails>? _logger;
    private readonly ConcurrentDictionary<string, ConfirmTicket> _tickets = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, object> _idempotency = new(StringComparer.Ordinal);

    public TradeGuardrails(TradeOptions options, ILogger<TradeGuardrails>? logger = null)
    {
        _options = options;
        _logger = logger;
    }

    public TradeOptions Options => _options;

    public (bool ok, string? error) EnsureTradingEnabled()
    {
        if (!_options.TradingEnabled)
            return (false, "TradingEnabled=false. Set TRADE_MCP_TRADING_ENABLED=1 to allow write operations.");
        return (true, null);
    }

    public (bool ok, string? error) EnsureBankEnabled()
    {
        var t = EnsureTradingEnabled();
        if (!t.ok) return t;
        if (!_options.BankTransferEnabled)
            return (false, "BankTransferEnabled=false. Set TRADE_MCP_BANK_ENABLED=1 to allow bank transfers.");
        return (true, null);
    }

    public (bool ok, string? error) ValidateOrder(string zqdm, int quantity, float price)
    {
        if (quantity <= 0) return (false, "quantity must be > 0");
        if (quantity > _options.MaxOrderQuantity) return (false, $"quantity exceeds MaxOrderQuantity={_options.MaxOrderQuantity}");
        if (price < 0) return (false, "price invalid");
        var amount = (decimal)price * quantity;
        if (amount > _options.MaxOrderAmount) return (false, $"notional exceeds MaxOrderAmount={_options.MaxOrderAmount}");
        if (_options.AllowedSymbols.Length > 0 &&
            !_options.AllowedSymbols.Contains(zqdm, StringComparer.OrdinalIgnoreCase))
            return (false, $"symbol {zqdm} not in allow-list");
        if (_options.EnforceTradingSession && !IsLikelyTradingSession())
            return (false, "outside configured A-share trading session window");
        return (true, null);
    }

    public (bool ok, string? error) ValidateBankAmount(string amountText)
    {
        if (!decimal.TryParse(amountText, out var amount) || amount <= 0)
            return (false, "amount invalid");
        if (amount > _options.MaxBankTransferAmount)
            return (false, $"amount exceeds MaxBankTransferAmount={_options.MaxBankTransferAmount}");
        return (true, null);
    }

    public ConfirmTicket Prepare(string action, string idempotencyKey, object payload)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            throw new ArgumentException("idempotencyKey required");
        if (_idempotency.ContainsKey(idempotencyKey))
            throw new InvalidOperationException($"idempotencyKey already used: {idempotencyKey}");

        var canonical = JsonSerializer.Serialize(payload);
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
        var ticket = new ConfirmTicket(
            token,
            action,
            idempotencyKey,
            canonical,
            DateTimeOffset.UtcNow.AddSeconds(_options.ConfirmTtlSeconds),
            false);
        _tickets[token] = ticket;
        Audit("prepare", action, idempotencyKey, canonical);
        return ticket;
    }

    public (ConfirmTicket ticket, string? error) Consume(string token, string action, string idempotencyKey, object payload)
    {
        if (!_tickets.TryGetValue(token, out var ticket))
            return (ticket!, "unknown or expired confirmation token");
        if (ticket.Consumed)
            return (ticket, "confirmation token already used");
        if (ticket.ExpiresAt < DateTimeOffset.UtcNow)
        {
            _tickets.TryRemove(token, out _);
            return (ticket, "confirmation token expired");
        }
        if (!string.Equals(ticket.Action, action, StringComparison.Ordinal))
            return (ticket, "action mismatch");
        if (!string.Equals(ticket.IdempotencyKey, idempotencyKey, StringComparison.Ordinal))
            return (ticket, "idempotencyKey mismatch");
        var canonical = JsonSerializer.Serialize(payload);
        if (!FixedEquals(ticket.CanonicalJson, canonical))
            return (ticket, "payload mismatch vs prepared request");
        if (!_idempotency.TryAdd(idempotencyKey, new object()))
            return (ticket, "idempotencyKey already completed");

        var consumed = ticket with { Consumed = true };
        _tickets[token] = consumed;
        Audit("confirm", action, idempotencyKey, Redact(canonical));
        return (consumed, null);
    }

    public void Audit(string stage, string action, string idempotencyKey, string detail)
    {
        _logger?.LogInformation("trade-audit stage={Stage} action={Action} key={Key} detail={Detail}",
            stage, action, idempotencyKey, Redact(detail));
    }

    private static bool IsLikelyTradingSession()
    {
        var now = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTimeOffset.UtcNow, "China Standard Time");
        if (now.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) return false;
        var t = now.TimeOfDay;
        return (t >= TimeSpan.FromHours(9.5) && t <= TimeSpan.FromHours(11.5))
            || (t >= TimeSpan.FromHours(13) && t <= TimeSpan.FromHours(15));
    }

    private static bool FixedEquals(string a, string b)
    {
        var ba = Encoding.UTF8.GetBytes(a);
        var bb = Encoding.UTF8.GetBytes(b);
        return CryptographicOperations.FixedTimeEquals(ba, bb);
    }

    private static string Redact(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        return text
            .Replace("JyPassword", "JyPassword***", StringComparison.OrdinalIgnoreCase)
            .Replace("TxPassword", "TxPassword***", StringComparison.OrdinalIgnoreCase)
            .Replace("bankPwd", "bankPwd***", StringComparison.OrdinalIgnoreCase)
            .Replace("fundPwd", "fundPwd***", StringComparison.OrdinalIgnoreCase)
            .Replace("Password", "Password***", StringComparison.OrdinalIgnoreCase);
    }
}

public static class TradeOptionsLoader
{
    public static TradeOptions FromEnvironment()
    {
        var o = new TradeOptions();
        o.TradingEnabled = EnvBool("TRADE_MCP_TRADING_ENABLED");
        o.BankTransferEnabled = EnvBool("TRADE_MCP_BANK_ENABLED");
        if (int.TryParse(Environment.GetEnvironmentVariable("TRADE_MCP_CONFIRM_TTL"), out var ttl))
            o.ConfirmTtlSeconds = ttl;
        if (int.TryParse(Environment.GetEnvironmentVariable("TRADE_MCP_MAX_QTY"), out var qty))
            o.MaxOrderQuantity = qty;
        if (decimal.TryParse(Environment.GetEnvironmentVariable("TRADE_MCP_MAX_AMOUNT"), out var amt))
            o.MaxOrderAmount = amt;
        if (decimal.TryParse(Environment.GetEnvironmentVariable("TRADE_MCP_MAX_BANK"), out var bank))
            o.MaxBankTransferAmount = bank;
        var allow = Environment.GetEnvironmentVariable("TRADE_MCP_ALLOW_SYMBOLS");
        if (!string.IsNullOrWhiteSpace(allow))
            o.AllowedSymbols = allow.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (bool.TryParse(Environment.GetEnvironmentVariable("TRADE_MCP_ENFORCE_SESSION"), out var enforce))
            o.EnforceTradingSession = enforce;
        return o;
    }

    private static bool EnvBool(string name)
    {
        var v = Environment.GetEnvironmentVariable(name);
        return v is "1" or "true" or "TRUE" or "yes" or "YES";
    }
}
