import re, pathlib
targets = ["BG25_HERO_103p","BG23_HERO_201p","TB_BaconShop_HP_020","BG23_HERO_304p","BG20_HERO_280p5","BG20_HERO_102p","BG27_HERO_801p2","TB_BaconShop_HP_069","BG31_HERO_005p"]
root = pathlib.Path(r"hsrl2\scripts")
files = list(root.rglob("*.py"))
for t in targets:
    hits=[]
    for f in files:
        try: s=f.read_text(encoding="utf-8")
        except: continue
        if t in s:
            hits.append(f)
    print("="*60)
    print(t, "->", [str(h) for h in hits])
    # print class blocks from first hit
    for f in hits:
        s=f.read_text(encoding="utf-8")
        for m in re.finditer(r'^class\s+(\w+)[^\n]*\n(?:(?:    |\t)[^\n]*\n|\n)*', s, re.M):
            if t in m.group(0):
                print(f"  [{f.name}] {m.group(1)}")
                print("\n".join("    "+ln for ln in m.group(0).splitlines()[:40]))
                break
