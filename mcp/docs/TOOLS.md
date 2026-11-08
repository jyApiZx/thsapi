# MCP Tools

所有工具返回 JSON 字符串。字段约定见 README。

## Quotes Server (`TradeMcp.Quotes`)

| Tool | 读/写 | 说明 |
|------|-------|------|
| `quote_connect_from_prefs` | 会话 | 推荐。读行情 Demo 保存的 `login_prefs.json`，`lane` 选 L1 / L2 / Ex |
| `quote_save_profile` | 写配置 | 保存行情档案；所有通道都要账号和非空密码 |
| `quote_connect` | 会话 | 用档案名或手动传地址、账号、密码；L1 自动加 `L1*` |
| `quote_disconnect` | 会话 | 断开 |
| `quote_security_count` | 读 | 市场证券数量 |
| `quote_security_list` | 读 | 证券列表分页 |
| `quote_bars` | 读 | K 线；指数/板块用 `index=true` |
| `quote_minute` / `quote_history_minute` | 读 | 分时 |
| `quote_transactions` / `quote_history_transactions` | 读 | 分笔 |
| `quote_quotes` | 读 | 批量五档/十档 |
| `quote_auction` | 读 | 集合竞价 |
| `quote_l2_queue` / `quote_l2_detail` | 读 | L2（需 L2 主机与账号） |
| `quote_boards` / `quote_board_members` | 读 | 板块 |
| `quote_market_stat` | 读 | 涨跌家数 |
| `quote_fund_flow` / `quote_history_fund_flow` | 读 | 资金流 |
| `quote_xdxr` / `quote_finance` / `quote_company_category` | 读 | 除权/财务/公司信息类别 |
| `ex_*` | 读 | 扩展行情（期货/港美等） |

**Market：** `0` 深 / `1` 沪 / `2` 北。  
**K 线 Category：** `0`–`3` 分钟，`4` 日，`5` 周，`6` 月。  
**同一 connectionId 串行**；不同 id 可并行。

## Trading Server (`TradeMcp.Trading`)

### 会话与查询

| Tool | 说明 |
|------|------|
| `trade_logon_from_prefs` | 推荐。读 xdDemo 保存的 `login_prefs.json` 登录，信用账户自动加 `X` |
| `trade_save_profile` | 保存交易档案到 Windows 凭据管理器 |
| `trade_logon` | 用 `profileName` 或手动传参；返回 `clientId` + `status` + 重连提示 |
| `trade_logoff` / `trade_status` | 登出 / 状态（非「正常」时建议重登） |
| `trade_query` | 当日查询；category 支持别名 `funds/positions/orders/fills/...` |
| `trade_query_history` | 历史查询（**自动 ≤30 天分段合并**） |
| `trade_shareholders` | 股东号 |
| `trade_quote` | 券商五档 |
| `trade_tradable_qty` | 可买可卖 |
| `trade_query_bank*` | 银行列表/余额/流水 |
| `trade_drain_push` | 取出成交等推送缓冲 |
| `trade_options` | 当前护栏配置 |

### `trade_query_history` 参数

| 参数 | 默认 | 说明 |
|------|------|------|
| `category` | `orders` | `orders/fills/delivery/fund/statement` 或 `0-4` |
| `startDate`/`endDate` | 空 | `yyyyMMdd`；都不传则默认近 30 天 |
| `days` / `months` | 空 | 相对结束日的便捷窗口（如 `months=2`） |
| `chunkDays` | `30` | 券商常见上限；工具自动切段合并去重 |
| `offset` / `limit` | `0` / `100` | 明细分页，避免撑爆上下文 |
| `summaryOnly` | `false` | 只返回按日/方向/标的统计 |
| `exportPath` | 空 | 写全量 `.json` 或 `.tsv`，响应里带回路径 |

示例：近两个月历史委托摘要 + 导出：

```text
trade_query_history(clientId, category="orders", months=2, summaryOnly=true,
  exportPath="orders.json")
```

### 写操作（需启用 + 两阶段）

| Prepare | Confirm | 动作 |
|---------|---------|------|
| `trade_prepare_order` | `trade_confirm_order` | 下单 `SendOrderEx` |
| `trade_prepare_cancel` | `trade_confirm_cancel` | 撤单 |
| `trade_prepare_bank_transfer` | `trade_confirm_bank_transfer` | 银证（另需 `BANK_ENABLED`） |
| `trade_prepare_repay` | `trade_confirm_repay` | 还款 |
| `trade_prepare_fund_order` | `trade_confirm_fund_order` | 场内/ETF |
| `trade_prepare_ipo` | `trade_confirm_ipo` | 一键打新（先 `dryRun=true`） |

确认令牌绑定：动作名、幂等键、规范化 JSON 参数、短 TTL。银证密码不进入令牌，confirm 时重新传入。
