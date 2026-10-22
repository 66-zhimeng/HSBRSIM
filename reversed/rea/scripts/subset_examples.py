import json, sys, re
sys.path.insert(0, r"C:\HSagent"); sys.path.insert(0, r"C:\HSagent\tools")
from pathlib import Path
import diff_replay_combat as drc
from hsrl2.db import CardDB
db=CardDB.load(r"C:\HSagent\data"); res=drc.CardResolver(db, drc.OFFICIAL_DATA_255130)
rep=json.load(open(r"runs\diff_report.json",encoding="utf-8-sig"))
recs=rep["records"]
def kws(r):
    out=set()
    for side in ("player","opponent"):
        for m in r["trace"][side].get("minions") or []:
            out.update(m.get("keywords") or [])
    return out
def ids(pred, n=3):
    return [r["trace"]["trace_id"] for r in recs if pred(r)][:n]
print("reborn traces:", ids(lambda r:"reborn" in kws(r)))
print("venom traces:", ids(lambda r: kws(r)&{"venomous","poisonous"}))
print("cleave traces:", ids(lambda r:"cleave" in kws(r)))
print("deathrattle-annotation traces:", [r["trace"]["trace_id"] for r in recs if any(g.startswith("deathrattle_annotation") for g in (r.get("simulation",{}).get("context_gaps") or []))][:3])
# buffed reborn example: reborn minion stats != def
def buffed_reborn(r):
    for side in ("player","opponent"):
        for m in r["trace"][side].get("minions") or []:
            if "reborn" not in (m.get("keywords") or []): continue
            cid=(r.get("simulation",{}).get("engine_card_ids") or {}).get(m.get("name") or "")
            d=db.get(cid) if cid else None
            if d and (m.get("atk"),m.get("health"))!=(d.atk,d.health): return True
    return False
print("buffed reborn traces:", ids(buffed_reborn))
