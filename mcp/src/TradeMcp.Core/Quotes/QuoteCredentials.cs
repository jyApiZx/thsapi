namespace TradeMcp.Core.Quotes;

/// <summary>
/// L1 needs the licensed account (sent as L1*account) and a non-empty password.
/// L2 uses the bare TDX account; Ex uses the KZ* account.
/// </summary>
public static class QuoteCredentials
{
    public static (string account, string password, string note) Resolve(string lane, string? account, string? password)
    {
        var l = (lane ?? "L1").Trim();
        var user = (account ?? "").Trim();

        if (l.Equals("L1", StringComparison.OrdinalIgnoreCase))
        {
            // QuoteApi.AccountForLane adds L1* back.
            if (user.StartsWith("L1*", StringComparison.OrdinalIgnoreCase))
                user = user[3..].Trim();
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password))
                throw new ArgumentException("L1 requires the licensed account and a non-empty password.");
            return (user, password, "L1 sends L1*account");
        }

        if (string.IsNullOrEmpty(user) || password is null)
            throw new ArgumentException("L2/Ex require account and password.");

        return (user, password, l.Equals("L2", StringComparison.OrdinalIgnoreCase)
            ? "L2 uses bare TDX account (no L1*/L2* prefix)"
            : "Ex uses KZ* account prefix");
    }
}
