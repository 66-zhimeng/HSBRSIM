import json
d = json.load(open(r"reversed\net\key_enums.json", encoding="utf-8-sig"))
gt = d["game_tag"]
print("game_tag type:", type(gt), "keys:", list(gt.keys()) if isinstance(gt, dict) else None)
if isinstance(gt, dict):
    for k,v in gt.items():
        print(k, type(v), (len(v) if hasattr(v,"__len__") else v))
    vals = gt.get("values") or gt.get("members") or {}
    print("values count:", len(vals))
    hits = {k:v for k,v in vals.items() if "DAMAGE_CAP" in k.upper()}
    print("DAMAGE_CAP:", hits)
    bacon = {k:v for k,v in vals.items() if k.upper().startswith("BACON")}
    print("BACON count:", len(bacon))
    for k,v in list(sorted(bacon.items()))[:40]: print(" ",k,"=",v)
