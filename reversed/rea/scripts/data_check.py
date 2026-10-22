import sys
sys.path.insert(0, r"C:\HSagent")
from hsrl2.db import CardDB
db = CardDB.load(r"C:\HSagent\data")
for cid in ["BG23_HERO_201pt","BG27_004t2","BG27_004_Gt2","BG27_004","BG27_004t","BG33_140","BG35_814","BG32_236","BGS_119","BG25_001"]:
    d=db.get(cid)
    print(cid, "->", None if d is None else (d.name, d.card_type, d.atk, d.health, d.tech_level, d.text[:60] if d.text else ""))
