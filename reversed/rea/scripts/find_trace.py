import json, os
idx = json.load(open(r"runs\official_traces\index.json", encoding="utf-8"))
print(type(idx), len(idx))
# find entries whose id matches
ids = ["4c3c73caa189365e","aa4c75414541d87b","b2683903c4464b6f","f5d1c647b2f449d1","9e80beef2ada485a","11285dbbe007f132"]
for e in (idx if isinstance(idx, list) else idx.get("traces", [])):
    tid = e.get("trace_id") or e.get("id")
    if tid in ids:
        print(json.dumps(e, ensure_ascii=False, indent=1)[:600])
