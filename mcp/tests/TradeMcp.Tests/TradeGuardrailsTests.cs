using TradeMcp.Core.Security;

namespace TradeMcp.Tests;

public class TradeGuardrailsTests
{
    private static TradeGuardrails Create(bool enabled = true) =>
        new(new TradeOptions
        {
            TradingEnabled = enabled,
            BankTransferEnabled = enabled,
            ConfirmTtlSeconds = 30,
            MaxOrderQuantity = 1000,
            MaxOrderAmount = 100000,
            EnforceTradingSession = false
        });

    [Fact]
    public void TradingDisabled_BlocksPrepare()
    {
        var g = Create(enabled: false);
        var (ok, error) = g.EnsureTradingEnabled();
        Assert.False(ok);
        Assert.Contains("TradingEnabled", error);
    }

    [Fact]
    public void PrepareAndConfirm_Succeeds()
    {
        var g = Create();
        var payload = new { clientId = 1, zqdm = "600000", quantity = 100, price = 10f };
        var ticket = g.Prepare("order", "key-1", payload);
        Assert.False(string.IsNullOrEmpty(ticket.Token));
        var (consumed, error) = g.Consume(ticket.Token, "order", "key-1", payload);
        Assert.Null(error);
        Assert.True(consumed.Consumed);
    }

    [Fact]
    public void Confirm_RejectsReplay()
    {
        var g = Create();
        var payload = new { clientId = 1, zqdm = "600000", quantity = 100, price = 10f };
        var ticket = g.Prepare("order", "key-2", payload);
        var first = g.Consume(ticket.Token, "order", "key-2", payload);
        Assert.Null(first.error);
        var second = g.Consume(ticket.Token, "order", "key-2", payload);
        Assert.NotNull(second.error);
    }

    [Fact]
    public void Confirm_RejectsPayloadMismatch()
    {
        var g = Create();
        var payload = new { clientId = 1, zqdm = "600000", quantity = 100, price = 10f };
        var ticket = g.Prepare("order", "key-3", payload);
        var bad = new { clientId = 1, zqdm = "600000", quantity = 200, price = 10f };
        var (_, error) = g.Consume(ticket.Token, "order", "key-3", bad);
        Assert.Contains("payload mismatch", error);
    }

    [Fact]
    public void IdempotencyKey_CannotReuseOnPrepare()
    {
        var g = Create();
        var payload = new { x = 1 };
        var ticket = g.Prepare("order", "key-4", payload);
        g.Consume(ticket.Token, "order", "key-4", payload);
        Assert.Throws<InvalidOperationException>(() => g.Prepare("order", "key-4", payload));
    }

    [Fact]
    public void ValidateOrder_EnforcesLimits()
    {
        var g = Create();
        Assert.False(g.ValidateOrder("600000", 5000, 10).ok);
        Assert.False(g.ValidateOrder("600000", 100, 2000).ok);
        Assert.True(g.ValidateOrder("600000", 100, 10).ok);
    }
}
