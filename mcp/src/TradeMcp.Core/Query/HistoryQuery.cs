using System.Text;
using System.Text.Json;
using TradeMcp.Native.Interop;
using TradeMcp.Native.Text;
using TradeMcp.Native.Trading;

namespace TradeMcp.Core.Query;

public sealed class HistoryQueryResult
{
    public bool Ok { get; init; }
    public string? Error { get; init; }
    public int Category { get; init; }
    public string Start { get; init; } = "";
    public string End { get; init; } = "";
    public int ChunkDays { get; init; }
    public IReadOnlyList<object> Chunks { get; init; } = [];
    public IReadOnlyList<string> Headers { get; init; } = [];
    public IReadOnlyList<IReadOnlyDictionary<string, string>> AllRows { get; init; } = [];
    public IReadOnlyList<IReadOnlyDictionary<string, string>> Rows { get; init; } = [];
    public int TotalCount { get; init; }
    public int Offset { get; init; }
    public int Limit { get; init; }
    public bool TruncatedPage { get; init; }
    public object? Summary { get; init; }
    public string? ExportPath { get; init; }
}

public static class HistoryQuery
{
    public const int DefaultChunkDays = 30;
    public const int DefaultPageLimit = 100;

    public static HistoryQueryResult Execute(
        TradeApi api,
        int clientId,
        int category,
        DateRange range,
        int chunkDays = DefaultChunkDays,
        int offset = 0,
        int limit = DefaultPageLimit,
        bool summaryOnly = false,
        string? exportPath = null)
    {
        if (offset < 0) offset = 0;
        if (limit <= 0) limit = DefaultPageLimit;
        if (chunkDays <= 0) chunkDays = DefaultChunkDays;

        var headers = new List<string>();
        var rows = new List<IReadOnlyDictionary<string, string>>();
        var chunks = new List<object>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var (cs, ce) in range.Chunk(chunkDays))
        {
            var s = cs.ToString("yyyyMMdd");
            var e = ce.ToString("yyyyMMdd");
            var hist = api.QueryHistory(clientId, category, s, e);
            if (!hist.Ok)
            {
                chunks.Add(new { start = s, end = e, ok = false, error = hist.Error, count = 0 });
                // If broker still rejects (e.g. limit lower than chunkDays), surface hard fail when nothing collected yet.
                if (rows.Count == 0 && chunks.Count == 1)
                {
                    return new HistoryQueryResult
                    {
                        Ok = false,
                        Error = hist.Error,
                        Category = category,
                        Start = range.StartText,
                        End = range.EndText,
                        ChunkDays = chunkDays,
                        Chunks = chunks
                    };
                }
                continue;
            }

            var table = hist.Table ?? Tsv.Parse(hist.Text);
            var added = 0;
            if (table.Headers.Count > 0 && headers.Count == 0)
                headers.AddRange(table.Headers);

            foreach (var obj in table.AsObjects())
            {
                var key = RowKey(obj);
                if (!seen.Add(key)) continue;
                rows.Add(obj);
                added++;
            }
            chunks.Add(new { start = s, end = e, ok = true, count = added });
        }

        var summary = BuildSummary(rows);
        string? exported = null;
        if (!string.IsNullOrWhiteSpace(exportPath))
        {
            exported = Export(exportPath!, headers, rows, summary, category, range, chunks);
        }

        IReadOnlyList<IReadOnlyDictionary<string, string>> page = Array.Empty<IReadOnlyDictionary<string, string>>();
        var truncated = false;
        if (!summaryOnly)
        {
            if (offset >= rows.Count)
                page = Array.Empty<IReadOnlyDictionary<string, string>>();
            else
            {
                var take = Math.Min(limit, rows.Count - offset);
                page = rows.GetRange(offset, take);
                truncated = offset + take < rows.Count;
            }
        }

        return new HistoryQueryResult
        {
            Ok = true,
            Category = category,
            Start = range.StartText,
            End = range.EndText,
            ChunkDays = chunkDays,
            Chunks = chunks,
            Headers = headers,
            AllRows = rows,
            Rows = page,
            TotalCount = rows.Count,
            Offset = offset,
            Limit = limit,
            TruncatedPage = truncated,
            Summary = summary,
            ExportPath = exported
        };
    }

    public static object ToPayload(HistoryQueryResult r) => new
    {
        ok = r.Ok,
        error = r.Error,
        category = r.Category,
        start = r.Start,
        end = r.End,
        chunkDays = r.ChunkDays,
        chunks = r.Chunks,
        count = r.TotalCount,
        offset = r.Offset,
        limit = r.Limit,
        truncated = r.TruncatedPage,
        summary = r.Summary,
        exportPath = r.ExportPath,
        headers = r.Ok ? r.Headers : null,
        rows = r.Ok ? r.Rows : null
    };

    private static string RowKey(IReadOnlyDictionary<string, string> row)
    {
        static string Get(IReadOnlyDictionary<string, string> r, params string[] keys)
        {
            foreach (var k in keys)
            {
                foreach (var kv in r)
                {
                    if (kv.Key.Equals(k, StringComparison.OrdinalIgnoreCase) ||
                        kv.Key.StartsWith(k + "_", StringComparison.OrdinalIgnoreCase) ||
                        kv.Key.Contains(k, StringComparison.OrdinalIgnoreCase))
                        return kv.Value ?? "";
                }
            }
            return "";
        }

        var date = Get(row, "委托日期", "成交日期", "发生日期", "日期");
        var time = Get(row, "委托时间", "成交时间", "时间");
        var no = Get(row, "委托编号", "合同编号", "成交编号", "流水号");
        var code = Get(row, "证券代码", "代码");
        var side = Get(row, "操作", "买卖标志", "方向");
        if (!string.IsNullOrEmpty(no) || !string.IsNullOrEmpty(date))
            return string.Join("|", date, time, no, code, side);
        return string.Join("\t", row.Select(kv => kv.Key + "=" + kv.Value));
    }

    private static object BuildSummary(IReadOnlyList<IReadOnlyDictionary<string, string>> rows)
    {
        var byDate = new Dictionary<string, int>(StringComparer.Ordinal);
        var bySide = new Dictionary<string, int>(StringComparer.Ordinal);
        var byCode = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var row in rows)
        {
            var date = Find(row, "委托日期", "成交日期", "发生日期") ?? "";
            var side = Find(row, "操作", "买卖标志") ?? "";
            var code = Find(row, "证券代码") ?? "";
            if (!string.IsNullOrEmpty(date)) byDate[date] = byDate.GetValueOrDefault(date) + 1;
            if (!string.IsNullOrEmpty(side)) bySide[side] = bySide.GetValueOrDefault(side) + 1;
            if (!string.IsNullOrEmpty(code)) byCode[code] = byCode.GetValueOrDefault(code) + 1;
        }

        return new
        {
            total = rows.Count,
            byDate = byDate.OrderBy(kv => kv.Key).Select(kv => new { date = kv.Key, count = kv.Value }),
            bySide = bySide.OrderByDescending(kv => kv.Value).Select(kv => new { side = kv.Key, count = kv.Value }),
            topCodes = byCode.OrderByDescending(kv => kv.Value).Take(20).Select(kv => new { code = kv.Key, count = kv.Value })
        };
    }

    private static string? Find(IReadOnlyDictionary<string, string> row, params string[] names)
    {
        foreach (var name in names)
        {
            foreach (var kv in row)
            {
                if (kv.Key.Equals(name, StringComparison.OrdinalIgnoreCase) ||
                    kv.Key.StartsWith(name + "_", StringComparison.OrdinalIgnoreCase))
                    return kv.Value;
            }
        }
        return null;
    }

    private static string Export(
        string path,
        IReadOnlyList<string> headers,
        IReadOnlyList<IReadOnlyDictionary<string, string>> rows,
        object summary,
        int category,
        DateRange range,
        IReadOnlyList<object> chunks)
    {
        var full = Path.GetFullPath(path);
        var dir = Path.GetDirectoryName(full);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        if (full.EndsWith(".tsv", StringComparison.OrdinalIgnoreCase))
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Join('\t', headers));
            foreach (var row in rows)
            {
                sb.AppendLine(string.Join('\t', headers.Select(h => row.TryGetValue(h, out var v) ? v : "")));
            }
            File.WriteAllText(full, sb.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }
        else
        {
            var payload = new
            {
                ok = true,
                category,
                start = range.StartText,
                end = range.EndText,
                count = rows.Count,
                chunks,
                summary,
                headers,
                rows
            };
            File.WriteAllText(full, JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            }), new UTF8Encoding(false));
        }
        return full;
    }
}
