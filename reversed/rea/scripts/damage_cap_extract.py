"""只读：从游戏侧 Power.log 提取 `BACON_COMBAT_DAMAGE_CAP`(2089) /
`BACON_COMBAT_DAMAGE_CAP_ENABLED`(3403) 的逐回合观察（含存活人数推断）。

为什么需要它：HDT BobsBuddy 会话日志不打印 cap/血量（见
`reversed/rea/damage_cap_evidence.json`），权威值只能从游戏的 Power.log
（GameEntity tag）取。当前机器没有 Power.log；本脚本先在**合成样本**上自测，
拿到真实日志后可直接跑。

用法：
  REM 自测（不需要真实日志）
  C:\\Python313\\python.exe reversed\\rea\\scripts\\damage_cap_extract.py --self-test

  REM 真实日志
  C:\\Python313\\python.exe reversed\\rea\\scripts\\damage_cap_extract.py ^
      --powerlog "C:\\Program Files (x86)\\Hearthstone\\Logs\\Power.log" --markdown

输出：JSON 到 stdout（`--out PATH` 可写文件；默认不写项目文件）。
"""
from __future__ import annotations

import argparse
import json
import re
import sys
from collections import Counter, defaultdict
from pathlib import Path

CAP_TAG = 2089
ENABLED_TAG = 3403
PLAYSTATE_PLAYING = 1

#: 真实 Power.log 用**符号 tag 名**（`tag=BACON_COMBAT_DAMAGE_CAP value=2`），
#: 旧脚本只认数字 2089/3403 → 真实日志 0 行。两种形式都兼容。
TAG_NAME_TO_ID = {
    "BACON_COMBAT_DAMAGE_CAP": CAP_TAG,
    "BACON_COMBAT_DAMAGE_CAP_ENABLED": ENABLED_TAG,
    "2089": CAP_TAG,
    "3403": ENABLED_TAG,
}
RE_TAG_VALUE = re.compile(
    r"tag=(BACON_COMBAT_DAMAGE_CAP_ENABLED|BACON_COMBAT_DAMAGE_CAP|3403|2089)"
    r"\s+value=(-?\d+)")
RE_TURN = re.compile(r"tag=TURN\s+value=(\d+)")
RE_PLAYER_ENTITY = re.compile(r"\bPlayer\s+EntityID=(\d+)")
RE_ENTITY_ID = re.compile(r"\bEntityID=(\d+)")
#: 真实日志 PLAYSTATE 是符号值（PLAYING/LEFT/...），合成样本是数字
PLAYSTATE_VALUES = {
    "PLAYING": 1, "LEFT": 3, "DISCONNECTED": 2, "CONCEDED": 4,
    "OUT": 5, "UNKNOWN": 0,
}
RE_PLAYSTATE = re.compile(r"tag=PLAYSTATE\s+value=(\S+)")
RE_TAG_CHANGE_ENTITY = re.compile(r"TAG_CHANGE\s+Entity=(\S+)")

DEFAULT_CANDIDATES = [
    Path(r"C:\Program Files (x86)\Hearthstone\Logs\Power.log"),
    Path.home() / "AppData/Local/Blizzard/Hearthstone/Logs/Power.log",
    Path.home() / "Documents/Hearthstone/Logs/Power.log",
]


def parse_powerlog(text: str) -> dict:
    """返回 {observations, by_turn_alive, players}。"""
    observations = []
    by_turn_alive: dict[tuple, set] = defaultdict(set)
    turn: int | None = None
    cap: int | None = None
    enabled: int | None = None
    players: dict[str, int] = {}          # player entity id -> last playstate
    current_player: str | None = None
    game_entity_id: str | None = None

    for lineno, line in enumerate(text.splitlines(), 1):
        m_game = re.search(r"GameEntity EntityID=(\d+)", line)
        if m_game:
            game_entity_id = m_game.group(1)
        m_player = RE_PLAYER_ENTITY.search(line)
        if m_player:
            current_player = m_player.group(1)
        m_play = RE_PLAYSTATE.search(line)
        if m_play:
            # 两种形式：Player 块内的裸 tag=PLAYSTATE（归属最近 Player
            # EntityID=）与 TAG_CHANGE Entity=...（归属该 id）；值可能是
            # 符号（PLAYING）或数字（合成样本）。
            target = current_player
            m_change = re.search(r"TAG_CHANGE\s+Entity=(.+?)\s+tag=", line)
            if m_change:
                ref = m_change.group(1).strip()
                m_id = re.search(r"\bid=(\d+)", ref)
                if m_id:
                    target = m_id.group(1)
                elif ref.isdigit():
                    target = ref
            raw = m_play.group(1)
            value = PLAYSTATE_VALUES.get(raw)
            if value is None:
                try:
                    value = int(raw)
                except ValueError:
                    value = None
            if target is not None and value is not None:
                players[target] = value
            continue  # playstate 行不同时含 cap
        if "tag=TURN" in line:
            m_turn = RE_TURN.search(line)
            if m_turn:
                turn = int(m_turn.group(1))
                continue
        m_tag = RE_TAG_VALUE.search(line)
        if not m_tag:
            continue
        tag = TAG_NAME_TO_ID[m_tag.group(1)]
        value = int(m_tag.group(2))
        is_change = "TAG_CHANGE" in line
        entity = "?"
        m_ent = re.search(r"TAG_CHANGE\s+Entity=(.+?)\s+tag=", line)
        if m_ent:
            ref = m_ent.group(1).strip()
            m_id = re.search(r"\bid=(\d+)", ref)
            if m_id:
                entity = m_id.group(1)
            elif ref.isdigit():
                entity = ref
            elif "GameEntity" in ref and game_entity_id:
                entity = game_entity_id
        if tag == CAP_TAG:
            cap = value
        else:
            enabled = value
        alive = None
        states = [s for s in players.values() if s in (0, 1, 2, 3, 4, 5)]
        if states:
            alive = sum(1 for s in states if s == PLAYSTATE_PLAYING)
        if cap is not None:
            observations.append({
                "line": lineno,
                "turn": turn,
                "alive_players": alive,
                "cap": cap,
                "enabled": enabled,
                "event": "tag_change" if is_change else "initial",
                "entity": entity,
            })
            # 观察表只收 cap 事件（enabled 变化单独留在 observations，避免重复行）
            if tag == CAP_TAG and turn is not None:
                by_turn_alive[(turn, alive)].add(cap)

    return {
        "observations": observations,
        "by_turn_alive": [
            {"turn": t, "alive_players": a, "caps": sorted(caps)}
            for (t, a), caps in sorted(by_turn_alive.items(), key=lambda kv: (kv[0][0], kv[0][1]))
        ],
        "players_final_playstate": players,
    }


def summarize(result: dict) -> dict:
    caps = Counter(o["cap"] for o in result["observations"])
    enabled = Counter(o["enabled"] for o in result["observations"] if o["enabled"] is not None)
    return {
        "observations": len(result["observations"]),
        "distinct_caps": dict(sorted(caps.items())),
        "enabled_values": dict(enabled),
        "turn_alive_rows": result["by_turn_alive"],
        "players_final_playstate": result["players_final_playstate"],
    }


def markdown_table(rows) -> str:
    out = ["| turn | 存活人数 | cap 值 | 观察次数 |", "|------|----------|--------|----------|"]
    for row in rows:
        out.append(f"| {row['turn']} | {row['alive_players']} | "
                   f"{', '.join(map(str, row['caps']))} | - |")
    return "\n".join(out)


SELF_TEST_TEXT = """\
D 10:00:00.0000000 PowerTaskList.DebugPrintPower() -     CREATE_GAME
D 10:00:00.0000001 PowerTaskList.DebugPrintPower() -         GameEntity EntityID=1
D 10:00:00.0000002 PowerTaskList.DebugPrintPower() -             tag=TURN value=1
D 10:00:00.0000003 PowerTaskList.DebugPrintPower() -             tag=2089 value=5
D 10:00:00.0000004 PowerTaskList.DebugPrintPower() -             tag=3403 value=1
D 10:00:00.0000005 PowerTaskList.DebugPrintPower() -         Player EntityID=2 PlayerID=1
D 10:00:00.0000006 PowerTaskList.DebugPrintPower() -             tag=PLAYSTATE value=1
D 10:00:00.0000007 PowerTaskList.DebugPrintPower() -         Player EntityID=3 PlayerID=2
D 10:00:00.0000008 PowerTaskList.DebugPrintPower() -             tag=PLAYSTATE value=1
D 10:00:05.0000000 PowerTaskList.DebugPrintPower() - TAG_CHANGE Entity=GameEntity tag=TURN value=5
D 10:00:05.0000001 PowerTaskList.DebugPrintPower() - TAG_CHANGE Entity=GameEntity tag=2089 value=10
D 10:00:20.0000000 PowerTaskList.DebugPrintPower() - TAG_CHANGE Entity=GameEntity tag=TURN value=9
D 10:00:20.0000001 PowerTaskList.DebugPrintPower() - TAG_CHANGE Entity=GameEntity tag=2089 value=15
D 10:00:30.0000000 PowerTaskList.DebugPrintPower() - TAG_CHANGE Entity=[entityName=X id=3] tag=PLAYSTATE value=3
D 10:00:30.0000001 PowerTaskList.DebugPrintPower() - TAG_CHANGE Entity=GameEntity tag=3403 value=0
D 10:00:31.0000000 PowerTaskList.DebugPrintPower() - TAG_CHANGE Entity=GameEntity tag=2089 value=5
"""


REAL_FORMAT_TEXT = """\
D 21:16:02.5176110 GameState.DebugPrintPower() - CREATE_GAME
D 21:16:02.5176110 GameState.DebugPrintPower() -     GameEntity EntityID=7
D 21:16:02.5176110 GameState.DebugPrintPower() -         tag=TURN value=1
D 21:16:02.5176110 GameState.DebugPrintPower() -         tag=BACON_COMBAT_DAMAGE_CAP value=2
D 21:16:02.5176110 GameState.DebugPrintPower() -         tag=BACON_COMBAT_DAMAGE_CAP_ENABLED value=1
D 21:16:02.5176110 GameState.DebugPrintPower() -     Player EntityID=8 PlayerID=3
D 21:16:02.5176110 GameState.DebugPrintPower() -         tag=PLAYSTATE value=PLAYING
D 21:16:02.5176110 GameState.DebugPrintPower() -     Player EntityID=9 PlayerID=11
D 21:16:02.5176110 GameState.DebugPrintPower() -         tag=PLAYSTATE value=PLAYING
D 21:17:05.0000000 GameState.DebugPrintPower() - TAG_CHANGE Entity=7 tag=TURN value=5
D 21:17:05.0000002 GameState.DebugPrintPower() - TAG_CHANGE Entity=8 tag=PLAYSTATE value=LEFT
D 21:17:05.0000001 GameState.DebugPrintPower() - TAG_CHANGE Entity=7 tag=BACON_COMBAT_DAMAGE_CAP value=5
D 21:17:05.0000003 GameState.DebugPrintPower() - TAG_CHANGE Entity=7 tag=BACON_COMBAT_DAMAGE_CAP value=10
"""


def self_test() -> int:
    result = parse_powerlog(SELF_TEST_TEXT)
    summary = summarize(result)
    print(json.dumps(summary, ensure_ascii=False, indent=1))
    rows = {(r["turn"], r["alive_players"]): r["caps"] for r in summary["turn_alive_rows"]}
    checks = [
        # create_game 初始块：cap 先于 Player 实体出现 → 存活人数未知（None）
        ("turn1 cap=5 (alive=None)", rows.get((1, None)) == [5]),
        ("turn5 cap=10 (alive=2)", rows.get((5, 2)) == [10]),
        ("turn9 cap=15 (alive=2)", rows.get((9, 2)) == [15]),
        ("turn9 cap=5 after loss (alive=1)", rows.get((9, 1)) == [5]),
        ("enabled 事件含 0", summary["enabled_values"].get(0, 0) >= 1),
    ]
    # 真实格式（符号 tag 名 / Entity=7 / PLAYING 符号值 / 缩进）
    real = summarize(parse_powerlog(REAL_FORMAT_TEXT))
    real_rows = {(r["turn"], r["alive_players"]): r["caps"]
                 for r in real["turn_alive_rows"]}
    checks += [
        ("real: turn1 cap=2 (alive=None)", real_rows.get((1, None)) == [2]),
        ("real: turn5 caps=5,10 alive=1", real_rows.get((5, 1)) == [5, 10]),
        ("real: enabled=1", real["enabled_values"].get(1, 0) >= 1),
    ]
    ok = True
    for name, passed in checks:
        print(("PASS" if passed else "FAIL"), name)
        ok = ok and passed
    print("SELF-TEST:", "PASS" if ok else "FAIL")
    return 0 if ok else 1


def discover() -> Path | None:
    for p in DEFAULT_CANDIDATES:
        if p.exists():
            return p
    return None


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--powerlog", default=None)
    ap.add_argument("--out", default=None)
    ap.add_argument("--markdown", action="store_true")
    ap.add_argument("--self-test", action="store_true")
    args = ap.parse_args()
    if args.self_test:
        return self_test()
    path = Path(args.powerlog) if args.powerlog else discover()
    if path is None or not path.exists():
        print(json.dumps({
            "error": "Power.log not found",
            "candidates": [str(p) for p in DEFAULT_CANDIDATES],
            "next": "运行游戏并开启 Power.log 后重试；当前可先用 --self-test 验证解析器",
        }, ensure_ascii=False, indent=1))
        return 2
    text = path.read_text(encoding="utf-8", errors="replace")
    result = parse_powerlog(text)
    summary = summarize(result)
    summary["source"] = str(path)
    payload = {"summary": summary, "observations": result["observations"][:500]}
    text_out = json.dumps(payload, ensure_ascii=False, indent=1)
    if args.out:
        Path(args.out).write_text(text_out, encoding="utf-8")
        print(f"written {args.out}")
    else:
        print(text_out)
    if args.markdown:
        print("\n" + markdown_table(summary["turn_alive_rows"]))
    return 0


if __name__ == "__main__":
    sys.exit(main())
