# 行情接口：原函数与 Alloc

交付库：客户包内一般为 `HqApi.dll`（内部亦可为 `TdxHqApi.dll`）。`__stdcall`。只支持 Windows。

官网：https://thsapi.com/hq.html （Alloc 见 https://thsapi.com/hq.html#alloc ）  
Python：`tdxhq` → [python-sdk.md](python-sdk.md) 与 https://thsapi.com/python.html

## 版本怎么选

| | Alloc 函数 | 原函数 |
|--|--|--|
| 缓冲 | `TdxHq_*Alloc` + `TdxHq_FreeBuffer` | 调用方预分配 Result（常 1MB）/ ErrInfo（256） |
| 编码 | UTF-8 | GBK |
| 大表 | 不因 1MB 截断 | 可能截断 |
| Python | `tdxhq` SDK（封装 Alloc） | 仓库 `行情/python/demo.py` 直接调用 |

## 通道与账号

| 通道 | 典型端口 | 账号写法 | 函数前缀 |
|------|----------|----------|----------|
| L1 | 7709 | 必须 `L1*账号`，不能 guest/空；密码非空 | `TdxHq_*` |
| L2 | 常见 7719 | 通达信 L2 账号（不要加 `L1*`）；需单独开通 | `TdxHq_*`（十档/队列/逐笔） |
| 扩展 Ex | 7727 / 7721 | 常 `KZ*账号` | `TdxExHq_*` |

A 股连接号不要拿去调扩展行情。授权含 L1 与扩展行情（美股、港股、期货）；L2 另议。

推荐：先用 `行情测试demo.exe` 连接成功，生成 `login_prefs.json`，再 `tdxhq.HqClient.connect_from_prefs(lane=...)`。Python 禁止手填 host/port/账号。

## 市场与 K 线周期

- A 股市场：`0` 深 · `1` 沪 · `2` 北
- K 线 Category / Period：`0` 5 分 · `1` 15 分 · `2` 30 分 · `3` 60 分 · `4` 日 · `5` 周 · `6` 月 · `7` 1 分

## thsapi 要点

- 连接：`TdxHq_ConnectAlloc`（及 Ex 对应 Alloc）→ 返回连接 ID。
- 查询：如 `TdxHq_GetSecurityQuotesAlloc`、`TdxHq_GetSecurityBarsAlloc`、`TdxHq_GetMinuteTimeDataAlloc`、`TdxHq_GetTransactionDataAlloc`、`TdxHq_ListBoardsAlloc` 等。
- 返回堆上 UTF-8 TSV，用完 **`TdxHq_FreeBuffer`**。
- Python SDK 已封装全部 Alloc，业务代码不要直接碰指针。

## 原函数主要导出（调用方分配缓冲区，GBK）

```c
/* A 股 L1 / L2 */
int  __stdcall TdxHq_Connect(char* IP, int Port, char* Account, char* Password, char* Result, char* ErrInfo);
void __stdcall TdxHq_Disconnect(int ConnID);

int  __stdcall TdxHq_GetSecurityCount(int ConnID, int Market, char* Result, char* ErrInfo);
int  __stdcall TdxHq_GetSecurityList(int ConnID, int Market, int Start, char* Result, char* ErrInfo);
int  __stdcall TdxHq_GetSecurityBars(int ConnID, int Category, int Market, char* Code, int Start, int Count, char* Result, char* ErrInfo);
int  __stdcall TdxHq_GetIndexBars(...);
int  __stdcall TdxHq_GetMinuteTimeData(int ConnID, int Market, char* Code, char* Result, char* ErrInfo);
int  __stdcall TdxHq_GetHistoryMinuteTimeData(...);
int  __stdcall TdxHq_GetTransactionData(...);
int  __stdcall TdxHq_GetHistoryTransactionData(...);
int  __stdcall TdxHq_GetSecurityQuotes(int ConnID, /* 市场+代码数组 */, char* Result, char* ErrInfo);
int  __stdcall TdxHq_GetSecurityQuotes10(...);          /* L2 十档 */
int  __stdcall TdxHq_GetBuySellQueue(...);              /* L2 队列 */
int  __stdcall TdxHq_GetDetailTransactionData(...);     /* L2 逐笔成交 */
int  __stdcall TdxHq_GetDetailOrderData(...);           /* L2 逐笔委托 */
int  __stdcall TdxHq_GetCallAuctionData(...);
int  __stdcall TdxHq_GetCompanyInfoCategory(...);
int  __stdcall TdxHq_GetCompanyInfoContent(...);
int  __stdcall TdxHq_GetXDXRInfo(...);
int  __stdcall TdxHq_GetFinanceInfo(...);
int  __stdcall TdxHq_ListBoards(...);
int  __stdcall TdxHq_ListBoardMembers(...);
int  __stdcall TdxHq_GetBlockInfo(...);
int  __stdcall TdxHq_GetMarketStat(...);
int  __stdcall TdxHq_GetFundFlow(...);
int  __stdcall TdxHq_GetReportFile(...);

/* 扩展行情 */
int  __stdcall TdxExHq_Connect(...);
void __stdcall TdxExHq_Disconnect(int ConnID);
int  __stdcall TdxExHq_GetMarkets(...);
int  __stdcall TdxExHq_GetInstrumentCount(...);
int  __stdcall TdxExHq_GetInstrumentInfo(...);
int  __stdcall TdxExHq_GetInstrumentQuote(...);
int  __stdcall TdxExHq_GetInstrumentBars(...);
int  __stdcall TdxExHq_GetMinuteTimeData(...);
int  __stdcall TdxExHq_GetTransactionData(...);

/* thsapi：上述能力对应 *Alloc + TdxHq_FreeBuffer(char*) */
```

原函数的 Result/ErrInfo 由调用方分配；中文 **GBK**。具体参数表以官网 https://thsapi.com/hq.html 为准；不要编造未文档化导出。

## 常用能力对照

- 五档批量、K 线、分时、分笔、集合竞价、板块、成分、涨跌统计、资金流向、F10、除权
- L2：十档、买卖队列、逐笔委托/成交
- Ex：市场列表、品种、行情、K 线、分时

站点列表见测试包演示程序。全市场五档批量在良好网络下约百毫秒级（产品指引 / 首页有实测说明）。

## 注意

- 发布 zip **不要**打入客户 `login_prefs.json`。
- 程序位数与 DLL 一致；优先 64 位。
- **本 Skill 严禁写入内部协议字段、未公开 IP、本机开发路径或逆向细节。**
