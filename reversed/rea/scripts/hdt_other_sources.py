import re, pathlib
hdt = pathlib.Path(r"%USERPROFILE%\AppData\Roaming\HearthstoneDeckTracker")
pat = re.compile(r"damage|health|armor|damagecap", re.I)
files = [hdt/"BgsLastGames.xml", hdt/"DeckStats.xml", hdt/"DefaultDeckStats.xml", hdt/"config.xml"]
for f in files:
    if not f.exists(): print(f.name, "MISSING"); continue
    s = f.read_text(encoding="utf-8-sig", errors="replace")
    hits = [(m.start(), m.group(0)) for m in pat.finditer(s)]
    print(f"{f.name}: size={len(s)} token_hits={len(hits)}")
    for pos, tok in hits[:8]:
        print("   ...", re.sub(r"\s+"," ", s[max(0,pos-90):pos+90])[:190])
# distinct attributes in BgsLastGames
s=(hdt/"BgsLastGames.xml").read_text(encoding="utf-8-sig", errors="replace")
attrs=sorted(set(re.findall(r'([A-Za-z_][A-Za-z0-9_]*)="', s[:400000])))
print("\nBgsLastGames attrs:", attrs)
print("Game count:", s.count("<Game "))
