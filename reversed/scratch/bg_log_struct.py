"""分析真实 BG 录像（严格判定：出现 BG 卡 id）的 Power.log 结构。"""
import re
import zipfile
from collections import Counter
from pathlib import Path

REPLAYS = Path.home() / "AppData" / "Roaming" / "HearthstoneDeckTracker" / "Replays"
BG_RX = re.compile(r"CardID=BG[0-9A-Z_]")


def is_bg(txt: str) -> bool:
    return bool(BG_RX.search(txt)) or "TB_BaconShop" in txt


def read(f):
    with zipfile.ZipFile(f) as z:
        n = next((x for x in z.namelist()
                  if x.lower().endswith((".log", ".txt"))), None)
        return z.read(n).decode("utf-8", errors="replace") if n else ""


files = sorted(REPLAYS.glob("*.hdtreplay"))
bg = []
for f in files:
    try:
        txt = read(f)
    except Exception:
        continue
    if is_bg(txt):
        bg.append((f, txt))
print("total replays:", len(files), "strict BG:", len(bg))

# 结构统计（取最长的一个样本 + 全部聚合）
f, txt = max(bg, key=lambda x: len(x[1]))
print("sample lines:", txt.count("\n"))
block_types = Counter(re.findall(r"BlockType=(\w+)", txt))
print("block types:", block_types.most_common(15))
tags = Counter(re.findall(r"tag=([A-Z_0-9]+) ", txt))
print("top tags:", tags.most_common(25))
for marker in ["BACON_IN_COMBAT_PHASE", "TB_BaconShop", "PLAYER_TECH_LEVEL",
               "BACON_COMBAT_DAMAGE", "MAIN_COMBAT", "ATTACK", "DEATHRATTLE",
               "BACON_TRIPLE", "TAVERN", "BACON_"]:
    print(f"{marker}: {txt.count(marker)}")
attacks = [ln for ln in txt.splitlines() if "BlockType=ATTACK" in ln]
print("attack blocks:", len(attacks))
print("attack sample:", attacks[0][:260] if attacks else None)
turns = re.findall(r"tag=TURN value=(\d+)", txt)
print("TURN range:", (min(map(int, turns)), max(map(int, turns))) if turns else None)
cards = set(re.findall(r"CardID=([A-Za-z0-9_]+)", txt))
print("unique cards:", len(cards), "BG ids:", len([c for c in cards if c.startswith("BG")]))
print("BG id samples:", sorted(c for c in cards if c.startswith("BG"))[:10])

# tag enumID 数字型（客户端未命名 tag）出现频率
numtags = Counter(re.findall(r"tag=(\d{3,4}) ", txt))
print("numeric tags top:", numtags.most_common(15))

# 聚合：全部 BG 日志的关键标记均值
tot = Counter()
for _f, t in bg:
    for k in ["GameState.", "PowerTaskList.", "CREATE_GAME", "FULL_ENTITY",
              "TAG_CHANGE", "BLOCK_START", "BlockType=ATTACK",
              "BlockType=DEATHS", "BlockType=TRIGGER", "BACON_"]:
        tot[k] += t.count(k)
print("aggregate over", len(bg), "bg logs:", dict(tot))
