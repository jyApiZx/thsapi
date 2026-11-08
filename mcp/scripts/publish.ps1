#Requires -Version 5.1
# Publishes two self-contained win-x64 exes. The commercial DLLs are not bundled;
# users copy each exe into the matching 2.0 test package folder.
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

$quotesOut = Join-Path $root "artifacts\quotes"
$tradingOut = Join-Path $root "artifacts\trading"

Write-Host "Running tests..."
dotnet test "$root\TradeMcp.sln" -c Release --nologo
if ($LASTEXITCODE -ne 0) { throw "tests failed" }

$common = @("-c", "Release", "-r", "win-x64", "--self-contained", "true",
  "-p:PublishSingleFile=true", "-p:IncludeNativeLibrariesForSelfExtract=true",
  "-p:EnableCompressionInSingleFile=true",
  "-p:DebugType=none")

Write-Host "Publishing Quotes..."
Remove-Item -Recurse -Force $quotesOut -ErrorAction SilentlyContinue
dotnet publish "$root\src\TradeMcp.Quotes.Server\TradeMcp.Quotes.Server.csproj" @common -o $quotesOut
if ($LASTEXITCODE -ne 0) { throw "publish quotes failed" }

Write-Host "Publishing Trading..."
Remove-Item -Recurse -Force $tradingOut -ErrorAction SilentlyContinue
dotnet publish "$root\src\TradeMcp.Trading.Server\TradeMcp.Trading.Server.csproj" @common -o $tradingOut
if ($LASTEXITCODE -ne 0) { throw "publish trading failed" }

Get-ChildItem $quotesOut, $tradingOut -File | Where-Object { $_.Extension -ne ".exe" } | Remove-Item -Force

Write-Host "Done."
Write-Host "  $quotesOut\TradeMcp.Quotes.exe"
Write-Host "  $tradingOut\TradeMcp.Trading.exe"
