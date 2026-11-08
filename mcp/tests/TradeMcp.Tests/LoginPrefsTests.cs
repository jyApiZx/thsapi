using TradeMcp.Core.Prefs;

namespace TradeMcp.Tests;

public class LoginPrefsTests
{
    [Fact]
    public void Trade_NormalAccount()
    {
        const string json = """
        {"lastAccount":"1234567890","accounts":{"1234567890":{
          "serverIp":"1.2.3.4","serverPort":8002,"yybId":"48","account":"1234567890",
          "tradeAccount":"","jyPassword":"pw","txPassword":"","isCredit":false,
          "config":{"qs_id":90,"account_type":0}}}}
        """;
        var a = LoginPrefs.ParseTrade(json);
        Assert.Equal("1.2.3.4", a.Ip);
        Assert.Equal(8002, a.Port);
        Assert.Equal(48, a.YybId);
        Assert.Equal("1234567890", a.AccountNo);
        Assert.Equal("1234567890", a.TradeAccount);
        Assert.Contains("\"qs_id\":90", a.Config);
        Assert.False(a.Credit);
    }

    [Fact]
    public void Trade_CreditAddsXToLoginOnly()
    {
        const string json = """
        {"accounts":{"1234567890":{
          "serverIp":"1.2.3.4","serverPort":"8002","account":"1234567890",
          "tradeAccount":"9876543210","jyPassword":"pw","isCredit":true,
          "apiQsid":"23","accountTypeCode":"0"}}}
        """;
        var a = LoginPrefs.ParseTrade(json);
        Assert.Equal("X1234567890", a.AccountNo);
        Assert.Equal("9876543210", a.TradeAccount);
        Assert.Equal("{\"qs_id\":23,\"account_type\":\"0\"}", a.Config);
    }

    [Fact]
    public void Trade_PicksAccountIgnoringX()
    {
        const string json = """
        {"accounts":{
          "111":{"serverIp":"h","serverPort":1,"account":"111","jyPassword":"a"},
          "X222":{"serverIp":"h","serverPort":1,"account":"X222","jyPassword":"b"}}}
        """;
        Assert.Equal("X222", LoginPrefs.ParseTrade(json, "222").Account);
        Assert.Equal("111", LoginPrefs.ParseTrade(json).Account);
    }

    [Fact]
    public void Trade_MissingPasswordThrows()
    {
        const string json = """{"accounts":{"1":{"serverIp":"h","serverPort":1,"account":"1"}}}""";
        Assert.Throws<InvalidDataException>(() => LoginPrefs.ParseTrade(json));
    }

    [Fact]
    public void Quote_ReadsLane()
    {
        const string json = """
        {"lanes":{"L1":{"ip":"5.6.7.8","port":7709,"account":"L1*abc","password":"p"},
                  "Ex":{"ip":"9.9.9.9","account":"KZ*abc","password":"q"}}}
        """;
        var l1 = LoginPrefs.ParseQuote(json);
        Assert.Equal("5.6.7.8", l1.Ip);
        Assert.Equal("L1*abc", l1.Account);
        var ex = LoginPrefs.ParseQuote(json, "ex");
        Assert.Equal(7727, ex.Port);
        Assert.Throws<InvalidDataException>(() => LoginPrefs.ParseQuote(json, "L2"));
    }
}
