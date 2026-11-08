namespace TradeMcp.Native.Interop;

public static class NativeBuffers
{
    public const int ErrCap = 512;
    public const int ResultCap = 8 * 1024 * 1024;

    public static byte[] Result() => new byte[ResultCap];
    public static byte[] Err() => new byte[ErrCap];
}
