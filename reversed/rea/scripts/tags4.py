import json
d = json.load(open(r"reversed\net\key_enums.json", encoding="utf-8-sig"))
vals = d["game_tag"]["values"]
want = ["BACON_COMBAT_DAMAGE_CAP","BACON_COMBAT_DAMAGE_CAP_ENABLED","BACON_ELEMENTAL_PLAY_COUNTER","BACON_BLOODGEMBUFFATKVALUE","BACON_BLOODGEMBUFFHEALTHVALUE","BACON_COMBAT_PHASE_HERO","BACON_CURRENT_COMBAT_PLAYER_ID","BACON_DIED_LAST_COMBAT","BACON_MAX_MINIONS","BACON_TAVERN_TIER","BACON_HERO_CAN_BE_DRAFTED","BACON_FREE_REFRESH_COUNT","BACON_TRIPLE_UPGRADE_MINIONS","BACON_REFRESH_COST","BACON_UPGRADE_COST"]
for w in want:
    print(w, "=", vals.get(w, "MISSING"))
# any tags about cost / tier / refresh
import re
for k,v in sorted(vals.items()):
    if re.search(r'(COST|PRICE|TIER|REFRESH|GOLD|COIN|SHOP|ODDS)', k) and k.startswith("BACON"):
        print("  ", k, "=", v)
