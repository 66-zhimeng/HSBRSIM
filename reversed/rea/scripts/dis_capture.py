import sys, dis, importlib
sys.path.insert(0, r"C:\HSagent\reversed\rea\agentic_extract\AgenticAssistant.exe_extracted\PYZ.pyz_extracted")
from bgcontroller.client import memory as M
# disassemble capture_snapshot to learn flow + snapshot schema
code = M.capture_snapshot.__code__
print("co_names:", code.co_names)
print("co_consts:", [c for c in code.co_consts if not hasattr(c,'co_code')])
print("\n--- dis (first 140) ---")
for idx, ins in enumerate(dis.get_instructions(code)):
    if idx>140: break
    print(f"{ins.offset:5d} {ins.opname:24s} {str(ins.argval)[:120]}")
