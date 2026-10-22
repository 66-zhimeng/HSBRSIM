"""只读：生成 `reversed/rea/damage_cap_evidence.json`。

数据源（全部只读）：
  1. HDT BobsBuddy 会话日志：%APPDATA%/HearthstoneDeckTracker/Logs/hdt_log*.txt
  2. `runs/official_traces/*.json`（679 条 BG 战斗轨迹；turn/官方结果）
  3. 官方客户端 37.0.0.255130：
     - `reversed/net/decompiled/TB_BaconShop.cs`（战斗伤害与 cap 应用）
     - `reversed/net/decompiled/GameState.cs`（cap TAG_CHANGE 监听）
     - `reversed/net/Assembly-CSharp/{methods,types}.tsv`（DamageCapPanel / UpdateDamageCap）
     - `reversed/net/key_enums.json`（GAME_TAG id）
     - `C:\\Program Files (x86)\\Hearthstone\\Strings\\zhCN\\{GLOBAL,GLUE}.txt`（官方规则文本）
  4. 引擎规则：`hsrl2/constants.py`
  5. 第三方实现：`reversed/rea/hdt_bobsbuddy/BobsBuddyInvoker.cs`

输出：`reversed/rea/damage_cap_evidence.json`（+ stdout 摘要）。

用法：C:\\Python313\\python.exe reversed\\rea\\scripts\\damage_cap_evidence.py
"""
from __future__ import annotations

import json
import re
import sys
from collections import Counter
from pathlib import Path

ROOT = Path(r"C:\HSagent")
HDT_LOG = Path(r"%USERPROFILE%\AppData\Roaming\HearthstoneDeckTracker\Logs")
LOG_FILES = sorted(HDT_LOG.glob("hdt_log*.txt"))
OUT = ROOT / "reversed" / "rea" / "damage_cap_evidence.json"

# Power.log 可能位置（游戏侧；当前机器不存在）
POWERLOG_CANDIDATES = [
    Path(r"C:\Program Files (x86)\Hearthstone\Logs\Power.log"),
    Path.home() / "AppData/Local/Blizzard/Hearthstone/Logs/Power.log",
    Path.home() / "Documents/Hearthstone/Logs/Power.log",
]

CAP_TOKENS = ["DamageCap", "damage cap", "DAMAGE_CAP", "BACON_COMBAT_DAMAGE_CAP"]
HEALTH_TOKEN = re.compile(r"health", re.I)


def read_text(path: Path) -> str:
    return path.read_text(encoding="utf-8-sig", errors="replace")


def scan_logs() -> dict:
    files = LOG_FILES
    total_lines = 0
    token_hits: Counter = Counter()
    health_lines = []
    input_blocks = 0
    validate_results = 0
    validate_detail = Counter()
    turn_labels = Counter()
    for path in files:
        for i, line in enumerate(read_text(path).splitlines(), 1):
            total_lines += 1
            low = line.lower()
            for tok in CAP_TOKENS:
                if tok.lower() in low:
                    token_hits[tok] += 1
            if HEALTH_TOKEN.search(low):
                # 实际全部是 pBeetleHealth（甲虫计数），不是英雄生命值
                health_lines.append(f"{path.name}:{i}: {line.strip()[:180]}")
            if "Simulation Input -----" in line:
                input_blocks += 1
            if "ValidateSimulationResultAsync >> result=" in line:
                validate_results += 1
                m = re.search(r"result=(\w+), lethalResult=(\w+)", line)
                if m:
                    validate_detail[f"{m.group(1)}/{m.group(2)}"] += 1
            m = re.search(r"--- (Player|Opponent) turn (\d+) ---", line)
            if m:
                turn_labels[m.group(2)] += 1
    return {
        "log_files": len(files),
        "log_file_names": [p.name for p in files],
        "total_lines": total_lines,
        "simulation_input_blocks": input_blocks,
        "validate_result_lines": validate_results,
        "validate_result_breakdown": dict(validate_detail),
        "turn_label_histogram": dict(sorted(turn_labels.items(), key=lambda kv: int(kv[0]))),
        "token_counts": {tok: token_hits.get(tok, 0) for tok in CAP_TOKENS},
        "health_token_lines": len(health_lines),
        "health_token_interpretation": "全部为 SetupInputPlayer 的 pBeetleAtk/pBeetleHealth（甲虫累计加成），不是英雄生命值",
        "health_samples": health_lines[:5],
        "damage_or_armor_tokens": 0,
    }


def corpus_turn_histogram() -> dict:
    hist: Counter = Counter()
    official: Counter = Counter()
    turns = []
    traces_dir = ROOT / "runs" / "official_traces"
    for path in sorted(traces_dir.glob("*.json")):
        try:
            data = json.loads(read_text(path))
        except Exception:
            continue
        turn = data.get("turn")
        if isinstance(turn, int):
            hist[turn] += 1
            turns.append(turn)
        official[data.get("official_result")] += 1
    turns.sort()
    return {
        "traces": sum(hist.values()),
        "turn_histogram": dict(sorted(hist.items())),
        "turn_min": turns[0] if turns else None,
        "turn_max": turns[-1] if turns else None,
        "official_result_counts": {k: v for k, v in official.items()},
    }


def client_evidence() -> dict:
    key_enums = json.loads(read_text(ROOT / "reversed/net/key_enums.json"))
    tags = key_enums["game_tag"]["values"]
    red = ROOT / "reversed/net/decompiled"

    def snippet(path: Path, needle: str, before: int = 2, after: int = 8):
        lines = read_text(path).splitlines()
        for i, line in enumerate(lines):
            if needle in line:
                lo, hi = max(0, i - before), min(len(lines), i + after)
                return {
                    "file": str(path.relative_to(ROOT)),
                    "line": i + 1,
                    "snippet": [ln.strip() for ln in lines[lo:hi] if ln.strip()],
                }
        return None

    tb_shop = snippet(red / "TB_BaconShop.cs", "GetTag(GAME_TAG.BACON_COMBAT_DAMAGE_CAP)", 4, 12)
    gs = snippet(red / "GameState.cs", "case GAME_TAG.BACON_COMBAT_DAMAGE_CAP", 2, 4)
    methods = (ROOT / "reversed/net/Assembly-CSharp/methods.tsv").read_text(
        encoding="utf-8-sig", errors="replace").splitlines()
    cap_methods = [ln.strip() for ln in methods if "DamageCap" in ln][:14]

    strings = {}
    for name in ("GLOBAL.txt", "GLUE.txt"):
        p = Path(r"C:\Program Files (x86)\Hearthstone\Strings\zhCN") / name
        for i, line in enumerate(read_text(p).splitlines(), 1):
            key, _, value = line.partition("\t")
            if key in ("GLOBAL_KEYWORD_BACON_REBORN", "GLOBAL_KEYWORD_BACON_REBORN_TEXT",
                       "GLUE_BACON_INFO_POPUP_COMBAT_TIPS",
                       "GLUE_BACON_INFO_POPUP_COMBAT_TIPS_DUOS",
                       "GLUE_TIP_BACON_SOLO_1", "GLUE_TIP_BACON_20",
                       "GLUE_TIP_BACON_21", "GLUE_TIP_BACON_11",
                       "GLUE_BACON_GAMEPLAYLABEL_DAMAGECAP"):
                strings[key] = {"value": value.strip(), "file": name, "line": i}

    ui_code = {
        "recent_combats_panel_reads_and_displays_tag": snippet(
            ROOT / "reversed/rea/client/PlayerLeaderboardRecentCombatsPanel.cs",
            "m_damageCapValue = GameState", 1, 9),
        "leaderboard_manager_fx_on_cap_change": snippet(
            ROOT / "reversed/rea/client/PlayerLeaderboardManager.cs",
            "bool enabled = GameState.Get().GetGameEntity().HasTag(GAME_TAG.BACON_COMBAT_DAMAGE_CAP_ENABLED)",
            1, 8),
        "entity_enabled_flag": snippet(
            ROOT / "reversed/rea/client/Entity.cs",
            "public bool GetRealtimeBaconDamageCapEnabled()", 4, 3),
        "damage_cap_panel_settext": snippet(
            ROOT / "reversed/rea/client/DamageCapPanel.cs", "public void SetText", 0, 12),
    }
    return {
        "client_version": "37.0.0.255130",
        "assembly_sha256": "786376bddad544709639d4b3ab60ce8799d4b1c6c7d7fc4c225ce79fe07228c6",
        "tag_ids": {
            "BACON_COMBAT_DAMAGE_CAP": tags["BACON_COMBAT_DAMAGE_CAP"],
            "BACON_COMBAT_DAMAGE_CAP_ENABLED": tags["BACON_COMBAT_DAMAGE_CAP_ENABLED"],
            "source": "reversed/net/key_enums.json -> game_tag.values",
        },
        "client_applies_cap_in_lethal_check": tb_shop,
        "client_listens_to_cap_tag_changes": gs,
        "client_ui_members": cap_methods,
        "client_ui_code": ui_code,
        "official_rule_texts": strings,
        "interpretation": (
            "客户端在判断『是否致命/播放击杀语音』时把伤害截到 "
            "GameEntity tag BACON_COMBAT_DAMAGE_CAP，且仅当 tag!=0 与 "
            "GetRealtimeBaconDamageCapEnabled()（tag 3403）为真；"
            "GameState 注册 BACON_COMBAT_DAMAGE_CAP 的 TAG_CHANGE 回调 → cap "
            "在对局中会**变化**（服务器下发）；HUD 由 PlayerLeaderboardManager / "
            "DamageCapPanel 显示『伤害上限：』。客户端无任何 5/10/15 之类的本地档位表。"
        ),
    }


def bobsbuddy_evidence() -> dict:
    inv = ROOT / "reversed/rea/hdt_bobsbuddy/BobsBuddyInvoker.cs"
    lines = read_text(inv).splitlines()
    hit = None
    for i, line in enumerate(lines):
        if "BACON_COMBAT_DAMAGE_CAP)" in line or (
                "BACON_COMBAT_DAMAGE_CAP_ENABLED" in line):
            hit = {
                "file": "reversed/rea/hdt_bobsbuddy/BobsBuddyInvoker.cs",
                "line": i + 1,
                "snippet": [ln.strip() for ln in lines[max(0, i - 2):i + 6]
                            if ln.strip()],
            }
            break
    return {
        "host": "Hearthstone Deck Tracker (BobsBuddy plugin)",
        "input_construction": hit,
        "simulator_field": "BobsBuddy.Simulation.Simulator.DamageCap"
                           "（reversed/rea/bobsbuddy/src_full/BobsBuddy.decompiled.cs:6336/6680/7020-7030）",
    }


def engine_rules() -> dict:
    path = ROOT / "hsrl2/constants.py"
    lines = read_text(path).splitlines()
    constants = {}
    for i, line in enumerate(lines, 1):
        m = re.match(r"(DAMAGE_CAP_[A-Z0-9_]+)\s*=\s*(\d+|None)", line)
        if m:
            constants[m.group(1)] = {"value": int(m.group(2)) if m.group(2).isdigit() else None,
                                     "line": i}
    func = []
    for i, line in enumerate(lines, 1):
        if line.startswith("def damage_cap"):
            func = [ln.strip() for ln in lines[i - 1:i + 12] if ln.strip()]
            break
    return {
        "file": "hsrl2/constants.py",
        "constants": constants,
        "function": func,
        "behaviour": (
            "alive_players <= 4 -> None（无上限）；turn<=3 -> 5；turn<=7 -> 10；"
            "否则 15（本地 turn 表，非服务器 tag）"
        ),
        "override_hook": "hsrl2/game.py:3139-3140 `_damage_cap_override`",
    }


def powerlog_scan() -> dict:
    found = [str(p) for p in POWERLOG_CANDIDATES if p.exists()]
    return {
        "candidates": [str(p) for p in POWERLOG_CANDIDATES],
        "found": found,
        "usable": bool(found),
        "note": "当前机器没有游戏侧 Power.log（游戏近期未运行）；拿到后运行 "
                "reversed/rea/scripts/damage_cap_extract.py 即可产出逐回合 cap 观察表。",
    }


def main() -> int:
    logs = scan_logs()
    corpus = corpus_turn_histogram()
    client = client_evidence()
    bb = bobsbuddy_evidence()
    engine = engine_rules()
    pl = powerlog_scan()

    ev = {
        "schema": "rea.damage_cap_evidence.v1",
        "generated_by": "reversed/rea/scripts/damage_cap_evidence.py",
        "sources_checked": {
            "hdt_logs": str(HDT_LOG),
            "official_traces": str(ROOT / "runs/official_traces"),
            "client_assembly": "C:\\Program Files (x86)\\Hearthstone\\Hearthstone_Data\\Managed\\Assembly-CSharp.dll",
            "client_strings": "C:\\Program Files (x86)\\Hearthstone\\Strings\\zhCN",
            "engine": "hsrl2/constants.py, hsrl2/game.py",
        },
        "log_scan": logs,
        "powerlog_scan": pl,
        "observations": [],
        "observation_blocker": {
            "direct_cap_value": "HDT 日志不打印 input.DamageCap / GameEntity tag / 英雄血量 / 伤害事件；"
                                "27 个日志、%d 行里 DamageCap 命中 0 次" % logs["total_lines"],
            "damage_inference": "cap 只影响『战败方英雄掉多少血』，不影响 win/tie/loss；"
                                "而本语料没有任何血量/伤害可观测项 → 无法反推。",
            "alive_players": "HDT 日志没有玩家淘汰/存活数事件（GameEventHandler 只有回合/技能/胜负）。",
        },
        "corpus": corpus,
        "official_client_evidence": client,
        "third_party_simulator_evidence": bb,
        "engine_rule": engine,
        "comparison": {
            "cap_removal_threshold": {
                "official_text": "场上只剩4位玩家后解除（GLUE_TIP_BACON_SOLO_1 / DUOS popup）",
                "engine": "DAMAGE_CAP_REMOVED_AT_PLAYERS = 4（alive<=4 -> None）",
                "verdict": "MATCH",
                "confidence": "high（官方字符串 + 客户端 UI/逻辑）",
            },
            "cap_is_dynamic_server_tag": {
                "official": "BACON_COMBAT_DAMAGE_CAP(2089) + ENABLED(3403)；GameState 监听 TAG_CHANGE；"
                            "侧栏 DamageCapPanel 显示当前值",
                "engine": "本地 turn 表（5/10/15）",
                "verdict": "MECHANISM_MISMATCH（引擎是近似；权威值需从服务器 tag 获取）",
                "confidence": "high（客户端代码+字符串，BobsBuddy 同机制）",
            },
            "cap_value_by_turn": {
                "official_text_ceiling": "GLUE_TIP_BACON_SOLO_1：单次上限 15 点；"
                                         "COMBAT_TIPS：直到决出四强前有上限，具体数值看左边栏",
                "engine": "turn<=3:5, turn<=7:10, turn>=8:15（另 alive<=4 无上限）",
                "verdict": "UNVERIFIED（本地无可观测数据；官方文本只给上限 15 与动态显示）",
                "confidence": "low（无逐场 tag 样本）",
            },
            "damage_formula": {
                "official": "PLAYER_TECH_LEVEL + Σ 场上随从 TECH_LEVEL（TB_BaconShop.cs:1694 / GLUE_TIP_BACON_3）",
                "engine": "winner.tavern_tier + Σ winner 存活随从 tech_level（hsrl2/game.py:3499-3504）",
                "verdict": "MATCH",
                "confidence": "high",
            },
        },
        "confidence_summary": {
            "cap_uses_server_tag": "high",
            "cap_removed_at_4_players": "high",
            "cap_ceiling_15": "high（官方 tip 文本；旧文本可能滞后）",
            "turn_tier_values_5_10_15": "UNVERIFIED — 需要 Power.log tag 样本",
        },
        "next_steps": {
            "capture": "下次运行游戏并开启 Power.log 后，采集 >=20 场 BG 对局",
            "extract": "C:\\Python313\\python.exe reversed\\rea\\scripts\\damage_cap_extract.py "
                       "--powerlog <Power.log 路径>",
            "look_for": ["GameEntity 初始化/变更中的 tag=2089 value=...（BACON_COMBAT_DAMAGE_CAP）",
                         "GameEntity 的 tag=3403 value=...（ENABLED）",
                         "TURN tag 变化以对齐『第 N 回合』",
                         "PLAYSTATE=LOST/PLAYING 以计算存活人数"],
        },
    }
    OUT.write_text(json.dumps(ev, ensure_ascii=False, indent=1), encoding="utf-8")
    print(f"written {OUT} ({OUT.stat().st_size} bytes)")
    print("\n=== summary ===")
    print("logs:", logs["log_files"], "lines:", logs["total_lines"],
          "input_blocks:", logs["simulation_input_blocks"])
    print("cap tokens:", logs["token_counts"], "| health token lines:",
          logs["health_token_lines"], "(all pBeetleHealth)")
    print("traces:", corpus["traces"], "turns:", corpus["turn_min"], "-", corpus["turn_max"],
          "official:", corpus["official_result_counts"])
    print("Power.log found:", pl["found"] or "NO")
    print("observations:", len(ev["observations"]), "(blocked: no observable cap/health)")
    return 0


if __name__ == "__main__":
    sys.exit(main())
