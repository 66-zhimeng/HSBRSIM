import re
s=open(r"hsdata\CardDefs.xml",encoding="utf-8").read()
pats = ["Also damages the minions next to",
        "Destroy any minion damaged by",
        "When this minion dies, resummon it with",
        "resummon it with 1 Health",
        "Reborn"]
for p in pats:
    hits = [m.start() for m in re.finditer(re.escape(p), s)]
    print("="*60); print("PATTERN:", p, "hits:", len(hits))
    for h in hits[:3]:
        seg = s[max(0,h-400):h+250]
        ids = re.findall(r'CardID="([^"]+)"', seg)
        print("  nearby cardids:", ids[-3:])
        print("  ...", re.sub(r"\s+"," ", seg)[-260:])
