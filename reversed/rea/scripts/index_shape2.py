import json
idx=json.load(open(r"runs\official_traces\index.json",encoding="utf-8-sig"))
print("hdt_logs:", json.dumps(idx["hdt_logs"], ensure_ascii=False)[:1200])
print("\ntraces:", json.dumps(idx["traces"], ensure_ascii=False)[:800])
print("\nblockers:", idx.get("blockers"))
print("\nsettings:", json.dumps(idx.get("settings"), ensure_ascii=False)[:500])
