import re, pathlib
base = pathlib.Path(r"C:\Program Files (x86)\Hearthstone\Strings")
files = list(base.rglob("*.txt"))
print("locale dirs:", [p.name for p in base.iterdir()])
pat = re.compile(r"REBORN|CLEAVE|POISON|VENOM|DAMAGE_CAP|DAMAGECAP|BACON.*(CAP|DAMAGE)", re.I)
for f in files:
    if f.name not in ("GAMEPLAY.txt","GLOBAL.txt","GLUE.txt"): continue
    try: lines=f.read_text(encoding="utf-8-sig", errors="replace").splitlines()
    except Exception as e: print(f, e); continue
    hits=[(i+1,l) for i,l in enumerate(lines) if pat.search(l)]
    print(f"\n=== {f} (lines={len(lines)}) hits={len(hits)} ===")
    for i,l in hits[:40]:
        print(f"{i}: {l[:300]}")
