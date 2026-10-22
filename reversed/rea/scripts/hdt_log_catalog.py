"""只读：编目 HDT 日志（hdt_log*.txt）的可用信息，用于 damage cap / 争议机制取证。

用法：C:\\Python313\\python.exe reversed\\rea\\scripts\\hdt_log_catalog.py [--file NAME]
输出：模块/方法计数、消息模板计数、关键词命中、BG 会话结构样例。
"""
from __future__ import annotations

import argparse
import re
import sys
from collections import Counter, defaultdict
from pathlib import Path

LOG_DIR = Path(r"%USERPROFILE%\AppData\Roaming\HearthstoneDeckTracker\Logs")
LINE_RE = re.compile(r"^(\d{1,2}:\d{2}:\d{2})\|(\w+)\|([\w\.\<\>\+]+) >> (.*)$")
TEMPLATE_RE = re.compile(r"\d+")

KEYWORDS = [
    "damagecap", "damage cap", "damagecapenabled", "health", "armor",
    "damage", "lethal", "reborn", "poison", "venomous", "cleave",
    "deathrattle", "secret", "windfury", "divine", "taunt", "alive",
    "eliminat", "player count", "placement", "dead",
]


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
    ap.add_argument("--file", default=None)
    ap.add_argument("--top", type=int, default=60)
    args = ap.parse_args()

    files = sorted(LOG_DIR.glob("hdt_log*.txt"))
    if args.file:
        files = [LOG_DIR / args.file]
    print(f"log files: {len(files)}")
    total_lines = 0
    module_counter: Counter = Counter()
    template_counter: Counter = Counter()
    kw_files: dict[str, set] = defaultdict(set)
    kw_counts: Counter = Counter()
    input_blocks = 0
    validate_lines = 0
    turn_lines = 0

    for path in files:
        lines = read_lines(path)
        total_lines += len(lines)
        for line in lines:
            m = LINE_RE.match(line.rstrip("\n"))
            if not m:
                low = line.lower()
                for kw in KEYWORDS:
                    if kw in low:
                        kw_counts[kw] += 1
                        kw_files[kw].add(path.name)
                continue
            _ts, level, module, msg = m.groups()
            module_counter[module] += 1
            tmpl = TEMPLATE_RE.sub("#", msg.strip())[:110]
            template_counter[(module, tmpl)] += 1
            low = msg.lower()
            for kw in KEYWORDS:
                if kw in low:
                    kw_counts[kw] += 1
                    kw_files[kw].add(path.name)
            if "Simulation Input" in msg:
                input_blocks += 1
            if "ValidateSimulationResultAsync >> result=" in line:
                validate_lines += 1
            if "Player turn" in msg or "Opponent turn" in msg:
                turn_lines += 1

    print(f"total lines: {total_lines}")
    print(f"Simulation Input blocks: {input_blocks}")
    print(f"Validate result lines: {validate_lines}")
    print(f"turn lines: {turn_lines}")
    print("\n=== top modules ===")
    for module, count in module_counter.most_common(35):
        print(f"  {count:7d}  {module}")
    print("\n=== keyword hits (files=how many distinct log files) ===")
    for kw in KEYWORDS:
        if kw_counts[kw]:
            print(f"  {kw:16s} count={kw_counts[kw]:6d} files={len(kw_files[kw])}")
        else:
            print(f"  {kw:16s} count=0 files=0")
    print("\n=== BobsBuddy / BG / turn 相关消息模板 ===")
    for (module, tmpl), count in template_counter.most_common():
        if re.search(r"BobsBuddy|Battlegrounds|GameEventHandler|Health|Damage|"
                     r"Leaderboard|PlayerDied|Combat", module) and count >= 1:
            if count >= 3 or module.startswith("BobsBuddyInvoker"):
                print(f"  {count:6d}  [{module}] {tmpl}")
                if count == 1:
                    pass
    return 0


if __name__ == "__main__":
    sys.exit(main())
