import sys, collections
sys.path.insert(0, r"C:\HSagent\reversed\rea\agentic_extract\AgenticAssistant.exe_extracted\PYZ.pyz_extracted")
from bgcontroller.client import powerlog as P
log=P.latest_power_log(r"C:\Program Files (x86)\Hearthstone\Logs")
cnt=collections.Counter(); hits=[]
with open(log,"r",encoding="utf-8",errors="replace") as f:
    for line in f:
        for kw in ("COMBAT","STATE=","STEP=","BACON"):
            if kw in line: cnt[kw]+=1
        if "COMBAT" in line and len(hits)<8: hits.append(line.strip()[:200])
print(cnt); print("\n".join(hits))
lg=P.LogGame()
with open(log,"r",encoding="utf-8",errors="replace") as f:
    for line in f: lg.feed(line)
snap=lg.snapshot()
print("\ngame_tags keys sample:", list(snap.get("game_tags",{}).keys())[:30])
print("game_tags:", {k:v for k,v in list(snap.get("game_tags",{}).items())[:25]})
