using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace TradeMcp.Native.Interop;

public static class DllGuard
{
    [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool SetDllDirectory(string path);

    [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr LoadLibrary(string path);

    public static void EnsureX64Process()
    {
        if (!Environment.Is64BitProcess)
            throw new InvalidOperationException("TradeMcp requires a 64-bit process matching the release DLLs.");
    }

    public static string ResolveBesideExe(string fileName)
    {
        var baseDir = AppContext.BaseDirectory;
        var path = Path.Combine(baseDir, fileName);
        if (File.Exists(path)) return path;
        throw new FileNotFoundException($"Native library not found: {fileName}", path);
    }

    public static void VerifySha256(string path, string? expectedHex)
    {
        if (string.IsNullOrWhiteSpace(expectedHex)) return;
        using var stream = File.OpenRead(path);
        var hash = Convert.ToHexString(SHA256.HashData(stream));
        if (!hash.Equals(expectedHex, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"SHA-256 mismatch for {Path.GetFileName(path)}: got {hash}, expected {expectedHex}");
    }

    public static void PrepareDirectory(string dllPath)
    {
        EnsureX64Process();
        var dir = Path.GetDirectoryName(dllPath)!;
        SetDllDirectory(dir);
        var handle = LoadLibrary(dllPath);
        if (handle == IntPtr.Zero)
            throw new InvalidOperationException($"LoadLibrary failed for {dllPath}, Win32={Marshal.GetLastWin32Error()}");
        VerifyPeMachineX64(dllPath);
    }

    public static void VerifyPeMachineX64(string path)
    {
        using var fs = File.OpenRead(path);
        using var br = new BinaryReader(fs);
        if (br.ReadUInt16() != 0x5A4D) throw new InvalidOperationException($"{path} is not a PE file.");
        fs.Seek(0x3C, SeekOrigin.Begin);
        var peOffset = br.ReadInt32();
        fs.Seek(peOffset, SeekOrigin.Begin);
        if (br.ReadUInt32() != 0x00004550) throw new InvalidOperationException($"{path} missing PE signature.");
        var machine = br.ReadUInt16();
        if (machine != 0x8664)
            throw new InvalidOperationException($"{path} is not x64 (Machine=0x{machine:X}).");
    }
}
