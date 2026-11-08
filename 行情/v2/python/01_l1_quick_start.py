# -*- coding: utf-8 -*-
"""
2.0 行情示例（Python）

用法：
1. 解压测试包，运行「行情测试demo.exe」，用 L1 连接一次
2. pip install tdxhq-2.0.0-py3-none-any.whl
3. python python/01_l1_quick_start.py
"""
from pathlib import Path
from tdxhq import HqClient, HqField, Period

HERE = Path(__file__).resolve().parent.parent
api = HqClient(dll_path=str(HERE / "HqApi.dll"))
api.connect_from_prefs(lane="L1").raise_for_status()

q = api.quotes(["600519", "000001"])
if q.ok and q.rows:
    print("现价:", q.rows[0].get(HqField.PRICE))
else:
    print("行情失败:", q.error)

bars = api.bars("600519", period=Period.DAILY, count=5)
print("日K行数:", len(bars.rows) if bars.ok else bars.error)
api.disconnect()
