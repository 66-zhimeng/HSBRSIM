"""判定 HDT 录像是否为酒馆战棋：多信号统计。"""
import re
import zipfile
from collections import Counter
from pathlib import Path

REPLAYS = Path.home() / "AppData" / "Roaming" / "HearthstoneDeckTracker" / "Replays"
markers = ["TB_BaconShop", "BG20_HERO", "BG", "Bacon", "BATTLEGROUNDS",
           "Battlegrounds", "CardID=BG", "SHOW_ENTITY", "BACON_IN_COMBAT_PHASE"]


def read(f):
    with zipfile.ZipFile(f) as z:
        n = next((x for x in z.namelist()
                  if x.lower().endswith((".log", ".txt"))), None)
        return z.read(n).decode("utf-8", errors="replace") if n else ""


files = sorted(REPLAYS.glob("*.hdtreplay"))
rows = []
for f in files:
    try:
        txt = read(f)
    except Exception:
        continue
    c = {m: txt.count(m) for m in markers}
    c["bg_cards"] = len(set(re.findall(r"CardID=(BG[A-Za-z0-9_]+)", txt)))
    c["tb_bacon"] = txt.count("TB_BaconShop")
    c["hero_bg"] = len(set(re.findall(r"CardID=(BG[0-9]+_HERO[A-Za-z0-9_]*)", txt)))
    c["lines"] = txt.count("\n")
    c["name"] = f.name
    rows.append(c)

# 打印各信号的分布
for m in markers + ["bg_cards", "hero_bg"]:
    vals = [r[m] for r in rows]
    nz = sum(1 for v in vals if v)
    print(f"{m}: files with >0 = {nz}")

print("\n=== 有 TB_BaconShop 的文件（前 10 统计） ===")
sel = [r for r in rows if r["tb_bacon"]]
print("count:", len(sel))
for r in sel[:10]:
    print(f"  lines={r['lines']} bg_cards={r['bg_cards']} hero_bg={r['hero_bg']} "
          f"TB_BaconShop={r['tb_bacon']} Bacon={r['Bacon']}")

print("\n=== bg_cards>0 的文件 ===")
sel2 = [r for r in rows if r["bg_cards"]]
print("count:", len(sel2))
for r in sel2[:10]:
    print(f"  lines={r['lines']} bg_cards={r['bg_cards']} hero_bg={r['hero_bg']} "
          f"TB_BaconShop={r['tb_bacon']}")

print("\n=== hero_bg>0 的文件 ===")
sel3 = [r for r in rows if r["hero_bg"]]
print("count:", len(sel3))
for r in sel3[:10]:
    print(f"  lines={r['lines']} bg_cards={r['bg_cards']} hero_bg={r['hero_bg']} "
          f"TB_BaconShop={r['tb_bacon']}")
