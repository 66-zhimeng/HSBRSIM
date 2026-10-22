import sys, time, inspect, json
sys.path.insert(0, r"C:\HSagent\reversed\rea\agentic_extract\AgenticAssistant.exe_extracted\PYZ.pyz_extracted")
from bgcontroller.client import powerlog as P
print("LogGame.__init__:", inspect.signature(P.LogGame.__init__))
print("LogGame.feed:", inspect.signature(P.LogGame.feed))
print("LogGame.snapshot:", inspect.signature(P.LogGame.snapshot))
log = P.latest_power_log(r"C:\Program Files (x86)\Hearthstone\Logs")
print("latest:", log)
t=time.time(); lg=P.LogGame(); n=0
with open(log, "r", encoding="utf-8", errors="replace") as f:
    for line in f:
        lg.feed(line); n+=1
print("fed", n, "lines in", round(time.time()-t,2), "s")
snap = lg.snapshot()
print("snapshot type:", type(snap), "keys:", list(snap.keys())[:15] if isinstance(snap,dict) else None)
if isinstance(snap,dict):
    for k in ("schema","game_identity","game_over","players","entities"):
        v=snap.get(k); print(k, type(v).__name__, (len(v) if hasattr(v,'__len__') else v))
