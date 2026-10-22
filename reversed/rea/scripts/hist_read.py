import json
d=json.load(open(r"reversed\rea\damage_cap_evidence.json",encoding="utf-8-sig"))
c=d["corpus"]
print("traces:", c["traces"], "min/max:", c["turn_min"], c["turn_max"])
print("hist:", json.dumps(c["turn_histogram"], ensure_ascii=False))
print("sum:", sum(c["turn_histogram"].values()))
print("official:", c["official_result_counts"])
le3=sum(v for k,v in c["turn_histogram"].items() if int(k)<=3)
t47=sum(v for k,v in c["turn_histogram"].items() if 4<=int(k)<=7)
ge8=sum(v for k,v in c["turn_histogram"].items() if int(k)>=8)
print("turn<=3:",le3,"4-7:",t47,">=8:",ge8)
