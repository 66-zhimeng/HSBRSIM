import json
d=json.load(open(r"runs\diff_report.json",encoding="utf-8-sig"))
print("generated_at:", d.get("generated_at"))
print("summary keys:", list(d["summary"].keys()))
for k in ("divergence_attribution","context_gap_attribution","context_gap","engine_divergence","match","clean_subset_match_rate","deterministic_match_rate"):
    print(" ", k, "=", d["summary"].get(k))
print()
ids={"4c3c73caa189365e":"D1","aa4c75414541d87b":"D2","b2683903c4464b6f":"D3","f5d1c647b2f449d1":"D4","9e80beef2ada485a":"D5","11285dbbe007f132":"D6"}
by={r["trace"]["trace_id"]:r for r in d["records"]}
for tid,k in ids.items():
    r=by[tid]
    c=r["comparison"]
    print(k, tid, "| status:", c.get("status"), "| subclass:", c.get("subclass"), "| attr:", r.get("attribution",{}).get("attribution"), "| gaps:", r["simulation"].get("context_gaps"))
