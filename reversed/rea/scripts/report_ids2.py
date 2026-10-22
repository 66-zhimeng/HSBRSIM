import json
d=json.load(open(r"runs\diff_report.json",encoding="utf-8-sig"))
ids={"4c3c73caa189365e":"D1","aa4c75414541d87b":"D2","b2683903c4464b6f":"D3","f5d1c647b2f449d1":"D4","9e80beef2ada485a":"D5","11285dbbe007f132":"D6"}
by={r["trace"]["trace_id"]:r for r in d["records"]}
for tid,k in ids.items():
    r=by.get(tid)
    if not r: print(k,tid,"NOT FOUND"); continue
    sim=r["simulation"]; cmp=r["comparison"]; at=r["attribution"]
    print("="*66)
    print(k,tid)
    print(" engine_card_ids:", sim.get("engine_card_ids"))
    print(" context_gaps:", sim.get("context_gaps"))
    print(" data_gaps:", sim.get("data_gaps"), "errors:", sim.get("errors"))
    print(" comparison:", {kk:cmp.get(kk) for kk in ("classification","official_result","engine_rates","l1","confidence") if kk in cmp} or cmp)
    print(" attribution:", at)
    print(" engine_rates:", sim.get("rates"))
