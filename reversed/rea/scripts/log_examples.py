import re
from pathlib import Path
LOG=Path(r"%USERPROFILE%\AppData\Roaming\HearthstoneDeckTracker\Logs")
want={"reborn":re.compile(r"\bReborn\b"),"venomous":re.compile(r"\bVenomous\b"),
      "cleave":re.compile(r"\bCleave\b"),"deathrattles":re.compile(r"Deathrattles:")}
found={k:[] for k in want}
for f in sorted(LOG.glob("hdt_log*.txt")):
    for i,line in enumerate(f.open(encoding="utf-8",errors="replace"),1):
        for k,pat in want.items():
            if len(found[k])<3 and pat.search(line):
                found[k].append(f"{f.name}:{i}: {line.strip()[:200]}")
for k,v in found.items():
    print("="*20, k)
    for x in v: print(" ", x)
