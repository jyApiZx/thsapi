# 同花顺交易接口与通达信行情接口

**同花顺交易接口、通达信交易接口、通达信行情接口**（thsapi / tdxapi）。交易库 `thsapi.dll` 是同花顺交易接口，直接连接券商交易服务器，不用打开同花顺客户端，支持所有券商实盘（普通账户与融资融券）。行情库 `HqApi.dll` 是通达信行情接口，走通达信行情协议：A 股用 `TdxHq_*`，期货、美股、港股等扩展行情用 `TdxExHq_*`。股票交易 API / 量化交易接口可接 Python、C++、C#、Java，32 位和 64 位都有，程序位数要和 DLL 一致。

官网与产品指引：[thsapi.com](https://thsapi.com/) · [产品指引](https://thsapi.com/guide.html) · [软件安全核验](https://thsapi.com/security.html) · [授权价格](https://thsapi.com/#license)。联系 [Telegram：@Thsapi](https://t.me/Thsapi)。

## 功能

- 同花顺交易接口：登录后查询资金、持仓、委托和成交，再下单、撤单。成交通过 `SetPushCallback` 推送，不用轮询。
- 支持融资融券、新股申购、场内基金和银证转账；单账户或多账户同时在线；同一账户可批量查询、下单和撤单。
- 通达信行情接口：L1 五档、Tick、分时和 K 线；扩展行情含美股、港股、期货等。交易连接上也可以取五档快照。
- 常用导出：`Logon`、`QueryData`、`SendOrder`。结果为 GBK，由调用方预分配缓冲区。带 `Alloc` 后缀的函数结果为 UTF-8，用完必须释放，见 [交易文档](https://thsapi.com/trade.html#alloc) · [行情文档](https://thsapi.com/hq.html#alloc)。
- Python 包（`thsapi` / `tdxhq`）在同一 DLL 上封装登录和释放，见 [Python 快速开始](https://thsapi.com/python.html)。
- **MCP 托管**：把同花顺交易接口与通达信行情接口交给常用 AI 客户端，用一句话查持仓、看行情、下单。下单默认关闭，每笔先预览、确认后才提交。说明见 [MCP 托管](https://thsapi.com/mcp.html)。

这是把已有策略接到券商上的同花顺交易接口 / 通达信行情接口。

## 下载

测试包在 GitHub Release（与官网首页一致）。DLL 不在 git 仓库里。

### 交易 DLL（同花顺交易接口 `thsapi.dll`）

包内是 `thsapi.dll`、`xdDemo.exe`。位数与程序一致，优先 64 位。

- 最新版 1.0.7.13：[64 位](https://github.com/jyApiZx/thsapi/releases/download/v1.0.7.13/thsapi_1.0.7.13_x64.zip) · [32 位](https://github.com/jyApiZx/thsapi/releases/download/v1.0.7.13/thsapi_1.0.7.13_x32.zip)
- 稳定版 1.0.7.12：[64 位](https://github.com/jyApiZx/thsapi/releases/download/v1.0.7.12/thsapi_1.0.7.12_x64.zip) · [32 位](https://github.com/jyApiZx/thsapi/releases/download/v1.0.7.12/thsapi_1.0.7.12_x32.zip)

### 行情 DLL（通达信行情接口 `HqApi.dll`）

L1 端口 7709，账号写成 `L1*账号`。含扩展行情（美股、港股、期货）。

- 1.0.7.0：[64 位（推荐）](https://github.com/jyApiZx/thsapi/releases/download/hqapi-1.0.7.0/hqapi_1.0.7.0_x64.zip) · [32 位](https://github.com/jyApiZx/thsapi/releases/download/hqapi-1.0.7.0/hqapi_1.0.7.0_x32.zip)

### Python 包

先下载上面的交易 / 行情测试包，用 Demo 登录一次自己的券商；再装 wheel（不上 PyPI）。

**方式一：命令安装**

```bash
pip install https://github.com/jyApiZx/thsapi/releases/download/python-2.0.0/thsapi-2.0.0-py3-none-any.whl https://github.com/jyApiZx/thsapi/releases/download/python-2.0.0/tdxhq-2.0.0-py3-none-any.whl
```

**方式二：下载安装**

- [下载 thsapi](https://github.com/jyApiZx/thsapi/releases/download/python-2.0.0/thsapi-2.0.0-py3-none-any.whl)
- [下载 tdxhq](https://github.com/jyApiZx/thsapi/releases/download/python-2.0.0/tdxhq-2.0.0-py3-none-any.whl)

下载后在文件所在目录执行：`pip install thsapi-2.0.0-py3-none-any.whl tdxhq-2.0.0-py3-none-any.whl`。必须用 `login_from_prefs()` / `connect_from_prefs()`。详见 [Python 快速开始](https://thsapi.com/python.html)。

## 调用示例

网页示例：交易 [Python](https://thsapi.com/交易/Python3.html) · [C++](https://thsapi.com/交易/C++.html) · [C#](https://thsapi.com/交易/Csharp.html) · [Java](https://thsapi.com/交易/JAVA.html)。行情 [Python](https://thsapi.com/行情/Python3.html) · [C++](https://thsapi.com/行情/C++.html) · [C#](https://thsapi.com/行情/Csharp.html) · [Java](https://thsapi.com/行情/JAVA.html)。

本仓库源码：交易 [C++](交易/cpp/Demo.cpp) · [C#](交易/csharp/Demo.cs) · [Python](交易/python/demo.py) · [Java](交易/java/Demo.java)。行情 [C++](行情/cpp/Demo.cpp) · [C#](行情/csharp/Demo.cs) · [Python](行情/python/demo.py) · [Java](行情/java/Demo.java)。

文档：[同花顺交易接口](https://thsapi.com/trade.html) · [通达信行情接口](https://thsapi.com/hq.html) · [Python 包](https://thsapi.com/python.html) · [产品指引](https://thsapi.com/guide.html) · [MCP 托管](https://thsapi.com/mcp.html) · [软件安全核验](https://thsapi.com/security.html)

## 环境与模拟账户

- Windows 10、Windows 11，或同等的 Windows Server。32 位和 64 位都可以，程序位数必须和 DLL 一致。优先使用 64 位。
- Python、Java、C#、C/C++ 按对应位数调用。
- `xdDemo.exe` 需要 .NET Framework 4.8，Windows 10/11 通常已经带有。
- Linux 不在标准包内，`.so` 单独约定，见官网。

初次使用可以先用同花顺模拟账户。登录账号填手机号、同花顺账户或股东账户，三选一，资金账号留空。通讯密码留空。先打开 `xdDemo.exe` 登录成功，再写进自己的程序。填法看 [模拟账户.md](模拟账户.md)，网页说明见 [账户怎么填](https://thsapi.com/sim.html)。

## 安全

授权按账户开通，不需要提供交易密码。登录时只发送经过哈希处理的账号标识。授权服务器只保存这个哈希值和使用时间，不接收交易密码、持仓、委托或成交。校验通过后，程序只连接券商交易站点或行情站点。普通账户和信用账户是两个账户，各自授权。

DLL 只在程序调用时工作，读不到策略源码。银证转账受券商和银行的三方存管限制，资金只能在证券账户和已绑定的本人同名银行账户之间划转。程序仍会执行你发出的下单指令，交易密码请自己保管，并先用模拟账户把流程跑通。

可以自己核对：

- 用资源监视器或 `netstat` 看这个进程连了哪些地址。
- 在 Windows 防火墙里限制程序能访问的地址，只放行券商交易或行情站点。授权只在登录时校验一次。
- 先在单独目录里用模拟账户登录、查询、下单和撤单，再接到正式策略上。

完整说明见 [软件安全核验 / 账户安全与授权](https://thsapi.com/security.html)。

## 速度

下面是单次调用从发出到返回的往返时间，不是平均值。交易在同花顺模拟站点实测，行情在 L1 站点实测。不同网络、不同券商站点，速度会有差异。

交易接口 `thsapi.dll`：下单 `SendOrder` 45 毫秒。查询资金 `QueryData(0)` 38 毫秒，查询股份 `QueryData(1)` 40 毫秒，查询当日委托 `QueryData(2)` 36 毫秒，查询股东代码 `QueryData(5)` 42 毫秒。直连券商交易服务器，中间没有客户端。券商不限速时，批量下单约 200 单/秒。成交回报由券商主动推送。

行情接口 `HqApi.dll`：单只五档 `GetSecurityQuotes` 33 毫秒，K 线 `GetSecurityBars` 32 毫秒，分时 `GetMinuteTimeData` 33 毫秒，集合竞价 `GetCallAuctionData` 35 毫秒，板块 `ListBoards` 41 毫秒，F10 `GetCompanyInfoContent` 48 毫秒。一次调用批量函数拉取全市场五档，5224 只全部返回，含数据解码，185 毫秒取回。

同一账户的调用请串行。批量接口可以一次提交多笔，实际速度以券商限制为准。

## 常见问题

**找不到 DLL。** 把 `thsapi.dll` 或 `HqApi.dll` 放在程序目录，文件名保持小写，并确认 32/64 位和程序一致。`xdDemo.exe`、`ths.xml`、`thsapi.dll` 要放在同一目录。

**登录返回 -1。** 看 `ErrInfo`。先核对接的地址、端口、账号和密码。模拟盘用 `trade.10jqka.com.cn:8002`，`{"qs_id":356,"account_type":0}`，营业部 `48`。同一组参数先在 `xdDemo.exe` 里登录。

**Config 要写满吗。** 不用。只写 `qs_id` 和 `account_type`，空项不要写进去。

**授权服务器会看到密码和持仓吗。** 不会。只上传账号哈希，服务端只保存哈希值和使用时间。

**资金能转到别人的账户吗。** 银证转账只能在证券账户和已绑定的本人同名银行账户之间进行。这不能代替你自己保管密码。

**行情账号怎么填。** L1 写成 `L1*账号`。扩展行情用 `TdxExHq_*`。

**返回乱码。** 原函数的 `Result` 和 `ErrInfo` 是 GBK；Alloc 函数的结果是 UTF-8。`Result` 以换行分行、以制表符分列。

## 接口功能概览

以下接口介绍按原函数名书写。同名的 Alloc 函数（如 `QueryDataAlloc`、`TdxHq_GetSecurityQuotesAlloc`），结果为 UTF-8，用完调用 `FreeApiBuffer` / `TdxHq_FreeBuffer`，详见 [交易文档](https://thsapi.com/trade.html#alloc) 和 [行情文档](https://thsapi.com/hq.html#alloc)。

| 类别 | 说明 |
| --- | --- |
| **同花顺交易接口** | 登录、查询、下单、撤单、打新、场内基金等；`Result` 为 GBK TSV |
| **通达信行情接口** | `TdxHq_*` A 股 L1 行情、`TdxExHq_*` 扩展行情；L1 常见端口 `7709`，扩展常见 `7727` / `7721` |
| **同花顺行情接口** | 交易会话上的证券**五档快照**（`GetQuote` 等），与交易共用 `ClientID` |

进程位数须与 `thsapi.dll`、`HqApi.dll` 一致（x86 或 x64）。除连接类接口外，成功返回多为 `true` 或 `0`；`Result`、`ErrInfo` 为 **GBK** 文本，`Result` 行以 `\n` 分隔、列以 `\t` 分隔。

---

## 同花顺交易接口 API 介绍

典型流程：`Init` → `Logon` → 查询 / 下单 → `Logoff` → `UnInit`。

`Logon` 的 `Config` 只含 `qs_id` 和 `account_type`。`qs_id` 是券商 ID，模拟炒股为 `356`，须与登录地址是同一家。`account_type`：`0` 资金帐户，`1` 深圳，`2` 上海，`3` 基金，`4` 深圳Ｂ股，`5` 上海Ｂ股，`k` 客户号。模拟站点示例：`{"qs_id":356,"account_type":0}`，地址 `trade.10jqka.com.cn`，端口 `8002`，营业部 `48`。

### 会话与查询

| API | 用途 |
| --- | --- |
| `Logon` | 登录券商；成功返回 `ClientID`（≥0），失败 -1 |
| `Logoff` | 注销指定 `ClientID` |
| `QueryData` | 当日数据：资金、持仓、委托、成交、可撤单、股东代码、新股等 |
| `QueryHistoryData` | 历史委托、历史成交、交割单、资金明细、对账单（日期 `yyyyMMdd`） |
| `QueryDatas` | 同一账户、多 `Category` 批量查询 |
| `QueryMultiAccountsDatas` | 多账户批量查询 |
| `SetPushCallback` | 登录成功后注册成交回报；空回调表示注销 |

登录成功后调用一次 `SetPushCallback`。券商有推送时，接口在单独线程里调用回调。文本是 GBK，第一行表头，第二行一条记录。列依次是 `ClientID`、`推送类型`、`证券代码`、`方向`、`价格`、`数量`、`金额`、`状态`、`成交时间`、`股东代码`、`成交编号`、`剩余数量`、`摘要`。指针只在回调返回前有效。`FunctionID` 当前为 2304。返回 0 成功，-1 表示 `ClientID` 无效。

推送类型有 `成交`、`公告`、`登录`、`系统状态`。成交的方向：`1` 为 `买入`，`2` 为 `卖出`。成交的状态：`1` 为 `部分成交`，`2` 为 `全部成交`，其他状态保留数字。分笔成交会逐条回调。数量、价格、金额按券商原文。剩余数量在纯数字且能被 100 整除时除以 100，按股给出，否则保留原文。

这是通知，不能代替 `QueryData` 的当日成交。推送里的成交编号不是柜台合同编号。回调里可以再查 `QueryData`，不要在回调里 `Logoff` 或 `UnInit`。处理得慢时，还没送出的推送最多留 64 条，更早的会被丢掉。C#、Python、Java 要自己留住回调对象，释放前先注销。

### 下单与撤单

| API | 用途 |
| --- | --- |
| `SendOrder` | 下单（买卖、融资融券、新股申购等） |
| `SendOrders` | 同一账户批量下单 |
| `SendMultiAccountsOrders` | 多账户批量下单 |
| `CancelOrder` | 撤单（交易所 + 委托编号） |
| `CancelOrders` | 同一账户批量撤单 |
| `CancelMultiAccountsOrders` | 多账户批量撤单 |

### 其它

| API | 用途 |
| --- | --- |
| `GetTradableQuantity` | 可交易数量（可买 / 可卖） |
| `GetCanBuySell` | 可买可卖数量查询 |
| `GetShareholderCodes` | 股东代码 |
| `Repay` | 融资融券直接还款 |
| `OneClickIpo` | 一键打新 |
| `FundOrder` | 场内基金 / ETF 申购赎回 |
| `EnableResultColumnOrder` | 按列序重排 `Result`（可选） |
| `EnableResultColumnIdSuffix` | 表头附带字段 ID 后缀（可选） |

---

## 同花顺行情接口

通过**已登录**的交易连接取证券五档快照，无需单独行情 `ConnectionID`。

| API | 用途 |
| --- | --- |
| `GetQuote` | 单只证券五档行情（代码可带 `sz` / `sh` / `bj` 前缀） |
| `GetQuotes` | 同一账户批量五档 |
| `GetMultiAccountsQuotes` | 多账户批量五档 |

---

## 通达信行情接口

A 股：`TdxHq_Connect` → 各类查询 → `TdxHq_Disconnect`。扩展行情使用 `TdxExHq_*`，**连接号不要与 A 股混用**。市场：`0` 深圳，`1` 上海，`2` 北交所。

### 连接

| API | 用途 |
| --- | --- |
| `TdxHq_Connect` | 连接 A 股行情服务器 |
| `TdxHq_Disconnect` | 断开 A 股连接 |
| `TdxHq_LoadHostCfg` | 从本机通达信目录读取站点（Channel：1 L1，2 L2，3 扩展） |

### 列表与 K 线、分时、分笔

| API | 用途 |
| --- | --- |
| `TdxHq_GetSecurityCount` | 指定市场证券数量 |
| `TdxHq_GetSecurityList` | 证券列表 |
| `TdxHq_GetSecurityBars` | 个股 / 基金 K 线 |
| `TdxHq_GetIndexBars` | 指数 K 线（含涨跌家数） |
| `TdxHq_GetMinuteTimeData` | 当日分时 |
| `TdxHq_GetHistoryMinuteTimeData` | 历史分时 |
| `TdxHq_GetTransactionData` | 当日分笔成交 |
| `TdxHq_GetHistoryTransactionData` | 历史分笔成交 |

### 快照、资料、板块、资金

| API | 用途 |
| --- | --- |
| `TdxHq_GetSecurityQuotes` | 五档行情（批量，每包最多 80 只） |
| `TdxHq_GetCompanyInfoCategory` | 公司信息目录 |
| `TdxHq_GetCompanyInfoContent` | 公司信息正文 |
| `TdxHq_GetXDXRInfo` | 除权除息 |
| `TdxHq_GetFinanceInfo` | 财务简表 |
| `TdxHq_GetCallAuctionData` | 集合竞价 |
| `TdxHq_GetBlockInfo` | 板块文件（概念等） |
| `TdxHq_ListBoards` | 板块列表 |
| `TdxHq_ListBoardMembers` | 板块成分 |
| `TdxHq_GetMarketStat` | 全市场涨跌统计 |
| `TdxHq_GetFundFlow` | 当日资金流向 |
| `TdxHq_GetHistoryFundFlow` | 历史资金流向 |
| `TdxHq_GetReportFile` | 下载报表文件到本地 |

### L2

DLL 中仍保留下列导出。L2 定制联系 [Telegram：@Thsapi](https://t.me/Thsapi)。

| API | 用途 |
| --- | --- |
| `TdxHq_GetSecurityQuotes10` | 十档盘口 |
| `TdxHq_GetBuySellQueue` | 买卖队列（价、量、笔数、队列） |
| `TdxHq_GetDetailTransactionData` | 逐笔成交明细 |
| `TdxHq_GetDetailOriginalTransactionData` | 逐笔成交（含买卖方委托号） |
| `TdxHq_GetDetailOrderData` | 逐笔委托 |

### 扩展行情 `TdxExHq_*`

| API | 用途 |
| --- | --- |
| `TdxExHq_Connect` | 连接扩展行情（常见端口 `7727` / `7721`） |
| `TdxExHq_Disconnect` | 断开扩展行情 |
| `TdxExHq_GetMarkets` | 扩展市场列表 |
| `TdxExHq_GetInstrumentCount` | 合约数量 |
| `TdxExHq_GetInstrumentInfo` | 合约信息 |
| `TdxExHq_GetInstrumentBars` | 合约 K 线 |
| `TdxExHq_GetMinuteTimeData` | 当日分时 |
| `TdxExHq_GetHistoryMinuteTimeData` | 历史分时 |
| `TdxExHq_GetTransactionData` | 分笔成交 |
| `TdxExHq_GetHistoryTransactionData` | 历史分笔 |
| `TdxExHq_GetInstrumentQuote` | 合约快照行情 |

更细的参数与 `Result` 列说明见 [HQ_API.md](HQ_API.md)。

---

交易示例已填同花顺模拟炒股站点。账号换成手机号、同花顺账户或股东账户，资金账号留空，密码换成模拟交易密码，通讯密码多数留空。语言导航在上面的 [调用示例](#调用示例)。
