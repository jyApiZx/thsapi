# thsapi MCP 托管

把 **同花顺交易接口（thsapi.dll）** 和 **通达信行情接口（HqApi.dll）** 封装成两个本地 MCP 服务（MCP 托管），让支持 MCP 的 AI 客户端直接查持仓、看行情、下单。

```text
AI 客户端  --MCP（stdio）-->  TradeMcp.Trading.exe  -->  thsapi.dll
AI 客户端  --MCP（stdio）-->  TradeMcp.Quotes.exe   -->  HqApi.dll
```

本目录只有 MCP 外壳的源码。`thsapi.dll`、`HqApi.dll` 是商业库，不在仓库里；请从 GitHub Release / 官网下载同花顺交易接口与通达信行情接口测试包：<https://thsapi.com/#get>。Python 包见 <https://thsapi.com/python.html>。使用说明：<https://thsapi.com/mcp.html>。

## 支持范围

| 项 | 说明 |
|----|------|
| 系统 | Windows 10 / 11 / Server，**仅 64 位** |
| 运行时 | 发布版是自包含单文件 exe，不用装 .NET |
| 传输 | 本地 stdio，交易和行情各一个服务 |

## 直接使用（不编译）

1. 下载交易测试包（64 位），打开 `xdDemo.exe` 登录一次，生成 `login_prefs.json`。
2. 下载 [MCP 包](https://thsapi.com/downloads/thsapi_mcp_2.0.0_x64.zip)，把 `TradeMcp.Trading.exe` 放进交易测试包目录（和 `thsapi.dll`、`ths.xml`、`login_prefs.json` 同目录）。要行情就把 `TradeMcp.Quotes.exe` 放进行情测试包目录，并先用行情 Demo 连接一次。
3. 在 AI 客户端的 MCP 配置里加上（路径换成你的解压目录）：

```json
{
  "mcpServers": {
    "thsapi-trading": {
      "command": "D:\\thsapi_2.0.0_x64\\TradeMcp.Trading.exe",
      "env": { "TRADE_MCP_TRADING_ENABLED": "0" }
    },
    "thsapi-quotes": {
      "command": "D:\\hqapi_2.0.0_x64\\TradeMcp.Quotes.exe"
    }
  }
}
```

4. 对 AI 说「登录交易账户，查一下资金和持仓」。AI 会调用 `trade_logon_from_prefs`，密码从本机 `login_prefs.json` 读取，不经过聊天。

`login_prefs.json` 含交易密码，不要发给别人，也不要提交到代码仓库。

## 自己编译

需要 .NET 8 SDK。

```powershell
dotnet test TradeMcp.sln -c Release
.\scripts\publish.ps1
```

产物在 `artifacts/trading/TradeMcp.Trading.exe` 和 `artifacts/quotes/TradeMcp.Quotes.exe`。单元测试不需要 DLL，也不会下单。

## 登录方式

| 方式 | 工具 | 说明 |
|------|------|------|
| 推荐 | `trade_logon_from_prefs` / `quote_connect_from_prefs` | 读 Demo 保存的 `login_prefs.json`，信用账户自动加 `X` |
| 档案 | `trade_save_profile` + `trade_logon(profileName)` | 存进 Windows 凭据管理器，之后只传档案名 |
| 手动 | `trade_logon(ip, accountNo, jyPassword, ...)` | 密码会出现在对话里，不推荐 |

行情账号：L1 用授权账号（自动加 `L1*`），密码不能为空；L2 用通达信账号，不加前缀，需单独开通；扩展行情用 `KZ*` 账号。

## 下单开关（默认关闭）

| 环境变量 | 默认 | 含义 |
|----------|------|------|
| `TRADE_MCP_TRADING_ENABLED` | 关 | 允许下单、撤单、还款、基金、打新 |
| `TRADE_MCP_BANK_ENABLED` | 关 | 允许银证转账 |
| `TRADE_MCP_CONFIRM_TTL` | 60 | 确认令牌有效秒数 |
| `TRADE_MCP_MAX_QTY` | 100000 | 单笔最大数量 |
| `TRADE_MCP_MAX_AMOUNT` | 1000000 | 单笔最大金额 |
| `TRADE_MCP_MAX_BANK` | 200000 | 单笔最大转账金额 |
| `TRADE_MCP_ALLOW_SYMBOLS` | 空（不限） | 允许交易的代码，逗号分隔 |
| `TRADE_MCP_ENFORCE_SESSION` | true | 只在 A 股交易时段允许下单 |
| `TRADE_MCP_THSAPI_SHA256` / `TRADE_MCP_HQAPI_SHA256` | 空 | 设置后启动时校验 DLL 哈希 |

每笔写操作分两步：`trade_prepare_*` 返回预览和确认令牌，你看过后 AI 再调 `trade_confirm_*` 提交。这是工程护栏，不能代替券商风控和你自己的判断。详见 [docs/SECURITY.md](docs/SECURITY.md)。

## 工具一览

见 [docs/TOOLS.md](docs/TOOLS.md)。

## 结果格式

- DLL 返回的表格转成 UTF-8 JSON：`{ ok, headers, rows, count, truncated? }`
- 失败：`{ ok:false, error }` 或 `{ ok:false, code, error }`

## 目录

```text
src/TradeMcp.Native          P/Invoke、GBK 与表格解析
src/TradeMcp.Core            会话、凭据、login_prefs、下单护栏
src/TradeMcp.Quotes.Server   行情 MCP
src/TradeMcp.Trading.Server  交易 MCP
tests/                       单元测试（不连券商、不下单）
```

## 许可

见 [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md)。使用本工具产生的交易结果由使用者自行承担。
