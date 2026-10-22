"""只读：列出 HDT 日志里 GameEventHandler / GameV2 / BattlegroundsBoardState /
TagChangeActions 的全部消息模板，判断是否存在「英雄血量/死亡/伤害」可观测项。

用法：C:\\Python313\\python.exe reversed\\rea\\scripts\\hdt_event_catalog.py [--module SUBSTR]
"""
from __future__ import annotations

import argparse
import re
import sys
from collections import Counter
from pathlib import Path

LOG_DIR = Path(r"%USERPROFILE%\AppData\Roaming\HearthstoneDeckTracker\Logs")
LINE_RE = re.compile(r"^(\d{1,2}:\d{2}:\d{2})\|(\w+)\|([\w\.\<\>\+]+) >> (.*)$")
NUM_RE = re.compile(r"-?\d+(?:\.\d+)?")
WANT = ("GameEventHandler", "GameV2", "BattlegroundsBoardState", "TagChangeActions",
        "BobsBuddyDisplay", "Overlay", "PlayerLeaderboard", "GameV2.Player",
        "Player", "Bacon")


def read_lines(path: Path):
    for enc in ("utf-8-sig", "utf-8", "cp1252"):
        try:
            with path.open("r", encoding=enc, errors="strict") as fh:
                return fh.readlines()
        except (UnicodeDecodeError, LookupError):
            continue
    with path.open("r", encoding="utf-8", errors="replace") as fh:
        return fh.readlines()


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--module", default=None)
    args = ap.parse_args()
    files = sorted(LOG_DIR.glob("hdt_log*.txt"))
    tmpl_counter: Counter = Counter()
    samples: dict[tuple, str] = {}
    for path in files:
        for i, line in enumerate(read_lines(path), 1):
            m = LINE_RE.match(line.rstrip("\n"))
            if not m:
                continue
            _ts, _lvl, module, msg = m.groups()
            if args.module:
                if args.module.lower() not in module.lower():
                    continue
            elif not any(w.lower() in module.lower() for w in WANT):
                continue
            tmpl = NUM_RE.sub("#", msg.strip())[:130]
            key = (module, tmpl)
            tmpl_counter[key] += 1
            samples.setdefault(key, f"{path.name}:{i}: {msg.strip()[:150]}")
    for (module, tmpl), count in tmpl_counter.most_common(200):
        print(f"{count:6d}  [{module}] {tmpl}")
        if count <= 2:
            print(f"          e.g. {samples[(module, tmpl)]}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
