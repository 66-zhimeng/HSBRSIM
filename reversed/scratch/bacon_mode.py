"""判定 Bacon_ 录像的真实模式：看玩家英雄卡 id 与 BG 特有实体。"""
import re
import zipfile
from collections import Counter
from pathlib import Path

REPLAYS = Path.home() / "AppData" / "Roaming" / "HearthstoneDeckTracker" / "Replays"


def read(f):
    with zipfile.ZipFile(f) as z:
        n = next((x for x in z.namelist()
                  if x.lower().endswith((".log", ".txt"))), None)
        return z.read(n).decode("utf-8", errors="replace") if n else ""


files = sorted(REPLAYS.glob("*.hdtreplay"))
cnt = 0
for f in files:
    try:
        txt = read(f)
    except Exception:
        continue
    if "Bacon_" not in txt:
        continue
    cnt += 1
    if cnt > 3:
        break
    print("=" * 80)
    print("file:", f.name[:60])
    # 玩家行
    players = [ln.strip() for ln in txt.splitlines() if "Player EntityID" in ln]
    for p in players[:4]:
        print(" ", p[:220])
    # 英雄实体
    heroes = sorted(set(re.findall(r"cardId=(BG[0-9A-Za-z_]*HERO[0-9A-Za-z_]*)", txt)))
    print(" hero card ids:", heroes[:10])
    # 全 CardID 前缀分布
    cards = Counter(re.findall(r"CardID=([A-Za-z]+)", txt))
    print(" CardID prefix:", cards.most_common(10))
    # BG 关键实体
    for sig in ["BG31_BOB", "Bacon_", "Battlegrounds", "BACON", "PREDAMAGE",
                "BlockType=PLAY", "BlockType=ATTACK", "SUB_SPELL_START",
                "GameEntity EntityID"]:
        print(f"  {sig}: {txt.count(sig)}")
    # game entity tags
    genus = re.findall(r"tag=([A-Z_0-9]+) value=([^\s]+)", txt)
    print(" first tags:", genus[:15])
