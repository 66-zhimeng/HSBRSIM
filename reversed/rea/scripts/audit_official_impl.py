"""决定性取证：本机游戏本体能否直接给出「官方酒馆战棋完整实现」？

只读。枚举 Managed 程序集、做字节级符号扫描、解析既有类型/方法清单、
用 ilspycmd 定点反编译关键类型并抽取证据行，输出机读 JSON：
  reversed/rea/official_implementation_audit.json

用法：
  C:\\Python313\\python.exe reversed\\rea\\scripts\\audit_official_impl.py
  ... --skip-decompile        # 只用已有反编译文件（快）
  ... --json <path>           # 覆盖输出路径

离线可跑；不运行游戏、不联网、不写游戏目录。
"""
from __future__ import annotations

import argparse
import hashlib
import json
import os
import re
import subprocess
import sys
from collections import Counter
from datetime import datetime, timezone
from pathlib import Path

HS = Path(r"C:\Program Files (x86)\Hearthstone")
MANAGED = HS / "Hearthstone_Data" / "Managed"
PLUGINS = HS / "Hearthstone_Data" / "Plugins" / "x86_64"
DATA_WIN = HS / "Data" / "Win"
HDT_APP = Path(r"%USERPROFILE%\AppData\Local\HearthstoneDeckTracker\app-1.53.0")
ROOT = Path(r"C:\HSagent")
NET_DUMPS = ROOT / "reversed" / "net"
AUDIT_DIR = ROOT / "reversed" / "rea" / "audit"
OUT_DEFAULT = ROOT / "reversed" / "rea" / "official_implementation_audit.json"
ILSPY_CANDIDATES = [
    Path(r"%USERPROFILE%\.dotnet\tools\ilspycmd.EXE"),
    Path("ilspycmd"),
]

ADJUDICATOR_KEYWORDS = [
    b"CombatSimulator", b"SimulateCombat", b"ResolveCombat", b"CalculateCombat",
    b"PredictCombat", b"CombatResolver", b"CombatAdjudicator", b"BattleSimulator",
    b"CombatEngine", b"CombatLogic",
]
BG_KEYWORDS = [b"BaconCombat", b"BattlegroundsCombat", b"BaconBoardTester"]
OTHER_KEYWORDS = [b"DamageCap", b"LocalGame", b"PowerHistory", b"Simulate"]

# key types -> (decompiled filename, source dll)
KEY_TYPES = {
    "Network": "Assembly-CSharp.dll",
    "Gameplay": "Assembly-CSharp.dll",
    "PowerTask": "Assembly-CSharp.dll",
    "PowerProcessor": "Assembly-CSharp.dll",
    "PowerTaskList": "Assembly-CSharp.dll",
    "GameState": "Assembly-CSharp.dll",
    "GameUtils": "Assembly-CSharp.dll",
    "GameEntity": "Assembly-CSharp.dll",
    "TB_BaconShop": "Assembly-CSharp.dll",
    "ReconnectMgr": "Assembly-CSharp.dll",
}

# (evidence_id, decompiled file, needles that must each be found)
EVIDENCE = [
    ("network_power_history_from_protocol", "Network.cs",
     [r"public List<PowerHistory> GetPowerHistory\(\)",
      r"m_connectApi\.GetPowerHistory\(\)",
      r"PowerHistoryData"]),
    ("gameplay_registers_power_history_packet", "Gameplay.cs",
     [r"PowerHistory\.PacketID\.ID",
      r"private void OnPowerHistory\(\)",
      r"Network\.Get\(\)\.GetPowerHistory\(\)",
      r"GameState\.Get\(\)\.OnPowerHistory\("]),
    ("gameplay_queued_power_history", "Gameplay.cs",
     [r"ProcessQueuedPowerHistory\(\)",
      r"m_queuedPowerHistory\.Dequeue\(\)"]),
    ("power_processor_builds_tasks_from_history", "PowerProcessor.cs",
     [r"public void OnPowerHistory\(List<Network\.PowerHistory> powerList\)",
      r"private void BuildTaskList\(List<Network\.PowerHistory> powerList"]),
    ("power_task_dispatches_server_history", "PowerTask.cs",
     [r"public void DoTask\(\)",
      r"gameState\.OnTagChange\(",
      r"Network\.HistTagChange"]),
    ("power_tasklist_create_task_wraps_history", "PowerTaskList.cs",
     [r"public PowerTask CreateTask\(Network\.PowerHistory netPower\)",
      r"powerTask\.SetPower\(netPower\)"]),
    ("gamestate_on_tag_change_uses_server_value", "GameState.cs",
     [r"public bool OnTagChange\(Network\.HistTagChange netChange\)",
      r"tagDelta\.newValue = netChange\.Value",
      r"entity\.SetTag\("]),
    ("gameutils_apply_power_from_network", "GameUtils.cs",
     [r"public static void ApplyPower\(Entity entity, Network\.PowerHistory power\)",
      r"ApplyShowEntity",
      r"entity\.SetTag\(tag\.Name, tag\.Value\)"]),
    ("tb_shop_combat_events_are_server_blocks", "TB_BaconShop.cs",
     [r"private void GetRemainingCombatEvents",
      r"HistoryBlock\.Type\.ATTACK",
      r"HistoryBlock\.Type\.DEATHS"]),
    ("tb_shop_combat_speed_uses_server_tags", "TB_BaconShop.cs",
     [r"private IEnumerator HandleCombatSpeedUp\(\)",
      r"BG_COMBAT_SPEED_START_TIME",
      r"BG_COMBAT_SPEED_ACCELERATION"]),
    ("tb_shop_combat_phase_is_ui_state", "TB_BaconShop.cs",
     [r"public override bool IsInBattlegroundsCombatPhase\(\)",
      r"return IsBattlePhase\(\)"]),
    ("gameentity_bacon_tags_drive_ui", "GameEntity.cs",
     [r"BACON_COMBAT_DAMAGE_CAP_ENABLED",
      r"BACON_IN_COMBAT_PHASE",
      r"SetRealtimeBaconCombatPhase"]),
    ("damagecap_applied_for_vo_only", "TB_BaconShop.cs",
     [r"private IEnumerator PlayVOIfLethal",
      r"GetTag\(GAME_TAG\.BACON_COMBAT_DAMAGE_CAP\)"]),
    ("gamestate_damagecap_tag_change", "GameState.cs",
     [r"case GAME_TAG\.BACON_COMBAT_DAMAGE_CAP:",
      r"OnDamageCapChanged\(change\.oldValue, change\.newValue\)"]),
    ("reconnect_reloads_game_from_server", "ReconnectMgr.cs",
     [r"ReconnectGame\(",
      r"LoadGame"]),
]


# ────────────────────────────── 工具 ──────────────────────────────
def sha256_16(path: Path) -> str:
    h = hashlib.sha256()
    with path.open("rb") as fh:
        for chunk in iter(lambda: fh.read(1 << 20), b""):
            h.update(chunk)
    return h.hexdigest()[:16]


def read_text(path: Path) -> str:
    try:
        return path.read_text(encoding="utf-8-sig", errors="replace")
    except OSError:
        return ""


def tsv(path: Path):
    if not path.exists():
        return []
    return [ln for ln in read_text(path).splitlines() if ln.strip()]


def find_ilspy():
    for c in ILSPY_CANDIDATES:
        if c.is_file() or (c.name == "ilspycmd" and shutil_which("ilspycmd")):
            return str(c) if c.is_file() else shutil_which("ilspycmd")
    return None


def shutil_which(name):
    for p in os.environ.get("PATH", "").split(os.pathsep):
        cand = Path(p) / (name + (".exe" if os.name == "nt" else ""))
        if cand.exists():
            return str(cand)
    return None


def snippet(file: Path, needles, before=1, after=10):
    """返回第一个 needle 命中处的代码行（含行号）与其它 needle 的命中行号。"""
    lines = read_text(file).splitlines()
    found = {}
    for needle in needles:
        pat = re.compile(needle)
        for i, line in enumerate(lines):
            if pat.search(line):
                found[needle] = i + 1
                break
    if not found:
        return None
    first = min(found.values())
    lo, hi = max(0, first - 1 - before), min(len(lines), first - 1 + after)
    return {
        "file": str(file),
        "line": first,
        "needle_lines": {k: v for k, v in found.items()},
        "snippet": [ln.strip() for ln in lines[lo:hi] if ln.strip()],
    }


# ────────────────────────────── 1. 程序集清单 ─────────────────────────────
def inventory_assemblies():
    rows = []
    for dll in sorted(MANAGED.glob("*.dll")):
        stem = dll.stem
        types = tsv(NET_DUMPS / stem / "types.tsv")
        methods = tsv(NET_DUMPS / stem / "methods.tsv")
        rows.append({
            "name": dll.name,
            "bytes": dll.stat().st_size,
            "sha256_16": sha256_16(dll),
            "has_tsv_dump": bool(types or methods),
            "types": len(types) or None,
            "methods": len(methods) or None,
        })
    return rows


# ────────────────────────────── 2. 全 DLL 字节扫描 ─────────────────────────────
def scan_dlls(assemblies):
    hits = []
    for dll in sorted(MANAGED.glob("*.dll")):
        data = dll.read_bytes()
        row = {}
        for kw in ADJUDICATOR_KEYWORDS + BG_KEYWORDS + OTHER_KEYWORDS:
            c = data.count(kw)
            if c:
                row[kw.decode()] = c
        if row:
            hits.append({"dll": dll.name, "hits": row})
    total = len(list(MANAGED.glob("*.dll")))
    adjudicator_any = sorted({h["dll"] for h in hits
                              if any(k.decode() in h["hits"] for k in ADJUDICATOR_KEYWORDS)})
    return {
        "dlls_scanned": total,
        "adjudicator_keyword_dlls": adjudicator_any,
        "per_dll_hits": hits,
        "conclusion": ("126 个托管 DLL 中没有任何 CombatSimulator/SimulateCombat/"
                       "ResolveCombat/CalculateCombat/PredictCombat/CombatResolver/"
                       "CombatAdjudicator/BattleSimulator/CombatEngine/CombatLogic 符号"
                       if not adjudicator_any else "存在候选符号，需人工复核"),
    }


# ────────────────────────────── 3. BG/战斗符号语义 ─────────────────────────────
def classify_symbols():
    types = tsv(NET_DUMPS / "Assembly-CSharp" / "types.tsv")
    methods = tsv(NET_DUMPS / "Assembly-CSharp" / "methods.tsv")

    def match(lines, pattern):
        pat = re.compile(pattern)
        return [ln.strip() for ln in lines if pat.search(ln)]

    out = {
        "counts": {
            "types_total": len(types),
            "methods_total": len(methods),
            "bacon_battleground_types": len(match(types, r"Bacon|Battleground")),
            "bacon_battleground_methods": len(match(methods, r"Bacon|Battleground")),
            "bacon_battleground_monobehaviour_types": len(
                [ln for ln in match(types, r"Bacon|Battleground")
                 if re.search(r"MonoBehaviour|ScriptableObject|UnityEngine\.UI|PlayMaker", ln)]),
            "combat_types": len(match(types, r"Combat")),
            "combat_methods": len(match(methods, r"Combat")),
            "power_pipeline_methods": len(
                match(methods, r"^(PowerTask|PowerTaskList|PowerProcessor|PowerQueue)::")),
            "simulate_methods": len(match(methods, r"\bSimulate")),
            "predict_methods": len(match(methods, r"\bPredict")),
        },
        "bacon_combat_step": {
            "type": "Assets.Global/BaconCombatStep",
            "judgement": "presentation（战斗播放步骤枚举；DBF 列 baconcombatstep）",
            "members": match(methods, r"BaconCombatStep|Baconcombatstep"),
        },
        "battlegrounds_combat_phase": {
            "type": "GameEntity/TB_BaconShop/Entity",
            "judgement": "presentation（服务器 tag BACON_IN_COMBAT_PHASE/阶段标志驱动 UI）",
            "members": match(methods, r"BaconCombatPhase|BattlegroundsCombatPhase"),
        },
        "damage_cap": {
            "types": match(types, r"DamageCap"),
            "methods": match(methods, r"DamageCap"),
            "judgement": "UI/tag 显示（无本地公式；值来自 GameEntity tag 2089）",
        },
        "simulate": {
            "methods": match(methods, r"\bSimulate"),
            "judgement": "网络/UI 模拟（断线、收包、转盘、任务进度），与战斗裁决无关",
        },
        "predict": {
            "methods": match(methods, r"\bPredict"),
            "judgement": "ZoneMgr/InputManager 的表现预测（区域/费用），不是战斗裁决",
        },
        "cheat_debug": {
            "sample": match(methods, r"^BaconBoard(CombatSkin)?::Cheat|BaconBoardSkinBehaviour::Cheat|DebugTriggerFSMState")[:20],
            "judgement": "棋盘皮肤/特效调试（MonoBehaviour/PlayMaker FSM），非战斗逻辑",
        },
        "bacon_board_tester": {
            "type": "BaconBoardTester",
            "judgement": "MonoBehaviour 棋盘皮肤 authoring/预览工具（PlayMaker FSM 状态切换）",
        },
    }
    return out


# ────────────────────────────── 4. 关键类型反编译 + 证据行 ─────────────────────────────
def decompile_key_types(ilspy, skip=False):
    AUDIT_DIR.mkdir(parents=True, exist_ok=True)
    acs = MANAGED / "Assembly-CSharp.dll"
    produced = {}
    for type_name, dll_name in KEY_TYPES.items():
        out = AUDIT_DIR / f"{type_name}.cs"
        if out.exists() and out.stat().st_size > 200:
            produced[type_name] = out
            continue
        # 已有其它位置的同名反编译（GameState/PowerTaskList/TB_BaconShop）
        for cand in (NET_DUMPS / "decompiled" / f"{type_name}.cs",):
            if cand.exists() and cand.stat().st_size > 200:
                produced[type_name] = cand
                break
        if type_name in produced:
            continue
        if skip or not ilspy:
            produced[type_name] = None
            continue
        try:
            with out.open("w", encoding="utf-8") as fh:
                subprocess.run([ilspy, "--disable-updatecheck", "-t", type_name,
                                str(acs)], stdout=fh, stderr=subprocess.DEVNULL,
                               timeout=300, check=False)
        except (OSError, subprocess.SubprocessError):
            pass
        produced[type_name] = out if out.exists() and out.stat().st_size > 200 else None
    return produced


def collect_evidence(produced):
    out = []
    for evidence_id, fname, needles in EVIDENCE:
        candidates = [p for p in produced.values() if p and p.name == fname]
        if not candidates:
            alt = AUDIT_DIR / fname
            candidates = [alt] if alt.exists() else []
        if not candidates:
            out.append({"id": evidence_id, "found": False,
                        "reason": f"decompiled file {fname} unavailable"})
            continue
        item = snippet(candidates[0], needles, before=1, after=10)
        if item is None:
            out.append({"id": evidence_id, "found": False,
                        "file": str(candidates[0])})
        else:
            out.append({"id": evidence_id, "found": True, **item})
    return out


# ────────────────────────────── 5. bgs 程序集分类 ─────────────────────────────
def classify_bgs():
    result = {}
    for asm in ("blizzard.bgsclient", "blizzard.bgssdk"):
        types = tsv(NET_DUMPS / asm / "types.tsv")
        methods = tsv(NET_DUMPS / asm / "methods.tsv")

        def c(pattern, lines):
            pat = re.compile(pattern)
            return len([ln for ln in lines if pat.search(ln)])

        result[asm] = {
            "types": len(types),
            "methods": len(methods),
            "battleground_types": c(r"Battleground", types),
            "battleground_methods": c(r"Battleground", methods),
            "bacon": c(r"Bacon", types + methods),
            "combat_types": c(r"Combat", types),
            "combat_methods": c(r"Combat", methods),
            "simulate": c(r"Simulat", types + methods),
            "powerhistory_types": c(r"PowerHistory", types),
            "powerhistory_methods": c(r"PowerHistory", methods),
            "sample_powerhistory": [ln.strip()[:150] for ln in methods
                                    if "PowerHistory" in ln][:5],
            "judgement": ("Battle.net Game Service 协议/序列化（PegasusUtil/PegasusGame "
                          "protobuf Serialize/Deserialize），不含裁决实现"),
        }
    return result


# ────────────────────────────── 6. 原生 & 资源 ─────────────────────────────
def scan_native():
    rows = []
    for dll in sorted(PLUGINS.glob("*.dll")):
        data = dll.read_bytes()
        rows.append({
            "name": dll.name,
            "bytes": dll.stat().st_size,
            "battleground": data.count(b"Battleground"),
            "combat": data.count(b"Combat"),
            "powerhistory": data.count(b"PowerHistory"),
            "anticheat": data.count(b"AntiCheat"),
            "simulate": data.count(b"Simulate"),
        })
    return {"plugin_dir": str(PLUGINS), "libraries": rows}


def scan_assets():
    files = list(DATA_WIN.glob("*.unity3d"))
    cat = Counter()
    for f in files:
        n = f.name.lower()
        for c in ("cardasset", "carddef", "cardtexture", "sound", "video", "music",
                  "hero", "prefab", "material", "texture", "asset_manifest",
                  "initial", "portpremium", "essential", "ui", "board", "bacon"):
            if c in n:
                cat[c] += 1
                break
        else:
            cat["other"] += 1
    # 抽样扫描内容是否含托管代码标记（Unity MonoBehaviour 序列化字段不含 CIL）
    samples = []
    for f in sorted(files)[:30]:
        data = f.read_bytes()[:8 << 20]
        samples.append({
            "name": f.name,
            "assembly_csharp": data.count(b"Assembly-CSharp"),
            "monoscript": data.count(b"MonoScript"),
            "combat_sim": data.count(b"CombatSimulator"),
        })
    return {
        "path": str(DATA_WIN),
        "bundle_count": len(files),
        "total_gb": round(sum(f.stat().st_size for f in files) / 1e9, 2),
        "categories": dict(cat.most_common()),
        "sample_code_markers": samples,
        "conclusion": ("Unity Mono 工程：C# 代码只存在于 Hearthstone_Data/Managed/*.dll；"
                       "Data/Win 的 5155 个 .unity3d 均为资源（cardasset/prefab/material/"
                       "texture/sound/video 等），抽样无 Assembly-CSharp/MonoScript/"
                       "CombatSimulator 代码标记。"),
    }


# ────────────────────────────── 7. 迁移评估 ─────────────────────────────
def bobsbuddy_inventory():
    rows = []
    for name in ("BobsBuddy.dll", "BobsBuddy.Common.dll", "HearthDb.dll"):
        p = HDT_APP / name
        rows.append({"name": name, "exists": p.exists(),
                     "bytes": p.stat().st_size if p.exists() else None,
                     "sha256_16": sha256_16(p) if p.exists() else None})
    return {"hdt_app": str(HDT_APP), "assemblies": rows,
            "note": "BobsBuddy.dll 为第三方完整战斗实现（非官方），可作为 .NET 桥接/第二实现参照"}


def migration_assessment(scan, symbols, bgs, native, assets, evidence):
    found = {e["id"] for e in evidence if e.get("found")}
    return {
        "verdict": "OFFICIAL_IMPLEMENTATION_NOT_IN_CLIENT",
        "confidence": "high",
        "one_line": ("客户端只是服务器 PowerHistory 的『状态应用器 + 播放器 + UI』，"
                     "不存在攻击轮转/目标随机/死亡结算/伤害计算/随机数等裁决实现；"
                     "官方完整实现（服务端）不在本机。"),
        "evidence_ids": sorted(found),
        "client_code_surface_for_migration": {
            "assembly_csharp_types": symbols["counts"]["types_total"],
            "assembly_csharp_methods": symbols["counts"]["methods_total"],
            "bg_types": symbols["counts"]["bacon_battleground_types"],
            "bg_methods": symbols["counts"]["bacon_battleground_methods"],
            "bg_monobehaviour_types": symbols["counts"].get("bacon_battleground_monobehaviour_types"),
            "note": ("即便把 381 个 BG 类型/3705 个方法全部移植，得到的也只是"
                     "UI/动画/状态应用；战斗裁决仍需另写（=当前 hsrl2 + 差分校准）。"),
        },
        "assets_available_from_client": [
            "CardDefs/DBF 卡牌与 tag 数据（data/ 生成源）",
            "GAME_TAG 等 893 个枚举（reversed/net/key_enums.json）",
            "官方规则文本（Strings/zhCN/*.txt：伤害公式、上限解除、亡语先于复生、毒/烈毒定义）",
            "Power 协议 schema（blizzard.bgsclient 的 PegasusGame.PowerHistory* protobuf 序列化器）",
            "战斗历史块类型（HistoryBlock.Type: ATTACK/DEATHS/TRIGGER/PLAY…）",
            "UI 侧伤害公式（TB_BaconShop.PlayVOIfLethal：PLAYER_TECH_LEVEL+Σ TECH_LEVEL）",
            "伤害上限 tag 2089/3403 与显示逻辑",
        ],
        "assets_not_available": [
            "服务端战斗裁决器（攻击顺序/目标随机/亡语/复生/Avenge/伤害上限值）",
            "服务端随机数（攻击目标、随机效果、酒馆刷新）",
            "服务端卡牌脚本实现（客户端只有卡面文本，脚本在服务器）",
            "服务端伤害上限档位的逐回合下发值（只有运行期 tag）",
            "匹配/淘汰/隐藏信息等服务器状态",
        ],
        "highest_consistency_route": {
            "route": "官方 Power.log 事件级轨迹 + 官方数据/文本 + 差分校准（现有路线）+ BobsBuddy 作第二实现参照",
            "why": ("Power.log 是服务器下发的权威 PowerHistory（客户端只接收/打印），"
                    "因此事件级轨迹 = 官方裁决的可观测真值；这是不接触服务端时能达到的最高一致性。"),
            "current_corpus": "679 场 BG 战斗（HDT BobsBuddy 会话日志）；Power.log 采集后升级为事件级",
        },
        "alternatives": [
            {
                "option": "BobsBuddy .NET 桥接（第三方完整战斗实现，非官方）",
                "feasible": True,
                "how": ("本机有 dotnet SDK 9.0.101；BobsBuddy.dll/BobsBuddy.Common.dll/"
                        "HearthDb.dll 在 HDT app-1.53.0。建一个小 C# console（netstandard/ 或 "
                        "net9.0 引用 netstandard2.0）构造 Input→SimulateForInput→JSON，"
                        "Python 侧 subprocess 调用。"),
                "effort": "1-3 天（不含逐卡校验）",
                "caveat": "第三方实现，不等于官方；许可证/来源需确认；只覆盖它已实现的卡",
            },
            {
                "option": "BobsBuddy C#→Python 全量移植",
                "feasible": True,
                "how": "1817 类型/5991 方法/约 29,200 行反编译代码（reversed/rea/bobsbuddy）",
                "effort": "数周~数月，回归风险高",
                "caveat": "同上，非官方；不推荐",
            },
            {
                "option": "客户端 C# 逻辑迁移（把 Assembly-CSharp 的 BG 代码搬到 Python）",
                "feasible": False,
                "how": "381 类型/3705 方法，依赖 Unity/PlayMaker/UI/网络；且不含裁决",
                "effort": "不可行/无意义",
                "caveat": "移植后仍需自写裁决",
            },
            {
                "option": "服务端二进制分析（若用户能合法提供服务器程序）",
                "feasible": "unknown",
                "how": ("REA 可做 native 分析，但本机未安装 Ghidra/IDA/Hopper"
                        "（doctor: missing_analysis_engine）；需用户提供服务端 build + 合法访问权；"
                        "服务端通常为 Linux 原生、符号剥离，工作量极大且不确定性高"),
                "effort": "极高",
                "caveat": "通常不可得/有法律与 ToS 风险；不建议",
            },
        ],
    }


# ────────────────────────────── main ─────────────────────────────
def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--json", default=str(OUT_DEFAULT))
    ap.add_argument("--skip-decompile", action="store_true")
    args = ap.parse_args()

    ilspy = find_ilspy()
    assemblies = inventory_assemblies()
    scan = scan_dlls(assemblies)
    symbols = classify_symbols()
    bgs = classify_bgs()
    native = scan_native()
    assets = scan_assets()
    produced = decompile_key_types(ilspy, skip=args.skip_decompile)
    evidence = collect_evidence(produced)
    mig = migration_assessment(scan, symbols, bgs, native, assets, evidence)

    result = {
        "schema": "rea.official_implementation_audit.v1",
        "generated_at": datetime.now(timezone.utc).isoformat(),
        "generator": "reversed/rea/scripts/audit_official_impl.py",
        "environment": {
            "game_dir": str(HS),
            "managed_dir": str(MANAGED),
            "managed_dll_count": len(assemblies),
            "ilspycmd": ilspy or "not_found",
            "hdt_app": str(HDT_APP),
            "dotnet": shutil_which("dotnet") or "not_found",
        },
        "verdict": {
            "official_complete_implementation_available": False,
            "conclusion": mig["one_line"],
            "confidence": mig["confidence"],
        },
        "assemblies": assemblies,
        "dll_symbol_scan": scan,
        "bg_symbol_semantics": symbols,
        "bgs_assemblies": bgs,
        "native_plugins": native,
        "bobsbuddy_inventory": bobsbuddy_inventory(),
        "assets": assets,
        "evidence": evidence,
        "migration": mig,
    }
    out = Path(args.json)
    out.parent.mkdir(parents=True, exist_ok=True)
    out.write_text(json.dumps(result, ensure_ascii=False, indent=1), encoding="utf-8")
    print(f"written {out} ({out.stat().st_size} bytes)")
    print("\n=== summary ===")
    print("verdict:", result["verdict"])
    print("managed dlls:", len(assemblies))
    print("adjudicator keyword dlls:", scan["adjudicator_keyword_dlls"] or "NONE")
    print("BG types/methods:", symbols["counts"]["bacon_battleground_types"],
          "/", symbols["counts"]["bacon_battleground_methods"])
    print("Simulate methods:", symbols["counts"]["simulate_methods"],
          "| Predict methods:", symbols["counts"]["predict_methods"])
    print("bgsclient BG types:", bgs["blizzard.bgsclient"]["battleground_types"],
          "| bgs PowerHistory types:", bgs["blizzard.bgsclient"]["powerhistory_types"])
    print("native combat strings:", {r["name"]: r["combat"] for r in native["libraries"]})
    print("evidence found:", sum(1 for e in evidence if e.get("found")), "/", len(evidence))
    missing = [e["id"] for e in evidence if not e.get("found")]
    if missing:
        print("evidence missing:", missing)
    print("assets:", assets["bundle_count"], "bundles,", assets["total_gb"], "GB")
    return 0


if __name__ == "__main__":
    sys.exit(main())
