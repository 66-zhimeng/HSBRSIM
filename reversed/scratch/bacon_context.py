"""定位 'Bacon' 在录像中出现的位置，确认 BG 录像判定信号。"""
import re
import zipfile
from pathlib import Path

REPLAYS = Path.home() / "AppData" / "Roaming" / "HearthstoneDeckTracker" / "Replays"


def read(f):
    with zipfile.ZipFile(f) as z:
        n = next((x for x in z.namelist()
                  if x.lower().endswith((".log", ".txt"))), None)
        return z.read(n).decode("utf-8", errors="replace") if n else ""


files = sorted(REPLAYS.glob("*.hdtreplay"))
shown = 0
for f in files:
    try:
        txt = read(f)
    except Exception:
        continue
    if "Bacon" not in txt:
        continue
    idx = txt.find("Bacon")
    print("file:", f.name[:40] + "...", "lines:", txt.count("\n"))
    print("context:", txt[max(0, idx - 200):idx + 200].replace("\n", " | ")[:500])
    # 找 BG 特征
    for sig in ["BG31_BOB", "BG20", "BGDUO", "GameType", "GAME_TYPE",
                "playerHero", "TAVERN", "TIER", "BACON", "Bacon"]:
        print(f"  {sig}: {txt.count(sig)}")
    shown += 1
    if shown >= 2:
        break
