# Python SDK 参考（thsapi / tdxhq 2.0.0）

官网：https://thsapi.com/python.html  
Skill 入口：先读上级 [SKILL.md](../SKILL.md)。

**硬性约定**

- Python 用户禁止手填 host / port / yyb / qs_id / config；必须 `login_from_prefs()` / `connect_from_prefs()`。
- 缺 `login_prefs.json` 时，引导用户先跑 `xdDemo.exe`（交易）或 `行情测试demo.exe`（行情），登录自己的券商或模拟盘一次。
- 程序位数必须与 DLL 一致（优先 64 位）。
- 单元格默认是字符串；金额优先 `Decimal`。
- 查询成功前不要发实盘单；推送回调里禁止 `logout()` / `close()`。

---

## 安装

```bash
# 解压 测试包后（免翻墙，最稳）
pip install thsapi-2.0.0-py3-none-any.whl tdxhq-2.0.0-py3-none-any.whl

# 可选 DataFrame
pip install "thsapi[pandas]" "tdxhq[pandas]"

# 官网 / GitHub Releases 直链亦可
pip install https://thsapi.com/whl/thsapi-2.0.0-py3-none-any.whl https://thsapi.com/whl/tdxhq-2.0.0-py3-none-any.whl
```

Wheel **不含** DLL。把 `thsapi.dll` / `HqApi.dll` 与 `login_prefs.json` 放在工作目录，或设环境变量 `THSAPI_DLL` / `TDXHQ_DLL`。

---

## thsapi（交易）

### login_prefs.json

由 `xdDemo.exe` 登录成功后自动生成，通常与 DLL 同目录。

查找顺序：`prefs_path` 参数 → DLL 同目录 → 当前目录 → 主脚本目录。

主要字段：`serverIp`、`serverPort`、`yybId`、`account`、`tradeAccount`、`jyPassword`、`txPassword`、`isCredit`、`config`（含 `qs_id` / `account_type`）、`lastAccount`。

```python
api.login_from_prefs()                 # 用 lastAccount
api.login_from_prefs(account="客户号")  # 多账号切换
```

**发布包勿打入客户 login_prefs.json。**

### ApiResponse

统一返回结构（`dict` 子类）：

| 属性 | 类型 | 说明 |
|------|------|------|
| `ok` | bool | 是否成功 |
| `error` | str\|None | 失败信息 |
| `code` | int | 底层返回码 |
| `data` | dict | 业务数据 |
| `rows` | list[dict] | 以 fieldID / 英文列为键的行 |
| `columns` | list[dict] | 列元数据（含 id、name） |
| `rows_by_name` | list[dict] | 以中文列名为键 |
| `to_df()` | DataFrame | 需 pandas |
| `raise_for_status()` | self | 失败抛 `ThsApiError` |

下单成功时 `data["order_id"]` 为合同编号（字段 2135）。

### Client 构造

```python
Client(dll_path=None, *, raise_on_error=False, column_order=True)
```

- `dll_path`：显式路径；否则 `THSAPI_DLL` / 当前目录 / 自动发现
- `raise_on_error=True`：业务失败直接抛异常
- `column_order=True`：打开列序重排（推荐）
- 支持 `with Client(...) as api:`；进程级 `OpenTdx` 引用计数，多实例共用

### Client 方法签名

| 方法 | 说明 |
|------|------|
| `login_from_prefs(prefs_path=None, account=None) -> ApiResponse` | 读 Demo 配置登录（首选） |
| `login(host, port, account, password, *, trade_account=None, tx_password="", yyb_id=0, credit=False, config=None)` | 仅非 Python 迁移或调试时用；Python 业务代码禁止手填 |
| `logout() -> ApiResponse` | 登出 |
| `close()` | 登出并释放进程级引用 |
| `connection_status() -> ApiResponse` | `data["status"]` 连接状态文案 |
| `query(category: QueryCategory\|int) -> ApiResponse` | 当日表 |
| `query_history(category, start_date, end_date) -> ApiResponse` | 历史；日期 `yyyyMMdd` / `date` / `datetime` |
| `shareholders() -> ApiResponse` | 股东代码并缓存（下单自动选用） |
| `quote(symbol) -> ApiResponse` | 交易通道五档快照 |
| `tradable_qty(symbol, price, *, side=BUY, price_type=LIMIT, gddm=None, exchange=None)` | 可买可卖数量；`data["quantity"]` |
| `order(symbol, price, quantity, *, side=BUY, price_type=LIMIT, exchange=None, gddm=None)` | 下单；自动推断市场与股东号 |
| `cancel(order_id, *, symbol=None, exchange=None, gddm=None)` | 撤单；需 `exchange` 或 `symbol` |
| `bank_list()` / `bank_balance()` / `bank_flow(start, end)` | 银证 |
| `bank_transfer(direction, amount, *, bank_code, bank_account, bank_pwd="", fund_pwd="", money_type="R", gddm="")` | 银证转账 |
| `repay(amount)` | 信用直接还款 |
| `fund_order(category, symbol, amount, *, exchange=None, gddm=None)` | 场内基金 / ETF |
| `one_click_ipo(*, dry_run=False)` | 一键打新；`dry_run=True` 只演练 |
| `set_push_callback(fn\|None)` | 成交推送；`fn(dict)` 在推送线程调用 |

### QueryCategory（当日）

| 常量 | 值 | 含义 |
|------|----|------|
| `FUND` | 0 | 资金 |
| `POSITIONS` | 1 | 持仓 |
| `ORDERS` | 2 | 当日委托 |
| `TRADES` | 3 | 当日成交 |
| `CANCELABLE` | 4 | 可撤单 |
| `SHAREHOLDERS` | 5 | 股东代码 |
| `MARGIN_DEBT_FIN` | 6 | 融资负债 |
| `MARGIN_DEBT_SEC` | 7 | 融券负债 |
| `MARGIN_FIN_TARGETS` | 8 | 可融资证券 |
| `MARGIN_CONTRACTS` | 9 | 实时合约流水（部分券商可能无） |
| `IPO_LIST` | 12 | 可申购新股 |
| `IPO_QUOTA` | 13 | 新股申购额度 |
| `IPO_LOTTERY_NO` | 14 | 配号 |
| `IPO_WON` | 15 | 中签 |

### HistoryCategory

| 常量 | 值 | 含义 |
|------|----|------|
| `ORDERS` | 0 | 历史委托 |
| `TRADES` | 1 | 历史成交 |
| `DELIVERY` | 2 | 交割单 |
| `CASH_FLOW` | 3 | 资金明细 |
| `STATEMENT` | 4 | 对账单 |

### OrderSide

| 常量 | 值 | 含义 |
|------|----|------|
| `BUY` | 0 | 买入 |
| `SELL` | 1 | 卖出 |
| `MARGIN_BUY` | 2 | 融资买入 |
| `SHORT_SELL` | 3 | 融券卖出 |
| `BUY_TO_COVER` | 4 | 买券还券 |
| `SELL_TO_REPAY` | 5 | 卖券还款 |
| `STOCK_REPAY` | 6 | 现券还券 |
| `IPO_SUBSCRIBE` | 7 | 新股申购（信用） |

### Exchange

| 常量 | 值 | 含义 |
|------|----|------|
| `SZ` | 0 | 深圳 |
| `SH` | 1 | 上海 |
| `BJ` | 6 | 北交所 |

### PriceType

| 常量 | 值 | 含义 | 市场 |
|------|----|------|------|
| `LIMIT` | 0 | 限价 | 沪深北 |
| `PEER_BEST` | 1 | 对手方最优 | 沪深 |
| `OWN_BEST` | 2 | 本方最优 | 沪深 |
| `IOC` | 3 | 即时成交剩余撤销 | 深 |
| `FIVE_LEVEL_IOC` | 4 | 最优五档即时成交剩余撤销 | 沪深 |
| `FOK` | 5 | 全额成交或撤销 | 深 |
| `FIVE_LEVEL_TO_LIMIT` | 6 | 最优五档即时成交剩余转限价 | 沪 |

### FundCategory

| 常量 | 值 | 含义 |
|------|----|------|
| `LOF_SUBSCRIBE` | 0 | 场内申购（金额） |
| `LOF_REDEEM` | 1 | 场内赎回（份额） |
| `ETF_SUBSCRIBE` | 2 | ETF 申购（份额） |
| `ETF_REDEEM` | 3 | ETF 赎回（份额） |

### BankDirection

`BANK_TO_SEC=0`（银行→证券）· `SEC_TO_BANK=1`（证券→银行）

### Field（字符串键，与表头 `名称_ID` 中的 ID 一致）

| 常量 | ID | 含义 |
|------|-----|------|
| `SYMBOL` | 2102 | 证券代码 |
| `NAME` | 2103 | 证券名称 |
| `SHAREHOLDER` | 2106 | 股东帐户 |
| `SHAREHOLDER_NAME` | 2107 | 股东姓名 |
| `MARKET` | 2167 | 交易市场 |
| `ACCOUNT_CLASS` | 6000 | 账号类别 |
| `ORDER_ID` / `ENTRUST_NO` | 2135 | 合同编号 / 委托编号 |
| `AVAILABLE_CASH` | 2116 | 可用金额 |
| `FUND_BALANCE` | 2112 | 资金余额 |
| `PRICE` | 2127 | 委托价格 |
| `QUANTITY` | 2128 | 成交数量（语境依表而定） |
| `ROW_INDEX` | 999 | 序号（1 起） |
| `BANK_CODE` | 2137 | 银行代码 |
| `BANK_ACCOUNT` | 2185 | 银行帐号 |

取列请用 `Field.XXX`，不要依赖中文列名。

### 成交推送 envelope

`set_push_callback(fn)` 收到的 dict 主要字段：

- `client_id`、`function_id`（当前为 2304）
- `rows`：解析后的一行记录
- 原始 13 列顺序：ClientID、推送类型、证券代码、方向、价格、数量、金额、状态、成交时间、股东代码、成交编号、剩余数量、摘要
- 状态常见：`部分成交` / `全部成交`（数字 1 / 2）
- **禁忌**：回调线程内禁止 `logout` / `close` / `Logoff` / `UnInit`；只拷走数据
- 推送队列最多约 64 条，处理太慢会丢更早的

### 登录规则（券商差异）

| 场景 | 登录账号 | 资金/交易账号 | Config |
|------|----------|---------------|--------|
| 普通实盘 | 资金账号 | 多数传空或同资金账号 | `{"qs_id":N,"account_type":0}` |
| 两融信用 | 资金账号前加 `X` | 信用资金账号（不加 X） | 同上，`account_type` 按券商 |
| 华泰客户号 | 客户号 | 按 Demo 保存为准 | `"account_type":"k"` |
| 模拟盘 | 手机号 / 同花顺账户 / 股东账户 | **资金账号留空** | `{"qs_id":356,"account_type":0}`，`trade.10jqka.com.cn:8002`，营业部 48 |

通讯密码多数留空。IP/端口/营业部/`qs_id` 一律从 Demo 或包内 `ths.xml` 取，不要猜。见 https://thsapi.com/sim.html

### 最小示例

```python
# -*- coding: utf-8 -*-
from thsapi import Client, QueryCategory, Field, OrderSide

with Client(r"D:\ths\thsapi.dll") as api:
    r = api.login_from_prefs()
    if not r.ok:
        raise SystemExit(r.error)
    fund = api.query(QueryCategory.FUND)
    print("可用:", fund.rows[0][Field.AVAILABLE_CASH])
    pos = api.query(QueryCategory.POSITIONS)
    print("持仓行数:", len(pos.rows))
    # 实盘下单前先确认；默认注释
    # o = api.order("000001", 10.50, 100, side=OrderSide.BUY)
    # print(o.data.get("order_id"))
```

```python
# 成交推送
def on_fill(msg):
    print(msg.get("rows"))  # 只打印，不要 logout

api.set_push_callback(on_fill)
```

---

## tdxhq（行情）

### 通道 lane

| lane | 账号规则 | 典型端口 | 用途 |
|------|----------|----------|------|
| `L1` | 自动补 `L1*` | 7709 | 五档、K 线、分时、板块 |
| `L2` | 剥前缀；需单独开通 | 常见 7719 | 十档、队列、逐笔 |
| `Ex` | 自动补 `KZ*` | 7727 / 7721 | 期货 / 港美扩展 |

```python
api.connect_from_prefs(lane="L1")
api.connect_from_prefs(lane="Ex")
```

### HqResponse

与交易侧相同：`ok` / `error` / `code` / `data` / `rows` / `rows_by_name` / `to_df()` / `raise_for_status()`。结果已是 UTF-8（Alloc）。

### HqClient 常用方法

连接：`connect_from_prefs` / `connect` / `disconnect`

L1：`quotes` `bars` `index_bars` `minute` `transactions` `boards` `block_info` `board_members` `market_stat` `fund_flow` `call_auction` `finance` `xdxr` `security_list` `security_count`

L2：`quotes10` `buy_sell_queue` `detail_transactions` `detail_orders`

Ex：`ex_markets` `ex_list` `ex_quote` `ex_bars` `ex_minute` `ex_transactions`

代码自动识别 `600519` / `sh600519` / `000001` / `920000` 等市场。

### Period

`MIN5=0` `MIN15=1` `MIN30=2` `MIN60=3` `DAILY=4` `WEEKLY=5` `MONTHLY=6` `MIN1=7`

### Market

`SZ=0` `SH=1` `BJ=2`

### HqField（英文列 ID）

常用：`CODE` `NAME` `PRICE` `PRE_CLOSE` `OPEN` `HIGH` `LOW` `CLOSE` `VOLUME` `AMOUNT` `TIME` `BID1_PRICE` `ASK1_PRICE` `MARKET`

以及 `bid1_vol`/`ask1_vol` … `bid5_price`/`ask5_price` 等五档字段；L2 可到十档。完整中文→英文映射见包内 `fields.py` 的 `CN_TO_ID`。

### 最小示例

```python
# -*- coding: utf-8 -*-
from tdxhq import HqClient, HqField, Period

api = HqClient()  # 或 dll_path=r"...\\HqApi.dll"
api.connect_from_prefs(lane="L1").raise_for_status()
q = api.quotes(["600519", "000001"])
print(q.rows[0][HqField.PRICE])
print(api.bars("600519", period=Period.DAILY, count=5).rows)
api.disconnect()
```
