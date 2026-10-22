import json, re
from pathlib import Path
idx=json.load(open(r"runs\official_traces\index.json",encoding="utf-8-sig"))
print("index keys:", list(idx.keys()))
for k,v in idx.items():
    if isinstance(v,(list,dict)):
        print(k, type(v), len(v))
print(json.dumps({k:(v if not isinstance(v,(list,dict)) else "...") for k,v in idx.items()}, ensure_ascii=False)[:800])
if isinstance(idx.get("traces"), list) and idx["traces"]:
    print("trace entry:", json.dumps(idx["traces"][0], ensure_ascii=False)[:500])
if isinstance(idx.get("sessions"), list) and idx["sessions"]:
    print("session entry:", json.dumps(idx["sessions"][0], ensure_ascii=False)[:500])
