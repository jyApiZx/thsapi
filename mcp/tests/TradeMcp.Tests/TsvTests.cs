using TradeMcp.Native.Text;

namespace TradeMcp.Tests;

public class TsvTests
{
    [Fact]
    public void Parse_HeadersAndRows()
    {
        var table = Tsv.Parse("代码\t名称\n600000\t浦发银行\n000001\t平安银行\n");
        Assert.Equal(2, table.Headers.Count);
        Assert.Equal(2, table.Rows.Count);
        var objs = table.AsObjects();
        Assert.Equal("浦发银行", objs[0]["名称"]);
    }

    [Fact]
    public void Parse_Empty()
    {
        Assert.Empty(Tsv.Parse(null).Rows);
        Assert.Empty(Tsv.Parse("").Headers);
    }

    [Fact]
    public void Parse_RespectsMaxRows()
    {
        var text = "a\n" + string.Join("\n", Enumerable.Range(0, 100).Select(i => i.ToString()));
        var table = Tsv.Parse(text, maxRows: 10);
        Assert.Equal(10, table.Rows.Count);
    }
}
