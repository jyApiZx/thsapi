using TradeMcp.Native.Text;

namespace TradeMcp.Tests;

public class GbkTests
{
    [Fact]
    public void RoundTrip_Chinese()
    {
        var raw = Gbk.EncodeZ("正常");
        Assert.Equal("正常", Gbk.DecodeZ(raw));
    }

    [Fact]
    public void Encoding936_Available()
    {
        Assert.Equal(936, Gbk.Encoding.CodePage);
    }
}
