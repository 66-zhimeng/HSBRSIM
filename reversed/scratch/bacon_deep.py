"""深入看一个 Bacon_ 录像：英雄/实体/模式 tag。"""
import json
import re
import zipfile
from pathlib import Path

REPLAYS = Path.home() / "AppData" / "Roaming" / "HearthstoneDeckTracker" / "Replays"
ROOT = Path(r"C:\HSagent")
enums = json.loads((ROOT / "reversed" / "net" / "key_enums.json").read_text(encoding="utf-8-sig"))
inv = {v: k for k, v in enums["game_tag"]["values"].items()}


def read(f):
    with zipfile.ZipFile(f) as z:
        n = next((x for x in z.namelist()
                  if x.lower().endswith((".log", ".txt"))), None)
        return z.read(n).decode("utf-8", errors="replace") if n else ""


files = sorted(REPLAYS.glob("*.hdtreplay"))
target = None
for f in files:
    try:
        txt = read(f)
    except Exception:
        continue
    if "Bacon_" in txt:
        target = (f, txt)
        break
f, txt = target
lines = txt.splitlines()
print("file:", f.name[:50], "lines:", len(lines))

# 找英雄实体（HERO_ENTITY=74 等）
hero_ids = set(re.findall(r"HERO_ENTITY\]? value=(\d+)", txt)) | set(
    re.findall(r"tag=HERO_ENTITY value=(\d+)", txt))
print("hero entity ids:", hero_ids)
for hid in hero_ids:
    for i, ln in enumerate(lines):
        if f"id={hid} " in ln and "FULL_ENTITY" in ln:
            print(" hero entity line:", ln[:260])
            # 后续 tag 行
            for nxt in lines[i + 1:i + 12]:
                if "tag=" in nxt:
                    print("   ", nxt.split(" - ")[-1][:120])
            break

# 数字 tag 解析
cnt = {}
for m in re.finditer(r"tag=(\d{3,4}) value=(\d+)", txt):
    tid = int(m.group(1))
    cnt[tid] = cnt.get(tid, 0) + 1
print("\nnumeric tags -> names:")
for tid, c in sorted(cnt.items(), key=lambda x: -x[1])[:25]:
    print(f"  {tid} ({inv.get(tid,'?')}): {c}")

# 前 30 个完整实体行
full = [ln for ln in lines if "FULL_ENTITY - Creating" in ln or "FULL_ENTITY - Updating" in ln]
print("\nfull entity sample:")
for ln in full[:8]:
    print(" ", ln.split(" - ")[-1][:220])
