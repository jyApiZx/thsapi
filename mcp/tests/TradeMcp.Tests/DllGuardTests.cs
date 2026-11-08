using TradeMcp.Native.Interop;

namespace TradeMcp.Tests;

public class DllGuardTests
{
    // The commercial DLLs are not in this repository. Put them under native/ to run these checks.
    private static string NativePath(params string[] parts)
        => Path.GetFullPath(Path.Combine(new[] { AppContext.BaseDirectory, "..", "..", "..", "..", "..", "native" }.Concat(parts).ToArray()));

    [Fact]
    public void VerifyPeMachineX64_HqApi()
    {
        var path = NativePath("quotes", "HqApi.dll");
        if (!File.Exists(path)) return;
        DllGuard.VerifyPeMachineX64(path);
    }

    [Fact]
    public void VerifyPeMachineX64_Thsapi()
    {
        var path = NativePath("trading", "thsapi.dll");
        if (!File.Exists(path)) return;
        DllGuard.VerifyPeMachineX64(path);
    }

    [Fact]
    public void VerifySha256_EmptyExpectationSkips()
    {
        DllGuard.VerifySha256(typeof(DllGuardTests).Assembly.Location, null);
        DllGuard.VerifySha256(typeof(DllGuardTests).Assembly.Location, "");
    }
}
