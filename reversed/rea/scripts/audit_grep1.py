import re
from pathlib import Path
base = Path(r"reversed\net\Assembly-CSharp")
types = (base/"types.tsv").read_text(encoding="utf-8-sig", errors="replace").splitlines()
methods = (base/"methods.tsv").read_text(encoding="utf-8-sig", errors="replace").splitlines()
print("types:", len(types), "methods:", len(methods))

def show(title, lines, limit=40):
    print("\n=== " + title + f" ({len(lines)}) ===")
    for ln in lines[:limit]:
        print("  ", ln.strip()[:300])

show("types containing BaconCombat|BattlegroundsCombat|DamageCap",
     [l for l in types if re.search(r"BaconCombat|BattlegroundsCombat|DamageCap", l)])
show("methods containing BaconCombat|BattlegroundsCombat|DamageCap",
     [l for l in methods if re.search(r"BaconCombat|BattlegroundsCombat|DamageCap", l)])

pat_bg_combat = re.compile(r"Bacon.*Combat|Battleground.*Combat|Combat.*Bacon|Combat.*Battleground", re.I)
show("types Bacon/Battleground + Combat", [l for l in types if pat_bg_combat.search(l)], 60)
show("methods Bacon/Battleground + Combat", [l for l in methods if pat_bg_combat.search(l)], 60)
