import json
d=json.load(open(r"C:\HSagent\AgenticAssistant\_internal\bgassistant\data\catalog.json",encoding="utf-8"))
sk=d["skins"]; k=list(sk)[0]; print(k, "->", json.dumps(sk[k],ensure_ascii=False)[:300])
print("cards sample:", json.dumps(d["cards"][0],ensure_ascii=False)[:400])
heroes=[c for c in d["cards"] if c.get("kind")=="hero"][:3]
print("hero cards:", json.dumps(heroes,ensure_ascii=False)[:400])
