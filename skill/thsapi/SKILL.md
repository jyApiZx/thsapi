---
name: thsapi
description: >-
  同花顺交易 thsapi.dll / Python thsapi，以及通达信行情 HqApi.dll / Python tdxhq。
  当用户接入交易、行情、下单、撤单、五档、K线、login_prefs、thsapi 或 DLL 接口时使用本 Skill。
---

# thsapi / tdxhq Skill

官网：https://thsapi.com/  
交易文档：https://thsapi.com/trade.html （Alloc：https://thsapi.com/trade.html#alloc ）  
行情文档：https://thsapi.com/hq.html （Alloc：https://thsapi.com/hq.html#alloc ）  
Python 包：https://thsapi.com/python.html  
本 Skill 包：https://thsapi.com/skill/thsapi-skill.zip

## 硬性规则（先读）

1. **先问清**：直接调用 DLL，还是安装 Python 包；语言；程序位数（优先 64 位）；要交易还是行情；是否信用账户。Python 用官方包。C++ / C# / Java 直接调 DLL。
2. **Python 优先走官方 SDK**：`thsapi`（交易）与 `tdxhq`（行情）。**禁止**让用户在 Python 里手填 host / port / yyb / qs_id / config；必须 `login_from_prefs()` / `connect_from_prefs()`。
3. **缺配置时**：引导用户先跑测试包里的 `xdDemo.exe`（交易）或 `行情测试demo.exe`（行情），**登录自己的券商（或模拟盘）**；成功会自动生成同目录 `login_prefs.json`，再写代码。
4. **支持所有券商实盘**（普通账户 + 融资融券）。模拟账户可免费免授权先测。实盘开通：付款后把付款记录 + 资金账号发给客服，**不要索要交易密码**。
5. **位数必须一致**：64 位程序 ↔ 64 位 DLL；32 位同理。Python 先核对解释器位数。
6. **查询成功前不要发实盘单**。账号密码不要写进会提交的代码；用 prefs 或环境变量。示例只用假账号，例如 `12345678` / `your_password` / `trade.example.com`。
7. **推送回调里禁止** `Logoff` / `UnInit` / `logout()` / `close()`。
8. 同一份 DLL，两种调用方式，不要在一个项目里混用缓冲区约定：
   - **原函数**：`QueryData`、`SendOrder` 等，GBK，调用方分配缓冲区。
   - **Alloc 函数**：`*Alloc`，不用预分配缓冲区，结果是 UTF-8。每个非空指针用完必须释放一次，不要释放两次，不要用 `free`。
   - **Python 包**：同一 DLL 的封装，`login_from_prefs()` / `connect_from_prefs()`，不用自己释放。
9. **安全红线**：不要写入、猜测或复述任何内部协议字段名、逆向细节、未公开服务器地址、本机开发路径或未公开隐私。只使用官网与本 Skill `references/` 已公开内容。
10. 详细签名与字段表见 `references/`：
    - [python-sdk.md](references/python-sdk.md) — 全量方法、枚举、Field / HqField、推送、登录规则
    - [trade-v2-and-classic.md](references/trade-v2-and-classic.md) — C 导出、类别表、fieldID、13 列推送
    - [hq-v2-and-classic.md](references/hq-v2-and-classic.md) — A 股 / 扩展行情导出与通道

## 安装（本机 / 官网 wheel，不上 PyPI）

```bash
# 交易包目录里装 thsapi，行情包目录里装 tdxhq：
pip install thsapi-2.0.0-py3-none-any.whl
pip install tdxhq-2.0.0-py3-none-any.whl

# 或官网直链：
pip install https://thsapi.com/whl/thsapi-2.0.0-py3-none-any.whl https://thsapi.com/whl/tdxhq-2.0.0-py3-none-any.whl
```

Wheel **不含** DLL。把 `thsapi.dll` / `HqApi.dll` 与 `login_prefs.json` 放在工作目录，或设置 `THSAPI_DLL` / `TDXHQ_DLL`。

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

- **原函数**：`QueryData` / `SendOrder` / `TdxHq_GetSecurityQuotes`（GBK，调用方分配 Result/ErrInfo）。
- **Alloc 函数**：`*Alloc` + `FreeApiBuffer` / `TdxHq_FreeBuffer`，结果 UTF-8。登录参数从 `login_prefs.json` 读（参考测试包 `cpp/` `csharp/` `java/` 示例）；`isCredit` 为 true 时登录账号前加 `X`。
- 登录都用 `Logon`。券商 IP、端口、营业部、`qs_id` 用 Demo 保存的配置或测试包 `ths.xml`，不要问用户猜。
- 模拟盘：`trade.10jqka.com.cn:8002`，营业部 `48`，`{"qs_id":356,"account_type":0}`；登录账号填手机号/同花顺账户/股东账户，资金账号留空。账户填法见 https://thsapi.com/sim.html

## 不写代码：MCP

用户只想让 AI 直接查持仓、看行情、下单，不需要自己的程序时，推荐 MCP：把 `TradeMcp.Trading.exe` / `TradeMcp.Quotes.exe` 放进测试包目录（仅 64 位），在 AI 客户端添加为 stdio MCP 服务，登录调用 `trade_logon_from_prefs`。下单默认关闭，需 `TRADE_MCP_TRADING_ENABLED=1`，且每笔 prepare + confirm。说明见 https://thsapi.com/mcp.html

## 生成代码时

- 示例每一行可执行语句尽量带中文注释。
- 单元格当字符串；金额用 `Decimal`，不要盲目 `float`。
- 不要编造未文档化的导出函数；以 `references/` 与官网文档为准。
- 写完后提醒用户：只需在 Demo 里登录一次自己的券商，程序就能连上。
