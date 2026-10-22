import re
p=r"hsdata\CardDefs.xml"
s=open(p,encoding="utf-8").read()
for cid in ["BG23_HERO_201p","TB_BaconShop_HP_020","BG25_HERO_103p","BG23_HERO_201pt","BG27_004t","BG20_HERO_102p","BG20_HERO_280p5","BG23_HERO_304p","BG27_HERO_801p2","BG31_HERO_005p","TB_BaconShop_HP_069"]:
    m=re.search(r'<Entity CardID="%s"[^>]*>(.*?)</Entity>'%re.escape(cid), s, re.S)
    if not m: print(cid,"NOT FOUND"); continue
    body=m.group(1)
    name=re.search(r'name="CARDNAME".*?<enUS>(.*?)</enUS>', body, re.S)
    text=re.search(r'name="CARDTEXT".*?<enUS>(.*?)</enUS>', body, re.S)
    print("="*50); print(cid, "|", name.group(1) if name else "?")
    print("  TEXT:", (text.group(1)[:600] if text else "(none)"))
