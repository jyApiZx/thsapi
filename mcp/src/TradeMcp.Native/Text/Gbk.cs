using System.Text;

namespace TradeMcp.Native.Text;

public static class Gbk
{
    private static readonly Encoding Enc = Create();

    private static Encoding Create()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        return Encoding.GetEncoding(936);
    }

    public static Encoding Encoding => Enc;

    public static byte[] EncodeZ(string? text)
    {
        var raw = Enc.GetBytes(text ?? string.Empty);
        var buf = new byte[raw.Length + 1];
        Buffer.BlockCopy(raw, 0, buf, 0, raw.Length);
        return buf;
    }

    public static string DecodeZ(byte[] buf)
    {
        var n = Array.IndexOf(buf, (byte)0);
        if (n < 0) n = buf.Length;
        return Enc.GetString(buf, 0, n);
    }

    public static string DecodeUtf8Z(byte[] buf)
    {
        var n = Array.IndexOf(buf, (byte)0);
        if (n < 0) n = buf.Length;
        return System.Text.Encoding.UTF8.GetString(buf, 0, n);
    }
}
