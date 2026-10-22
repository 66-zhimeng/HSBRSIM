import json
d=json.load(open(r"runs\diff_report.json",encoding="utf-8-sig"))
print("generated:", d.get("generated_at"))
r=d["records"][0]
print("record keys:", list(r.keys()))
print("comparison keys:", list(r["comparison"].keys()))
print(json.dumps({k:v for k,v in r["comparison"].items() if k!="evidence"}, ensure_ascii=False, indent=1)[:600])
print("evidence keys:", list((r["comparison"].get("evidence") or {}).keys()))
print(json.dumps((r["comparison"].get("evidence") or {}), ensure_ascii=False)[:800])
print("attribution:", r.get("attribution"))
print("summary qg:", d["summary"].get("context_gap"), "div:", d["summary"].get("engine_divergence"), "match:", d["summary"].get("match"))
