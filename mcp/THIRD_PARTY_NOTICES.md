# Third-Party Notices

## Native libraries (not included)

- `thsapi.dll` / `ths.xml`: commercial trading library. Download the test package from https://thsapi.com/#get ; use is subject to its license.
- `HqApi.dll`: commercial market-data library (exports `TdxHq_*` / `TdxExHq_*`). Same source and terms.

These binaries are not open source and are not part of this repository.

## Open-source dependencies (NuGet)

- ModelContextProtocol (official C# MCP SDK): see package license
- Microsoft.Extensions.Hosting / Logging: MIT
- System.Text.Encoding.CodePages / System.Text.Json: MIT
- xUnit / Microsoft.NET.Test.Sdk: Apache-2.0 / MIT

Run `dotnet list package` for exact versions.
