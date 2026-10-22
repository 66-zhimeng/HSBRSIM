import sys, json, time
sys.path.insert(0, r"C:\HSagent\reversed\rea\agentic_extract\AgenticAssistant.exe_extracted\PYZ.pyz_extracted")
from bgcontroller.client import powerlog as P, mapping as Map
from bgassistant import catalog as C
mapper=Map.Mapper(C.Catalog())
log=P.latest_power_log(r"C:\Program Files (x86)\Hearthstone\Logs")
lg=P.LogGame(); found=[]; n=0; t=time.time()
with open(log,"r",encoding="utf-8",errors="replace") as f:
    for line in f:
        lg.feed(line); n+=1
        if "BACON_IN_COMBAT_PHASE" in line or "BACON_COMBAT_PHASE" in line:
            snap=lg.snapshot()
            try: fr=mapper.frame(snap)
            except Exception: continue
            if fr and str(fr.state.phase)=="combat" and not snap.get("game_over"):
                found.append((n, fr.state.turn, len(fr.state.board), len(fr.state.shop)))
print("scanned",n,"lines in",round(time.time()-t,2),"s; combat frames:",len(found))
print("first:",found[:5]); print("last:",found[-3:])
