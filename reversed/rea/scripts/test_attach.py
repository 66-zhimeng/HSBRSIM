import sys, json, traceback
sys.path.insert(0, r"C:\HSagent\reversed\rea\agentic_extract\AgenticAssistant.exe_extracted\PYZ.pyz_extracted")
from bgcontroller.client import memory as M
print("process_info():", M.process_info())
try:
    snap = M.capture_snapshot()
    print("SNAPSHOT type:", type(snap))
    if isinstance(snap, dict):
        print("keys:", list(snap.keys())[:20])
        for k,v in snap.items():
            if isinstance(v,(list,dict)):
                print(f"  {k}: {type(v).__name__} len={len(v)}")
            else:
                print(f"  {k}: {v!r}"[:180])
    else:
        print("repr:", repr(snap)[:2000])
except Exception as e:
    print("CAPTURE FAIL:", type(e).__name__, e)
    traceback.print_exc(limit=6)
