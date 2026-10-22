import json
d=json.load(open(r"runs\diff_report.json",encoding="utf-8-sig"))
recs=d["records"]
print("n records:", len(recs))
r0=recs[0]
print("keys:", list(r0.keys()))
print(json.dumps(r0, ensure_ascii=False)[:800])
