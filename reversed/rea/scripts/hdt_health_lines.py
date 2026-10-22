import re
from pathlib import Path
LOG=Path(r"%USERPROFILE%\AppData\Roaming\HearthstoneDeckTracker\Logs")
files=sorted(LOG.glob("hdt_log*.txt"))
pat=re.compile(r"health", re.I)
n=0
for f in files:
    for i,line in enumerate(f.open(encoding="utf-8",errors="replace"),1):
        if pat.search(line):
            n+=1
            if n<=40: print(f"{f.name}:{i}: {line.rstrip()[:200]}")
print("total health lines:", n)
