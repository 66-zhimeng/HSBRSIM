import json
p=r"C:\HSagent\AgenticAssistant\_internal\bgassistant\data\catalog.json"
d=json.load(open(p,encoding="utf-8"))
print(type(d), list(d.keys())[:10] if isinstance(d,dict) else len(d))
if isinstance(d,dict):
    for k in list(d.keys())[:3]:
        v=d[k]
        print(k, type(v), (list(v.keys())[:12] if isinstance(v,dict) else str(v)[:200]))
elif isinstance(d,list):
    print("len",len(d)); print(json.dumps(d[0],ensure_ascii=False)[:400])
