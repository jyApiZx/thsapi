# 同花顺交易接口与通达信接口

**同花顺交易接口、通达信交易接口、通达信行情接口**（thsapi / tdxapi）。交易库 `thsapi.dll` 是同花顺交易接口，直接连接券商交易服务器，不用打开同花顺客户端，支持所有券商实盘（普通账户与融资融券）。行情库 `HqApi.dll` 是通达信行情接口。Python、C++、C#、Java 都可以接这个量化交易接口，32 位和 64 位都有，程序位数要和 DLL 一致。

[![Windows](https://img.shields.io/badge/Windows-10%2F11%2FServer-0078D4)](https://thsapi.com/)
[![Language](https://img.shields.io/badge/Language-Python%20%7C%20C%2B%2B%20%7C%20C%23%20%7C%20Java-3776AB)](https://thsapi.com/python.html)
[![Latency](https://img.shields.io/badge/下单-45ms-2EA44F)](#速度)
[![MCP](https://img.shields.io/badge/MCP-开源-6E40C9)](#mcp开源)
[![Skill](https://img.shields.io/badge/AI-Skill-0B7A75)](#ai-skill)
[![License](https://img.shields.io/badge/License-Unlicense-lightgrey)](LICENSE)

功能 · [安装](#安装) · [AI Skill](#ai-skill) · [MCP](#mcp开源) · [示例](#示例python)

## 功能

- 同花顺交易接口：登录后查询资金、持仓、委托和成交，再下单、撤单。成交由券商推送，不用轮询。
- 支持融资融券、新股申购、场内基金和银证转账。可以单账户操作，也可以多个账户同时在线。
- 通达信行情接口：L1 五档、Tick、分时和 K 线，扩展行情含美股、港股、期货。交易连接上也可以取五档快照。
- Python 包（`thsapi` / `tdxhq`）在同一 DLL 上把登录和内存释放包好。
- MCP 开源：把同花顺交易接口和通达信行情接口交给常用 AI，用一句话查持仓、看行情、下单。

## 安装

测试包在 GitHub Release。DLL 不在 git 仓库里。优先 64 位，位数要和程序一致。

### 交易 DLL（同花顺交易接口 `thsapi.dll`）

包内是 `thsapi.dll`、`xdDemo.exe`。

- 最新版 1.0.7.22：[64 位](https://github.com/jyApiZx/thsapi/releases/download/v1.0.7.22/thsapi_1.0.7.22_x64.zip) · [32 位](https://github.com/jyApiZx/thsapi/releases/download/v1.0.7.22/thsapi_1.0.7.22_x32.zip)
- 稳定版 1.0.7.13：[64 位](https://github.com/jyApiZx/thsapi/releases/download/v1.0.7.13/thsapi_1.0.7.13_x64.zip) · [32 位](https://github.com/jyApiZx/thsapi/releases/download/v1.0.7.13/thsapi_1.0.7.13_x32.zip)

### 行情 DLL（通达信行情接口 `HqApi.dll`）

L1 端口 7709，账号写成 `L1*账号`。含扩展行情（美股、港股、期货）。

- 1.0.7.0：[64 位（推荐）](https://github.com/jyApiZx/thsapi/releases/download/hqapi-1.0.7.0/hqapi_1.0.7.0_x64.zip) · [32 位](https://github.com/jyApiZx/thsapi/releases/download/hqapi-1.0.7.0/hqapi_1.0.7.0_x32.zip)

### Python 包

先下载上面的交易或行情测试包，用 Demo 登录一次自己的券商（或模拟盘），再装 wheel。不上 PyPI。

**方式一：命令安装**

```bash
pip install https://github.com/jyApiZx/thsapi/releases/download/python-2.0.0/thsapi-2.0.0-py3-none-any.whl https://github.com/jyApiZx/thsapi/releases/download/python-2.0.0/tdxhq-2.0.0-py3-none-any.whl
```

**方式二：下载安装**

- [下载 thsapi](https://github.com/jyApiZx/thsapi/releases/download/python-2.0.0/thsapi-2.0.0-py3-none-any.whl)
- [下载 tdxhq](https://github.com/jyApiZx/thsapi/releases/download/python-2.0.0/tdxhq-2.0.0-py3-none-any.whl)

下载后在文件所在目录执行：`pip install thsapi-2.0.0-py3-none-any.whl tdxhq-2.0.0-py3-none-any.whl`。必须用 `login_from_prefs()` / `connect_from_prefs()`。

## AI Skill

现在多数人用 AI 改代码，不必先翻接口表。把 Skill 交给正在改项目的 AI，它会按官方约定接同花顺交易接口和通达信行情接口。

1. 下载 [thsapi-skill.zip](https://thsapi.com/skill/thsapi-skill.zip)，或直接用仓库里的 [skill/thsapi/](skill/thsapi/SKILL.md)。
2. 解压成名为 `thsapi` 的文件夹，导入豆包、DeepSeek Harness、WorkBuddy 等 AI 的「技能 / Skill」目录。
3. 对话里提到交易或行情接入时，把下面这段话发给 AI：

```text
帮我把项目接到 thsapi（同花顺交易接口）和 HqApi.dll（通达信行情接口）。先读 skill/thsapi/SKILL.md。Python 用官方包，必须 login_from_prefs() / connect_from_prefs()，不要手填地址和密码。C++、C#、Java 直接调 DLL。先让我用测试包里的 Demo 登录一次自己的券商，再改代码。
```

完整函数说明在 Skill 的 `references/` 里，也可以看 [交易文档](https://thsapi.com/trade.html) 和 [行情文档](https://thsapi.com/hq.html)。

## MCP（开源）

本仓库的 MCP 是开源的。它把同花顺交易接口和通达信行情接口封装成两个本地服务，豆包、DeepSeek、龙虾、WorkBuddy、Codex、Claude 都能用一句话查持仓、看行情、下单。

下单默认关闭。每笔先预览，你确认后才提交，单笔数量和金额有上限。用法见 [mcp/README.md](mcp/README.md)。

## 示例（Python）

先用 `xdDemo.exe` 登录一次，再跑下面几行。代码里不用填 IP、端口、营业部和密码。

```python
from thsapi import Client, QueryCategory, Field

with Client(r"D:\ths\thsapi.dll") as api:  # 改成你的解压目录
    api.login_from_prefs()
    fund = api.query(QueryCategory.FUND)
    print("可用资金:", fund.rows[0][Field.AVAILABLE_CASH])
    # api.order("000001", price=10.50, quantity=100)  # 确认后再打开
```

C++、C#、Java 同样支持，示例在 `交易/`、`行情/` 目录。

## 速度

单次往返，不是平均值。同花顺交易接口下单 `SendOrder` 45 毫秒，直连券商，中间没有客户端。通达信行情接口一次拉完全市场 5224 只五档，含解码 185 毫秒。不同网络和站点会有差异。

## 常见问题

**找不到 DLL。** 把 `thsapi.dll` 或 `HqApi.dll` 放在程序目录，文件名保持小写，32/64 位要和程序一致。

**登录失败。** 先用测试包里的 `xdDemo.exe`（交易）或行情 Demo 登录一次自己的券商或模拟盘，成功后再跑自己的程序。

联系 [Telegram：@Thsapi](https://t.me/Thsapi)。账户怎么填分三种：普通账户、信用账户（融资融券）、同花顺模拟账户，见 [账户怎么填](https://thsapi.com/sim.html)，仓库里也有 [模拟账户.md](模拟账户.md)。

项目主页：[thsapi.com](https://thsapi.com/)
