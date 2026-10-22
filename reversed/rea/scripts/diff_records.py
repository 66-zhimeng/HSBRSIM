import json
d=json.load(open(r"runs\diff_report.json",encoding="utf-8-sig"))
print("top keys:", list(d.keys())[:15] if isinstance(d,dict) else type(d))
# find records
recs = None
for k in ("records","traces","results","comparisons"):
    if isinstance(d,dict) and k in d: recs=d[k]; print("using",k,len(recs)); break
if recs is None and isinstance(d,list): recs=d
ids={"4c3c73caa189365e","aa4c75414541d87b","b2683903c4464b6f","f5d1c647b2f449d1","9e80beef2ada485a","11285dbbe007f132"}
shown=0
for e in recs:
    s=json.dumps(e, ensure_ascii=False)
    if any(i in s for i in ids):
        print("="*60)
        print(json.dumps({k:v for k,v in e.items() if k not in ("distribution",)}, ensure_ascii=False, indent=1)[:2500])
        shown+=1
        if shown>=6: break
