#Requires -Version 5.1
param(
  [Parameter(Mandatory = $true)][string]$Exe
)
# Minimal stdio handshake using newline-delimited JSON-RPC (MCP stdio spec).
$ErrorActionPreference = "Stop"

$psi = New-Object System.Diagnostics.ProcessStartInfo
$psi.FileName = $Exe
$psi.UseShellExecute = $false
$psi.RedirectStandardInput = $true
$psi.RedirectStandardOutput = $true
$psi.RedirectStandardError = $true
$psi.CreateNoWindow = $true
$p = [System.Diagnostics.Process]::Start($psi)

# Drain stderr in background so the process can't fill the pipe.
$stderrJob = Start-Job -ScriptBlock {
  param($sr)
  while (-not $sr.EndOfStream) { [void]$sr.ReadLine() }
} -ArgumentList $p.StandardError

function Send-Line([string]$json) {
  $p.StandardInput.WriteLine($json)
  $p.StandardInput.Flush()
}

function Read-JsonLine {
  $line = $p.StandardOutput.ReadLine()
  if ($null -eq $line) { throw "EOF from server stdout" }
  return $line
}

try {
  Send-Line '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"smoke","version":"1.0"}}}'
  $init = Read-JsonLine
  Write-Host "initialize => $($init.Substring(0, [Math]::Min(300, $init.Length)))"
  Send-Line '{"jsonrpc":"2.0","method":"notifications/initialized"}'
  Send-Line '{"jsonrpc":"2.0","id":2,"method":"tools/list"}'
  $tools = Read-JsonLine
  Write-Host "tools/list => $($tools.Substring(0, [Math]::Min(500, $tools.Length)))..."
  if ($tools -notmatch '"tools"') { throw "tools/list missing tools" }
  Write-Host "SMOKE OK"
}
finally {
  try { if (-not $p.HasExited) { $p.Kill() } } catch {}
  try { Stop-Job $stderrJob -Force; Remove-Job $stderrJob -Force } catch {}
}
