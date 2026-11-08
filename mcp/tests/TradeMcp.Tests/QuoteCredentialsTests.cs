using TradeMcp.Core.Quotes;

namespace TradeMcp.Tests;

public class QuoteCredentialsTests
{
    [Fact]
    public void L1_RequiresAccountAndPassword()
    {
        Assert.Throws<ArgumentException>(() => QuoteCredentials.Resolve("L1", null, null));
        Assert.Throws<ArgumentException>(() => QuoteCredentials.Resolve("L1", "user1", ""));
    }

    [Fact]
    public void L1_StripsPrefix()
    {
        var (account, password, _) = QuoteCredentials.Resolve("L1", "L1*user1", "secret");
        Assert.Equal("user1", account);
        Assert.Equal("secret", password);
    }

    [Fact]
    public void L2_RequiresAccount()
    {
        Assert.Throws<ArgumentException>(() => QuoteCredentials.Resolve("L2", null, null));
        var (account, password, _) = QuoteCredentials.Resolve("L2", "user1", "secret");
        Assert.Equal("user1", account);
        Assert.Equal("secret", password);
    }
}
