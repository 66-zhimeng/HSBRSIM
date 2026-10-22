import re, pathlib
base = pathlib.Path(r"C:\Program Files (x86)\Hearthstone\Strings")
pats = {"cleave_cn": re.compile("顺劈"), "cleave_en": re.compile("CLEAVE", re.I),
        "cap": re.compile("DAMAGECAP|DAMAGE_CAP|伤害上限", re.I),
        "reborn_keys": re.compile("BACON_REBORN|KEYWORD_REBORN", re.I)}
for f in sorted(base.rglob("*.txt")):
    try: lines=f.read_text(encoding="utf-8-sig", errors="replace").splitlines()
    except Exception: continue
    for name,pat in pats.items():
        hits=[(i+1,l) for i,l in enumerate(lines) if pat.search(l)]
        if hits:
            print(f"\n=== {name} in {f.name} ({len(hits)}) ===")
            for i,l in hits[:12]: print(f"{i}: {l[:320]}")
# neighbors of DAMAGECAP label
g = (base/"zhCN"/"GLUE.txt").read_text(encoding="utf-8-sig", errors="replace").splitlines()
for i,l in enumerate(g):
    if "DAMAGECAP" in l:
        print("\n=== GLUE.txt neighbors ===")
        for j in range(max(0,i-6), min(len(g), i+7)):
            print(f"{j+1}: {g[j][:200]}")
