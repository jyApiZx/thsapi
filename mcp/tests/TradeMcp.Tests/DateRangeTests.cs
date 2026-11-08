using TradeMcp.Core.Query;

namespace TradeMcp.Tests;

public class DateRangeTests
{
    [Fact]
    public void Default_IsLast30Days()
    {
        var r = DateRange.Resolve(null, null, null, null);
        Assert.Equal(29, (r.End - r.Start).TotalDays);
        Assert.Equal(DateTime.Today, r.End);
    }

    [Fact]
    public void Months_Two()
    {
        var r = DateRange.Resolve(null, "20261028", null, months: 2);
        Assert.Equal(new DateTime(2026, 8, 28), r.Start);
        Assert.Equal(new DateTime(2026, 10, 28), r.End);
    }

    [Fact]
    public void Days_Window()
    {
        var r = DateRange.Resolve(null, "20261028", days: 10, null);
        Assert.Equal("20261019", r.StartText);
        Assert.Equal("20261028", r.EndText);
    }

    [Fact]
    public void Chunk_ThirtyDays()
    {
        var r = DateRange.Resolve("20260828", "20261028", null, null);
        var chunks = r.Chunk(30).Select(c => (c.Start.ToString("yyyyMMdd"), c.End.ToString("yyyyMMdd"))).ToList();
        Assert.Equal(3, chunks.Count);
        Assert.Equal(("20260828", "20260926"), chunks[0]);
        Assert.Equal(("20260927", "20261026"), chunks[1]);
        Assert.Equal(("20261027", "20261028"), chunks[2]);
    }
}
