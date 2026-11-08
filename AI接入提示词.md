# Skill（原「AI 接入提示词」）

优先使用 **Skill 包**（含交易、行情和 Python 包的参考文档），比单段提示词更稳。

## 方式一：安装 Skill（推荐）

1. 下载 [thsapi-skill.zip](https://thsapi.com/skill/thsapi-skill.zip)（本仓库对应目录：`skill/thsapi/`）。
2. 解压成名为 `thsapi` 的文件夹，导入到你正在用的 AI（豆包、DeepSeek Harness、WorkBuddy 等）的「技能 / Skill」目录；具体路径以各产品说明为准。
3. 对话里提到交易/行情接入时，助手应自动带上本 Skill。

Skill 内文件：

- `SKILL.md` — 硬性规则与最小模板
- `references/python-sdk.md` — `thsapi` / `tdxhq` 全量方法与枚举
- `references/trade-v2-and-classic.md` — 交易接口：原函数与 Alloc
- `references/hq-v2-and-classic.md` — 行情接口：原函数与 Alloc

网页说明：https://thsapi.com/ai.html  
Python 快速开始：https://thsapi.com/python.html

## 方式二：一键复制给 AI

初次使用可用同花顺模拟账户：登录账号填手机号、同花顺账户或股东账户，三选一，资金账号留空。见 [模拟账户.md](模拟账户.md)。写完代码后，在 `xdDemo.exe` 里登录一次自己的券商即可。

### 交易

```text
你在帮我把项目接到 thsapi（同花顺交易接口）。先问清：直接调用 DLL，还是安装 Python 包；语言（C++/C#/Python/Java）；程序位数（优先 64 位）；券商；是否两融账户；要不要行情。我回答完再改代码。不要一次改很多无关的东西。

直接调用 DLL（C++ / C# / Java，或 Python ctypes）：
- 常用函数是 Logon、QueryData、SendOrder。结果是 GBK，由调用方预分配缓冲区（Result 约 1MB，ErrInfo 256）。
- 不想预分配缓冲区时，用 QueryDataAlloc、SendOrderExAlloc 等 Alloc 函数。结果是 UTF-8，用完必须 FreeApiBuffer，不要用 free，也不要释放两次。
- 登录参数从 xdDemo.exe 生成的 login_prefs.json 读取，不要手填 IP、端口、营业部、qs_id。两融账户（isCredit 为 true）登录账号前加 X。
- 缺 login_prefs.json 时，先让我运行测试包里的 xdDemo.exe，登录我自己的券商或模拟盘一次（成功会自动保存）。
- 下载同花顺交易接口测试包：https://github.com/jyApiZx/thsapi/releases/download/v1.0.7.13/thsapi_1.0.7.13_x64.zip （另有 x32）。文档 https://thsapi.com/trade.html ，Alloc 在 https://thsapi.com/trade.html#alloc ，产品指引 https://thsapi.com/guide.html

Python 包：
- 命令安装：pip install https://github.com/jyApiZx/thsapi/releases/download/python-2.0.0/thsapi-2.0.0-py3-none-any.whl 。也可先下载该 wheel 再 pip install 本地文件。必须用 login_from_prefs()，禁止手填 host/port/yyb/qs_id/config。登录和内存释放由包完成，底层仍是同一个 thsapi.dll。
- 说明 https://thsapi.com/python.html

通用：支持所有券商实盘（普通+两融）。账户填法 https://thsapi.com/sim.html 。券商 IP、端口、营业部、qs_id 不要问我猜，用 Demo 保存的配置或包内 ths.xml。OpenTdx/Init 与 CloseTdx/UnInit 进程级各一次；同一 ClientID 串行。按字段 ID 取列（2116 可用金额、2102 代码、2106 股东、2135 合同编号）。查询成功前不要发实盘单。账号密码不要写进会提交的代码。成交推送 SetPushCallback：回调里禁止 Logoff/UnInit。不要写入内部协议或未公开细节。
```

### 行情

```text
你在帮我把项目接到 HqApi.dll（通达信行情接口）。先问清：直接调用 DLL，还是安装 Python 包；语言；程序位数（优先 64 位）。我回答完再改代码。

直接调用 DLL：
- 常用函数是 TdxHq_Connect、TdxHq_GetSecurityQuotes。结果是 GBK，由调用方预分配缓冲区。
- 不想预分配缓冲区时，用 TdxHq_*Alloc / TdxExHq_*Alloc。结果是 UTF-8，用完必须 TdxHq_FreeBuffer。
- 连接参数从「行情测试demo.exe」生成的 login_prefs.json 读取。缺文件时，先让我用 Demo 连接成功一次。
- 下载通达信行情接口测试包：https://github.com/jyApiZx/thsapi/releases/download/hqapi-1.0.7.0/hqapi_1.0.7.0_x64.zip （另有 x32）。文档 https://thsapi.com/hq.html ，Alloc 在 https://thsapi.com/hq.html#alloc

Python 包：
- 命令安装：pip install https://github.com/jyApiZx/thsapi/releases/download/python-2.0.0/tdxhq-2.0.0-py3-none-any.whl 。也可先下载该 wheel 再 pip install 本地文件。必须用 connect_from_prefs(lane="L1" 或 "Ex")。
- 说明 https://thsapi.com/python.html

通用：L1 端口 7709，账号写成 L1*账号，密码非空。扩展行情常见端口 7727，账号常 KZ* 前缀。不要默认接 L2。不要写入内部协议或未公开细节。
```
