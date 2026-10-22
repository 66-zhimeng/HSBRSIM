import json
import re
from pathlib import Path

new = Path(r"C:\HSagent\reversed\hsdata_255130\RaceTagMap.xml").read_text(encoding="utf-8")
old = Path(r"C:\HSagent\hsdata\RaceTagMap.xml").read_text(encoding="utf-8")
print("=== NEW RaceTagMap ===")
print(new)
print("=== OLD diff ===")
nr = dict(re.findall(r'<Race race="(\w+)" tag="(\d+)"', new))
orr = dict(re.findall(r'<Race race="(\w+)" tag="(\d+)"', old))
for k in sorted(set(nr) | set(orr)):
    if nr.get(k) != orr.get(k):
        print(f"  {k}: old={orr.get(k)} new={nr.get(k)}")

cards = json.loads(Path(r"C:\HSagent\reversed\data_255130\bg_pool_minions.json").read_text(encoding="utf-8"))
sel = [c for c in cards if c.get("card_race") == 126]
print("=== ABERRATION pool minions:", len(sel))
for c in sel[:8]:
    print(" ", c["id"], c.get("name"), "tier", c.get("tech_level"), "atk", c.get("atk"), "hp", c.get("health"), "race_tags", c.get("race_tags"), "|", (c.get("text") or "")[:80])
