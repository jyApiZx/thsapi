# 交易接口：原函数与 Alloc

交付库：`thsapi.dll`（`__stdcall`）。同一 DLL 同时导出两套接口。只支持 Windows。

官网：https://thsapi.com/trade.html （Alloc 见 https://thsapi.com/trade.html#alloc ）  
Python 用户请优先看 [python-sdk.md](python-sdk.md) 与 https://thsapi.com/python.html，不要手填登录参数。

## 版本怎么选

| | Alloc 函数 | 原函数 |
|--|--|--|
| 结果缓冲 | DLL 堆分配，`FreeApiBuffer` 释放 | 调用方预分配，Result 常 1MB，ErrInfo 256 |
| 编码 | 业务结果 **UTF-8** | **GBK** |
| 大表 | 不因 1MB 截断（两融标的等） | 可能截断 |
| 表头 | 带稳定 fieldID（如 `可用金额_2116`） | 开启 `EnableResultColumnIdSuffix(1)` 后带 ID |
| 典型函数 | `QueryDataAlloc`、`SendOrderExAlloc` | `QueryData`、`SendOrder` / `SendOrderEx` |

流程：`Init`/`OpenTdx` → `Logon` → 业务 → `Logoff` → `UnInit`/`CloseTdx`。进程级 Init/Close 各一次。同一 `ClientID` **串行**；不同 ClientID 可并行。

---

## 登录 Logon（两套共用）

```c
int __stdcall Logon(
    char* IP, short Port, char* Config, short YybID,
    char* AccountNo, char* TradeAccount,
    char* JyPassword, char* TxPassword,
    char* ErrInfo);
```

- `Config` 只写需要的键，常见：`{"qs_id":90,"account_type":0}`；华泰客户号 `"account_type":"k"`。
- **模拟炒股**：`qs_id=356`，`trade.10jqka.com.cn:8002`，营业部 `48`；登录账号填手机号 / 同花顺账户 / 股东账户，**资金账号留空**。见 https://thsapi.com/sim.html
- **信用账户（两融）**：登录账号 = `X` + **普通账户**资金账号；资金账号 / 交易账号 = **信用账户**资金账号（不加 X）；通讯密码一般留空。`login_prefs.json` 的 `isCredit` 为 true 时，示例与 Python SDK 会自动给登录账号加 `X`。完整说明见 https://thsapi.com/sim.html
- 多数券商普通账户：资金账号 + 交易密码；通讯密码一般留空。
- **不要问用户猜** IP / 端口 / 营业部 / `qs_id`：查测试包 `ths.xml`，或先让用户在 `xdDemo.exe` 登录生成 `login_prefs.json`。

`account_type` 常见：`0` 资金帐户、`1` 深圳、`2` 上海、`3` 基金、`4` 深圳 B、`5` 上海 B、`k` 客户号。

---

## thsapi 的 Alloc 函数

- 查询：`QueryDataAlloc(ClientID, Category, &Result, &ErrInfo)` → 用完 `FreeApiBuffer(Result)` 与 `FreeApiBuffer(ErrInfo)`。
- 历史：`QueryHistoryDataAlloc(...)`
- 下单：`SendOrderExAlloc(...)` 等 Alloc 变体；同样 `FreeApiBuffer`。
- 结果 UTF-8 TSV：`\n` 行、`\t` 列；按 fieldID 取列，勿依赖中文列名。
- 另有：`GetQuoteAlloc`、`GetShareholderCodesAlloc`、`GetTradableQuantityAlloc`、`CancelOrderExAlloc`、`BankTransferExAlloc`、`RepayAlloc`、`FundOrderAlloc`、`OneClickIpoAlloc`、银证查询 Alloc 等。

---

## 原函数主要导出（调用方分配缓冲区，GBK）

```c
void __stdcall Init(void);
void __stdcall UnInit(void);
/* 亦可能导出 OpenTdx / CloseTdx，与 Init/UnInit 同类 */

int  __stdcall Logon(...);   /* 见上 */
void __stdcall Logoff(int ClientID);
const char* __stdcall IsConnectOK(int ClientID);

int  __stdcall QueryData(int ClientID, int Category, char* Result, char* ErrInfo);
void __stdcall QueryHistoryData(int ClientID, int Category, char* StartDate, char* EndDate, char* Result, char* ErrInfo);
void __stdcall QueryDatas(...);                 /* 批量查询 */
void __stdcall QueryMultiAccountsDatas(...);    /* 多账户 */
void __stdcall GetShareholderCodes(int ClientID, char* Result, char* ErrInfo);

void __stdcall GetQuote(int ClientID, char* Zqdm, char* Result, char* ErrInfo);
void __stdcall GetQuotes(...);
void __stdcall GetMultiAccountsQuotes(...);
int  __stdcall GetTradableQuantity(int ClientID, int Category, int PriceType, char* Gddm, char* Zqdm, float Price, char* Result, char* ErrInfo);
int  __stdcall GetCanBuySell(...);

void __stdcall SendOrder(int ClientID, int Category, int PriceType, char* Gddm, char* Zqdm, float Price, int Quantity, char* Result, char* ErrInfo);
void __stdcall SendOrderEx(int ClientID, int Category, int PriceType, char* Gddm, char* Zqdm, int ExchangeID, float Price, int Quantity, char* Result, char* ErrInfo);
void __stdcall SendOrders(...);
void __stdcall SendMultiAccountsOrders(...);

void __stdcall CancelOrder(int ClientID, char* ExchangeID, char* hth, char* Result, char* ErrInfo);
void __stdcall CancelOrderEx(int ClientID, const char* gddm, const char* ExchangeID, const char* hth, char* Result, char* ErrInfo);
void __stdcall CancelOrders(...);
void __stdcall CancelMultiAccountsOrders(...);

void __stdcall QueryBank(...);
void __stdcall QueryBankBalance(...);
void __stdcall QueryBankFlow(...);
void __stdcall BankTransfer(...);
void __stdcall BankTransferEx(...);
void __stdcall Repay(int ClientID, char* Amount, char* Result, char* ErrInfo);
int  __stdcall OneClickIpo(int ClientID, int DryRun, char* Result, char* ErrInfo);

void __stdcall EnableResultColumnOrder(int enable);
void __stdcall EnableResultColumnIdSuffix(int enable);

typedef void (__stdcall *ThsPushCallback)(int ClientID, int FunctionID, const char* Tsv, int TsvLen, void* UserData);
int  __stdcall SetPushCallback(int ClientID, ThsPushCallback Callback, void* UserData);
```

原函数：Result ≥ 1MB，ErrInfo ≥ 256，内容 **GBK**。查询前建议 `EnableResultColumnIdSuffix(1)`。

---

## QueryData 查询类别

| Category | 含义 |
|----------|------|
| 0 | 资金 |
| 1 | 股份 / 持仓 |
| 2 | 当日委托 |
| 3 | 当日成交 |
| 4 | 可撤单 |
| 5 | 股东代码 |
| 6 | 融资负债 |
| 7 | 融券负债 |
| 8 | 可融资证券 |
| 9 | 实时合约流水（部分券商可能无） |
| 12 | 可申购新股 |
| 13 | 新股申购额度 |
| 14 | 配号 |
| 15 | 中签 |

## QueryHistoryData 类别

| Category | 含义 |
|----------|------|
| 0 | 历史委托 |
| 1 | 历史成交 |
| 2 | 交割单 |
| 3 | 资金明细 |
| 4 | 对账单 |

## 委托种类 Category（SendOrder / SendOrderEx）

| 值 | 含义 |
|----|------|
| 0 | 买入 |
| 1 | 卖出 |
| 2 | 融资买入 |
| 3 | 融券卖出 |
| 4 | 买券还券 |
| 5 | 卖券还款 |
| 6 | 现券还券 |
| 7 | 新股申购（信用） |

## ExchangeID

`0` 深圳 · `1` 上海 · `6` 北交所。须与代码所属市场及股东号一致。

## PriceType

| 值 | 含义 | 市场 |
|----|------|------|
| 0 | 限价 | 沪深北 |
| 1 | 对手方最优 | 沪深 |
| 2 | 本方最优 | 沪深 |
| 3 | 即时成交剩余撤销 | 深 |
| 4 | 最优五档即时成交剩余撤销 | 沪深 |
| 5 | 全额成交或撤销 | 深 |
| 6 | 最优五档即时成交剩余转限价 | 沪 |

## 常用 fieldID

| ID | 含义 |
|----|------|
| 999 | 序号 |
| 2102 | 证券代码 |
| 2103 | 证券名称 |
| 2106 | 股东帐户 |
| 2107 | 股东姓名 |
| 2108 / 2167 | 交易市场 |
| 2109 | 操作 / 买卖方向 |
| 2112 | 资金余额 |
| 2116 | 可用金额 |
| 2126 | 委托数量 |
| 2127 | 委托价格 |
| 2128 | 成交数量 |
| 2135 | 合同编号 / 委托编号（撤单用） |
| 2137 | 银行代码 |
| 2140 | 委托时间 |
| 2185 | 银行帐号 |
| 3630 | 状态 |
| 6000 | 账号类别 |

不同券商中文列名可能不同，**fieldID 稳定**。打开表头后缀后形如 `可用金额_2116`。

---

## 成交回报推送（13 列）

登录后注册一次 `SetPushCallback`。券商有成交时在**独立线程**回调。

- `Tsv` 为 GBK（两版相同）：第一行表头，第二行一条记录。
- **13 列固定顺序**：ClientID、推送类型、证券代码、方向、价格、数量、金额、状态、成交时间、股东代码、成交编号、剩余数量、摘要。
- 状态：`1`→部分成交，`2`→全部成交；其他保留数字。
- `FunctionID` 当前为 `2304`。
- **禁忌**：回调里禁止 `Logoff` / `UnInit` / 重入危险操作；只拷走数据。处理慢时队列约 64 条，更早的会丢。
- 推送不能代替 `QueryData(3)` 当日成交；成交编号不是柜台合同编号，对账请查当日委托。

---

## 安全与授权（公开说明）

- 授权只用资金账户号匹配，**不需要把交易密码发给授权方**。
- 登录时只校验一次账号哈希，通过后业务只连券商交易站。
- 价格与开通：交易接口活动价 299 元/月、2999 元/年；付款后把付款记录 + 资金账号发给 @Thsapi。
- 完整核验：https://thsapi.com/security.html

**本 Skill 严禁写入或推测任何内部协议字段名、逆向细节、未公开服务器地址或本机开发路径。**
