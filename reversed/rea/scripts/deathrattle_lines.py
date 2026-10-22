import re
from pathlib import Path
LOG=Path(r"%USERPROFILE%\AppData\Roaming\HearthstoneDeckTracker\Logs")
pat=re.compile(r"Deathrattle", re.I)
n=0
for f in sorted(LOG.glob("hdt_log*.txt")):
    for i,line in enumerate(f.open(encoding="utf-8",errors="replace"),1):
        if pat.search(line):
            n+=1
            if n<=12: print(f"{f.name}:{i}: {line.strip()[:230]}")
print("total:", n)
