import sys
sys.path.insert(0, r"C:\HSagent")
from hsrl2.db import CardDB
from hsrl2.scripts import REGISTRY
db = CardDB.load(r"C:\HSagent\data")
for cid in ["BG23_HERO_201p","BG25_HERO_103p","BG20_HERO_102p","BG20_HERO_280p5","BG27_HERO_801p2","BG23_HERO_304p","TB_BaconShop_HP_020","TB_BaconShop_HP_069"]:
    d = db.get(cid)
    nums = []
    for i in range(4):
        try: nums.append(d.num(i))
        except Exception as e: nums.append(f"ERR:{type(e).__name__}")
    print(cid, "| name=", getattr(d,"name",None), "| script=", cid in REGISTRY, "| nums=", nums)
print()
print("BG27_004 in REGISTRY:", "BG27_004" in REGISTRY, "| BG27_004t2:", "BG27_004t2" in REGISTRY)
print("Teron target check: engine hero power def id for BG25_HERO_103p ->", db.get("BG25_HERO_103p").hero_power_id if hasattr(db.get("BG25_HERO_103p"),"hero_power_id") else "n/a")
