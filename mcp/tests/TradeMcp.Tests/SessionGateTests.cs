using TradeMcp.Core.Sessions;

namespace TradeMcp.Tests;

public class SessionGateTests
{
    [Fact]
    public async Task SameId_IsSerialized()
    {
        using var gate = new SessionGate();
        var order = new List<int>();
        var t1 = Task.Run(async () =>
        {
            await gate.RunAsync(1, async () =>
            {
                order.Add(1);
                await Task.Delay(80);
                order.Add(2);
                return 0;
            });
        });
        await Task.Delay(20);
        var t2 = Task.Run(async () =>
        {
            await gate.RunAsync(1, () =>
            {
                order.Add(3);
                return Task.FromResult(0);
            });
        });
        await Task.WhenAll(t1, t2);
        Assert.Equal([1, 2, 3], order);
    }
}
