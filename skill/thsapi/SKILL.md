---
name: thsapi
description: >-
  同花顺交易 thsapi.dll / Python thsapi，以及通达信行情 HqApi.dll / Python tdxhq。
  当用户接入交易、行情、下单、撤单、五档、K线、login_prefs、thsapi 或 DLL 接口时使用本 Skill。
---

# thsapi / tdxhq Skill

官网：https://thsapi.com/  
产品指引：https://thsapi.com/guide.html  
交易文档（同花顺交易接口）：https://thsapi.com/trade.html （Alloc：https://thsapi.com/trade.html#alloc ）  
行情文档（通达信行情接口）：https://thsapi.com/hq.html （Alloc：https://thsapi.com/hq.html#alloc ）  
Python 包：https://thsapi.com/python.html  
MCP 托管：https://thsapi.com/mcp.html  
软件安全核验：https://thsapi.com/security.html  
本 Skill 包：https://thsapi.com/skill/thsapi-skill.zip

## 硬性规则（先读）

1. **先问清**：直接调用 DLL，还是安装 Python 包；语言；程序位数（优先 64 位）；要交易还是行情；是否信用账户。Python 用官方包。C++ / C# / Java 直接调 DLL。
2. **Python 优先走官方 SDK**：`thsapi`（交易）与 `tdxhq`（行情）。**禁止**让用户在 Python 里手填 host / port / yyb / qs_id / config；必须 `login_from_prefs()` / `connect_from_prefs()`。
3. **缺配置时**：引导用户先跑测试包里的 `xdDemo.exe`（交易）或 `行情测试demo.exe`（行情），**登录自己的券商（或模拟盘）**；成功会自动生成同目录 `login_prefs.json`，再写代码。
4. **登录提示「证券账户不存在」或「密码错误」时**：先让用户打开**同花顺官方电脑版客户端**，用同一套账号登录一次。客户端能登录，这个接口就能登录。客户端也登录不了，请用户向所属券商确认。其他登录错误先看 `ErrInfo`，不要把这条答疑用到所有失败上。
5. **支持所有券商实盘**（普通账户 + 融资融券）。模拟账户可免费免授权先测。实盘开通：付款后把付款记录 + 资金账号发给客服，**不要索要交易密码**。
6. **位数必须一致**：64 位程序 ↔ 64 位 DLL；32 位同理。Python 先核对解释器位数。
7. **查询成功前不要发实盘单**。账号密码不要写进会提交的代码；用 prefs 或环境变量。示例只用假账号，例如 `12345678` / `your_password` / `trade.example.com`。
8. **成交推送不是每家券商都支持。** 不要默认只写 `SetPushCallback` / `set_push_callback`。必须用实盘下一笔单验证这个券商真的会回调。还没验证、或验证没有推送时，用 `QueryData`（当日成交）轮询，这是最可靠的做法。推送回调里禁止 `Logoff` / `UnInit` / `logout()` / `close()`。
9. **先看交易 DLL 版本，再决定用不用 Alloc。** Alloc 不是另一份 DLL，也不在 Python wheel 里。不要下载 `https://thsapi.com/downloads/thsapi_2.0.0_x64.zip`，这个地址已经无效。
   - **最新版 1.0.7.23**：同一份 `thsapi.dll` 同时有原函数和 Alloc，导出里有 `FreeApiBuffer`、`QueryDataAlloc`、`SendOrderExAlloc`。下载 https://github.com/jyApiZx/thsapi/releases/download/v1.0.7.23/thsapi_1.0.7.23_x64.zip （另有 x32）。
   - **稳定版 1.0.7.22** 也有 Alloc。让用户下 1.0.7.23；已在用 1.0.7.22 可以继续用。
   - **旧包 1.0.7.13**：只有原函数，约 38 个导出，**没有** `FreeApiBuffer` 和任何 `*Alloc`。用户已经拿着这份库时，只能调用 `QueryData`、`SendOrder`，不要生成 Alloc。要 Alloc 就换成 1.0.7.23，不要另找一个「Alloc 版 DLL」。
   - **原函数**：GBK，调用方分配缓冲区。
   - **Alloc 函数**（仅 1.0.7.22 及以后）：不用预分配缓冲区，结果是 UTF-8。每个非空指针用完必须 `FreeApiBuffer` 一次，不要释放两次，不要用 `free`。
   - **Python 包**：只是调用封装，wheel 不含 DLL。`pip install` 用 GitHub `python-2.0.0` 的 whl，DLL 仍用上面的测试包。登录用 `login_from_prefs()` / `connect_from_prefs()`，不用自己释放。
10. **安全红线**：不要写入、猜测或复述任何内部协议字段名、逆向细节、未公开服务器地址、本机开发路径或未公开隐私。只使用官网与本 Skill `references/` 已公开内容。
11. 详细签名与字段表见 `references/`：
    - [python-sdk.md](references/python-sdk.md) — 全量方法、枚举、Field / HqField、推送、登录规则
    - [trade-v2-and-classic.md](references/trade-v2-and-classic.md) — C 导出、类别表、fieldID、13 列推送
    - [hq-v2-and-classic.md](references/hq-v2-and-classic.md) — A 股 / 扩展行情导出与通道

## 安装（本机 / 官网 wheel，不上 PyPI）

```bash
# 交易包目录里装 thsapi，行情包目录里装 tdxhq：
pip install thsapi-2.0.0-py3-none-any.whl
pip install tdxhq-2.0.0-py3-none-any.whl

# 或 GitHub Release 命令安装：
pip install https://github.com/jyApiZx/thsapi/releases/download/python-2.0.0/thsapi-2.0.0-py3-none-any.whl https://github.com/jyApiZx/thsapi/releases/download/python-2.0.0/tdxhq-2.0.0-py3-none-any.whl

# 也可先下载再装：
# https://github.com/jyApiZx/thsapi/releases/download/python-2.0.0/thsapi-2.0.0-py3-none-any.whl
# https://github.com/jyApiZx/thsapi/releases/download/python-2.0.0/tdxhq-2.0.0-py3-none-any.whl
```

Wheel **不含** DLL。先下交易 / 行情测试包（同花顺交易接口 `thsapi.dll`、通达信行情接口 `HqApi.dll`），把 DLL 与 `login_prefs.json` 放在工作目录，或设置 `THSAPI_DLL` / `TDXHQ_DLL`。

## Python 交易最小模板

```python
# -*- coding: utf-8 -*-
from thsapi import Client, QueryCategory, Field

with Client(r"D:\ths\thsapi.dll") as api:  # 与 xdDemo.exe 同目录
    r = api.login_from_prefs()
    if not r.ok:
        raise SystemExit(r.error)
    fund = api.query(QueryCategory.FUND)
    print(fund.rows[0][Field.AVAILABLE_CASH])
    # api.order("000001", 10.50, 100)  # 确认后再打开
```

## Python 行情最小模板

```python
# -*- coding: utf-8 -*-
from tdxhq import HqClient, HqField, Period

api = HqClient()  # 或 dll_path=r"...\\HqApi.dll"
api.connect_from_prefs(lane="L1").raise_for_status()
print(api.quotes("600519").rows[0][HqField.PRICE])
api.disconnect()
```

## 非 Python（C++ / C# / Java）

- 交易 DLL 是 1.0.7.22 或 1.0.7.23 时，可以选原函数或 Alloc。用户手里是 1.0.7.13 时，只写原函数，不要生成 `FreeApiBuffer` / `QueryDataAlloc` / `SendOrderExAlloc`。
- **原函数**：`QueryData` / `SendOrder` / `TdxHq_GetSecurityQuotes`（GBK，调用方分配 Result/ErrInfo）。
- **Alloc 函数**（交易须 1.0.7.22 及以后）：`*Alloc` + `FreeApiBuffer` / `TdxHq_FreeBuffer`，结果 UTF-8。导出表里没有该函数就不要调用。`isCredit` 为 true 时登录账号前加 `X`。
- 登录都用 `Logon`。券商 IP、端口、营业部、`qs_id` 用 Demo 保存的配置或测试包 `ths.xml`，不要问用户猜。
- 模拟盘：`trade.10jqka.com.cn:8002`，营业部 `48`，`{"qs_id":356,"account_type":0}`；登录账号填手机号/同花顺账户/股东账户，资金账号留空。账户填法见 https://thsapi.com/sim.html

## 不写代码：MCP 托管

用户只想让 AI 直接查持仓、看行情、下单（同花顺交易接口 + 通达信行情接口），不需要自己的程序时，推荐 **MCP 托管**：把 `TradeMcp.Trading.exe` / `TradeMcp.Quotes.exe` 放进测试包目录（仅 64 位），在 AI 客户端添加为 stdio MCP 服务，登录调用 `trade_logon_from_prefs`。下单默认关闭，需 `TRADE_MCP_TRADING_ENABLED=1`，且每笔 prepare + confirm。说明见 https://thsapi.com/mcp.html

## 生成代码时

- 示例每一行可执行语句尽量带中文注释。
- 单元格当字符串；金额用 `Decimal`，不要盲目 `float`。
- 不要编造未文档化的导出函数；以 `references/` 与官网文档为准。
- 写完后提醒用户：只需在 Demo 里登录一次自己的券商，程序就能连上。
