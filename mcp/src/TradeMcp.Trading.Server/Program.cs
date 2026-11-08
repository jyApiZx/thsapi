using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using TradeMcp.Core.Security;
using TradeMcp.Core.Sessions;
using TradeMcp.Native.Trading;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);
builder.Logging.SetMinimumLevel(LogLevel.Information);

TradeApi.Prepare(Environment.GetEnvironmentVariable("TRADE_MCP_THSAPI_SHA256"));

var options = TradeOptionsLoader.FromEnvironment();
builder.Services.AddSingleton(options);
builder.Services.AddSingleton<TradeGuardrails>();
builder.Services.AddSingleton<TradeSessionManager>();
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

var app = builder.Build();
var log = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("TradeMcp.Trading");
log.LogInformation("TradingEnabled={Enabled} BankTransferEnabled={Bank}", options.TradingEnabled, options.BankTransferEnabled);
await app.RunAsync();
