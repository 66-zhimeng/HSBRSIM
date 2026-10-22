# 追加 repro3 证据脚本说明已存在
import os
p=r"reversed\rea\scripts\repro3_check.py"
print("exists:", os.path.exists(p), os.path.getsize(p) if os.path.exists(p) else 0)
p2=r"reversed\rea\behavior_mismatch_analysis.md"
print("doc:", os.path.getsize(p2))
