import json, re
d = json.load(open(r"reversed\net\key_enums.json", encoding="utf-8-sig"))
print("top keys:", list(d.keys())[:12])
gt = d.get("game_tag")
print("game_tag entries:", len(gt))
items = gt.items() if isinstance(gt, dict) else [(x,x) for x in gt]
bacon = sorted([(k,v) for k,v in items if str(k).startswith("BACON")])
print("BACON_* count:", len(bacon))
for k,v in bacon[:60]:
    print(f"  {k} = {v}")
