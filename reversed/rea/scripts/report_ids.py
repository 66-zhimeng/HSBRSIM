import json
d=json.load(open(r"runs\diff_report.json",encoding="utf-8-sig"))
recs=d["records"]
ids={"4c3c73caa189365e":"D1","aa4c75414541d87b":"D2","b2683903c4464b6f":"D3","f5d1c647b2f449d1":"D4","9e80beef2ada485a":"D5","11285dbbe007f132":"D6"}
by={r.get("trace_id"):r for r in recs if isinstance(r,dict)}
if not by:
    # records may be nested
    for r in recs:
        t=r.get("trace") or {}
        tid=t.get("trace_id")
        if tid: by[tid]=r
for tid,k in ids.items():
    r=by.get(tid)
    if not r: print(k,tid,"NOT FOUND in records"); continue
    sim=r.get("simulation") or {}
    cmp=r.get("comparison") or {}
    print("="*60)
    print(k,tid)
    print(" engine_card_ids:", sim.get("engine_card_ids"))
    print(" context_gaps:", sim.get("context_gaps"))
    print(" data_gaps:", sim.get("data_gaps"))
    print(" attribution:", r.get("attribution"), "| classification:", cmp.get("classification") or r.get("classification"))
    print(" rates:", sim.get("rates") or {kk:vv for kk,vv in r.items() if kk in ("rates","distribution")})
