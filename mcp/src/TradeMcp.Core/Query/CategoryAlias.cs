namespace TradeMcp.Core.Query;

public static class CategoryAlias
{
    private static readonly Dictionary<string, int> Today = new(StringComparer.OrdinalIgnoreCase)
    {
        ["0"] = 0, ["funds"] = 0, ["fund"] = 0, ["资金"] = 0,
        ["1"] = 1, ["positions"] = 1, ["position"] = 1, ["holdings"] = 1, ["持仓"] = 1, ["股份"] = 1,
        ["2"] = 2, ["orders"] = 2, ["order"] = 2, ["entrust"] = 2, ["委托"] = 2, ["当日委托"] = 2,
        ["3"] = 3, ["fills"] = 3, ["fill"] = 3, ["trades"] = 3, ["成交"] = 3, ["当日成交"] = 3,
        ["4"] = 4, ["cancelable"] = 4, ["cancellable"] = 4, ["可撤"] = 4, ["可撤单"] = 4,
        ["5"] = 5, ["shareholders"] = 5, ["gddm"] = 5, ["股东"] = 5, ["股东代码"] = 5,
        ["6"] = 6, ["7"] = 7, ["8"] = 8, ["9"] = 9,
        ["12"] = 12, ["ipo"] = 12, ["ipo_list"] = 12, ["可申购"] = 12,
        ["13"] = 13, ["ipo_quota"] = 13, ["额度"] = 13,
        ["14"] = 14, ["allotment"] = 14, ["配号"] = 14,
        ["15"] = 15, ["winning"] = 15, ["中签"] = 15,
    };

    private static readonly Dictionary<string, int> History = new(StringComparer.OrdinalIgnoreCase)
    {
        ["0"] = 0, ["orders"] = 0, ["order"] = 0, ["entrust"] = 0, ["委托"] = 0, ["历史委托"] = 0,
        ["1"] = 1, ["fills"] = 1, ["fill"] = 1, ["trades"] = 1, ["成交"] = 1, ["历史成交"] = 1,
        ["2"] = 2, ["delivery"] = 2, ["交割"] = 2, ["交割单"] = 2,
        ["3"] = 3, ["fund"] = 3, ["funds"] = 3, ["fund_detail"] = 3, ["资金明细"] = 3,
        ["4"] = 4, ["statement"] = 4, ["对账"] = 4, ["对账单"] = 4,
    };

    public static bool TryResolveToday(string? alias, out int category, out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(alias))
        {
            category = 0;
            error = "category required (e.g. funds/positions/orders/fills or 0-5)";
            return false;
        }
        if (Today.TryGetValue(alias.Trim(), out category))
            return true;
        if (int.TryParse(alias.Trim(), out category))
            return true;
        error = $"unknown today category '{alias}'. Use funds/positions/orders/fills/cancelable/shareholders or numeric id.";
        return false;
    }

    public static bool TryResolveHistory(string? alias, out int category, out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(alias))
        {
            category = 0; // default historical orders
            return true;
        }
        if (History.TryGetValue(alias.Trim(), out category))
            return true;
        if (int.TryParse(alias.Trim(), out category))
            return true;
        error = $"unknown history category '{alias}'. Use orders/fills/delivery/fund/statement or 0-4.";
        return false;
    }
}
