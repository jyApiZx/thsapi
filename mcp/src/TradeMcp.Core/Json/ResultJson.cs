using System.Text.Json;
using System.Text.Json.Serialization;
using TradeMcp.Native.Interop;

namespace TradeMcp.Core.Json;

public static class ResultJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    public static string From(NativeResult result) => JsonSerializer.Serialize(result.Payload, Options);

    public static string FromObject(object payload) => JsonSerializer.Serialize(payload, Options);

    public static string Error(string code, string message) =>
        FromObject(new { ok = false, code, error = message });
}
