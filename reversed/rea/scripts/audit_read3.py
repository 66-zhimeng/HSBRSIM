import re, pathlib
out=pathlib.Path(r"reversed\rea\audit")
for name, pats in {
  "Gameplay.cs": r"PowerHistory|ProcessQueued|Network\.Get|OnPowerHistory|PowerProcessor",
  "ReconnectMgr.cs": r"PowerHistory|FastForward|GetGameState|PowerProcessor|Replay|ReconnectGame|Load",
  "TB_BaconShop_Tutorial.cs": r"Combat|Power|Attack|Damage|Network",
  "GameUtils.cs": r"ApplyPower",
}.items():
    f=out/name
    if not f.exists(): print("MISSING", name); continue
    ls=f.read_text(encoding="utf-8",errors="replace").splitlines()
    print("\n"+"="*72); print(name, "lines:", len(ls))
    for i,l in enumerate(ls,1):
        if re.search(pats,l):
            print(f"{i}: {l.strip()[:230]}")
