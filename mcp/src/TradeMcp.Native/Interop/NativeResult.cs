using System.Text.Json.Serialization;
using TradeMcp.Native.Text;

namespace TradeMcp.Native.Interop;

public sealed class NativeResult
{
    public bool Ok { get; init; }
    public string? Error { get; init; }
    public string? Text { get; init; }
    public int? Id { get; init; }
    public int? Count { get; init; }
    public bool Truncated { get; init; }
    public TsvTable? Table { get; init; }

    [JsonIgnore]
    public object Payload => new
    {
        ok = Ok,
        error = Error,
        id = Id,
        count = Count,
        truncated = Truncated,
        headers = Table?.Headers,
        rows = Table?.AsObjects(),
        text = Table is null ? Text : null
    };

    public static NativeResult Success(string text, int? id = null, int? count = null, int maxRows = 50_000)
    {
        var table = Tsv.Parse(text, maxRows);
        var truncated = !string.IsNullOrEmpty(text) && table.Rows.Count >= maxRows;
        return new NativeResult
        {
            Ok = true,
            Text = text,
            Id = id,
            Count = count ?? table.Rows.Count,
            Truncated = truncated,
            Table = table.Headers.Count > 0 ? table : null
        };
    }

    public static NativeResult Fail(string error) => new() { Ok = false, Error = error };
}
