# Security Model

## 威胁模型（本地 MCP）

- MCP Server 与 AI 客户端运行在同一用户会话。
- 模型可能连续调用工具；「两阶段确认」降低误点，但无法阻止恶意/失控自动化。
- 密码不应进入聊天上下文；使用 Credential Manager 档案。

## 默认安全姿态

1. `TRADE_MCP_TRADING_ENABLED=0`：所有写操作拒绝。
2. `TRADE_MCP_BANK_ENABLED=0`：银证转账单独关闭。
3. 写操作必须 `prepare → confirm`，令牌短 TTL，参数必须字节级一致。
4. 幂等键全局防重放；confirm 后同键不可再 prepare。
5. 数量/金额上限与可选证券白名单。
6. 可选 A 股交易时段校验。
7. 审计日志脱敏（密码字段打码）。
8. 登录优先 `trade_logon_from_prefs`（读本机 `login_prefs.json`）或 `profileName`；避免在对话里传交易密码。`login_prefs.json` 含交易密码，不要外发或提交。
9. 历史查询自动分段时，全量导出文件（`exportPath`）可能含持仓/成交隐私，注意目录权限。

## DLL 完整性

启动时校验：

- 进程必须是 x64
- PE Machine = `0x8664`
- 设置了下面的环境变量时，额外校验 DLL 的 SHA-256（不设置则跳过）：
  - `TRADE_MCP_HQAPI_SHA256`
  - `TRADE_MCP_THSAPI_SHA256`

## 运维建议

- 生产环境用自包含发布目录，勿混入未加壳 DLL。
- 仅为需要下单的会话打开 `TRADING_ENABLED`，用完关闭。
- 不要在共享机器上保存明文 profile；Credential Manager 按 Windows 用户隔离。
- 实盘联调仅人工触发；自动化测试不得默认下单或转账。

## 免责声明

本软件通过券商/行情通道执行指令。使用本 MCP 产生的任何交易结果由操作方自行承担。两阶段协议与限额是工程护栏，不是投资建议，也不能替代合规与风控。
