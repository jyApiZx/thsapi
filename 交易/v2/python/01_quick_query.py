# -*- coding: utf-8 -*-
"""
2.0 交易示例（Python）

用法：
1. 解压测试包，双击 xdDemo.exe，登录自己的券商或模拟盘一次
2. pip install thsapi-2.0.0-py3-none-any.whl
3. 在解压目录运行：python python/01_quick_query.py
"""
from pathlib import Path
from thsapi import Client, QueryCategory, Field

# 与 xdDemo.exe / thsapi.dll / login_prefs.json 同目录
HERE = Path(__file__).resolve().parent.parent
DLL = HERE / "thsapi.dll"

with Client(dll_path=str(DLL)) as api:
    # 自动读取刚才 Demo 保存的券商配置
    r = api.login_from_prefs()
    if not r.ok:
        raise SystemExit("登录失败: " + str(r.error))

    # 查资金（Category=0）；按稳定字段 ID 取值
    fund = api.query(QueryCategory.FUND)
    if fund.ok and fund.rows:
        print("可用金额:", fund.rows[0].get(Field.AVAILABLE_CASH))
        print("资金余额:", fund.rows[0].get(Field.FUND_BALANCE))
    else:
        print("查询失败:", fund.error)

    # 查持仓
    pos = api.query(QueryCategory.POSITIONS)
    print("持仓行数:", len(pos.rows) if pos.ok else pos.error)

    # 下单示例（默认注释，确认无误再打开）
    # o = api.order("000001", price=10.50, quantity=100)
    # print(o.data.get("order_id") if o.ok else o.error)
