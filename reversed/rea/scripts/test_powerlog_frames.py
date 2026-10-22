import sys, collections
sys.path.insert(0, r"C:\HSagent\reversed\rea\agentic_extract\AgenticAssistant.exe_extracted\PYZ.pyz_extracted")
from bgcontroller.client import powerlog as P, mapping as Map
log=P.latest_power_log(r"C:\Program Files (x86)\Hearthstone\Logs")
lg=P.LogGame(); n=0; stats=collections.Counter(); frames=0; examples=[]
with open(log,"r",encoding="utf-8",errors="replace") as f:
    for line in f:
        lg.feed(line); n+=1
        if n%2000==0:
            snap=lg.snapshot()
            try: fr=Map.Mapper().frame(snap)
            except Exception as e:
                stats["frame_error"]+=1; continue
            if fr is None: stats["none"]+=1; continue
            frames+=1; st=fr.state
            key=(st.phase, len(st.shop), len(st.board), snap.get("game_over"))
            stats[key]+=1
            if len(examples)<8 and st.phase!="combat": examples.append((n,st.phase,st.turn,st.gold,st.tier,len(st.shop),len(st.board),len(st.choices),snap.get("game_over")))
print("lines",n,"frames",frames)
for k,v in stats.most_common(20): print(v,k)
print("examples:", examples)
# final snapshot
snap=lg.snapshot(); fr=Map.Mapper().frame(snap)
print("final frame:", None if fr is None else (fr.state.phase, fr.state.turn, len(fr.state.shop), len(fr.state.board), snap.get("game_over")))
