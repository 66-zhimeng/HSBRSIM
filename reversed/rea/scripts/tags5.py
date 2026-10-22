import json,re
d = json.load(open(r"reversed\net\key_enums.json", encoding="utf-8-sig"))
v = d["game_tag"]["values"]
for pat in ["ELEMENTAL","COMBAT_DAMAGE","DAMAGE_CAP","FIRST_PLAYER","CURRENT_OPPONENT","TAVERN_TIER","TECH_LEVEL","MAX_MINIONS","BACON_MAX"]:
    hits = {k:x for k,x in v.items() if pat in k}
    print(pat, "->", hits if len(hits)<8 else list(hits.items())[:8])
print()
print("official_name:", d["game_tag"]["official_name"], "full:", d["game_tag"]["full_name"])
