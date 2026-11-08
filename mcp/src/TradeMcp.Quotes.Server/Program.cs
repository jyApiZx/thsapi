using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using TradeMcp.Core.Sessions;
using TradeMcp.Native.Quotes;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);
builder.Logging.SetMinimumLevel(LogLevel.Information);

QuoteApi.Prepare(Environment.GetEnvironmentVariable("TRADE_MCP_HQAPI_SHA256"));

builder.Services.AddSingleton<QuoteSessionManager>();
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

var app = builder.Build();
await app.RunAsync();
