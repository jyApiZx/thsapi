namespace TradeMcp.Native.Text;

public sealed record TsvTable(IReadOnlyList<string> Headers, IReadOnlyList<IReadOnlyList<string>> Rows)
{
    public static TsvTable Empty { get; } = new([], []);

    public IReadOnlyList<IReadOnlyDictionary<string, string>> AsObjects()
    {
        var list = new List<IReadOnlyDictionary<string, string>>(Rows.Count);
        foreach (var row in Rows)
        {
            var dict = new Dictionary<string, string>(Headers.Count, StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < Headers.Count; i++)
            {
                var key = Headers[i];
                var value = i < row.Count ? row[i] : string.Empty;
                dict[key] = value;
            }
            list.Add(dict);
        }
        return list;
    }
}

public static class Tsv
{
    public static TsvTable Parse(string? text, int maxRows = 50_000)
    {
        if (string.IsNullOrWhiteSpace(text))
            return TsvTable.Empty;

        var lines = text.Replace("\r\n", "\n").Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (lines.Length == 0)
            return TsvTable.Empty;

        var headers = lines[0].Split('\t');
        var rows = new List<IReadOnlyList<string>>(Math.Min(lines.Length - 1, maxRows));
        for (var i = 1; i < lines.Length && rows.Count < maxRows; i++)
            rows.Add(lines[i].Split('\t'));

        return new TsvTable(headers, rows);
    }
}
