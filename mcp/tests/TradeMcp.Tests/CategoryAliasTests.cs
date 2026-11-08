using TradeMcp.Core.Query;

namespace TradeMcp.Tests;

public class CategoryAliasTests
{
    [Theory]
    [InlineData("orders", 0)]
    [InlineData("fills", 1)]
    [InlineData("委托", 0)]
    [InlineData("3", 3)]
    public void History_Aliases(string alias, int expected)
    {
        Assert.True(CategoryAlias.TryResolveHistory(alias, out var cat, out _));
        Assert.Equal(expected, cat);
    }

    [Theory]
    [InlineData("funds", 0)]
    [InlineData("positions", 1)]
    [InlineData("orders", 2)]
    [InlineData("fills", 3)]
    public void Today_Aliases(string alias, int expected)
    {
        Assert.True(CategoryAlias.TryResolveToday(alias, out var cat, out _));
        Assert.Equal(expected, cat);
    }

    [Fact]
    public void History_DefaultEmpty_IsOrders()
    {
        Assert.True(CategoryAlias.TryResolveHistory(null, out var cat, out _));
        Assert.Equal(0, cat);
    }
}
