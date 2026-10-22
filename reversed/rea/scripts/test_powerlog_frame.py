import sys, json, time
sys.path.insert(0, r"C:\HSagent\reversed\rea\agentic_extract\AgenticAssistant.exe_extracted\PYZ.pyz_extracted")
from bgcontroller.client import powerlog as P, mapping as Map
log = P.latest_power_log(r"C:\Program Files (x86)\Hearthstone\Logs")
lg=P.LogGame(); best=None; n=0; t=time.time()
with open(log,"r",encoding="utf-8",errors="replace") as f:
    for line in f:
        lg.feed(line); n+=1
        if n % 2000 == 0:
            snap=lg.snapshot()
            try: frame=Map.Mapper().frame(snap)
            except Exception as e: continue
            if frame is None: continue
            st=frame.state
            if getattr(st,"phase","")=="recruit" and len(getattr(st,"shop",[]))>0 and not snap.get("game_over"):
                best=frame
print("lines:", n, "elapsed:", round(time.time()-t,2), "s")
if best is not None:
    st=best.state
    print("phase:", st.phase, "turn:", st.turn, "gold:", st.gold, "tier:", st.tier)
    print("hero:", st.hero, "health:", st.health, "armor:", st.armor)
    print("board:", [(m.card,m.atk,m.hp) for m in st.board])
    print("hand:", [(i.kind,i.card,i.cost) for i in st.hand])
    print("shop:", [(i.kind,i.card,i.cost) for i in st.shop])
    print("choices:", [ (c.get("type") if isinstance(c,dict) else type(c).__name__) for c in st.choices])
    print("legal.buy:", best.legal.buy, "refresh:", best.legal.refresh, "upgrade:", best.legal.upgrade)
else:
    print("no recruit frame found")
