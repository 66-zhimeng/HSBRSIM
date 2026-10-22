"""只读：争议机制（Reborn / 剧毒 / 顺劈 / 死亡波次）的**间接**统计证据。

背景：HDT BobsBuddy 会话日志只有「战斗开始瞬间的战团快照 + 官方 win/loss/tie +
BobsBuddy 10k 次模拟概率」，**没有**逐事件伤害/死亡/属性变化 → Reborn 复生后属性、
毒杀是否要求攻击者存活、顺劈是否随攻击者死亡取消、死亡波次顺序都**无法直接观察**。

本脚本做可做的间接检验：在「无已知阻塞缺口」的轨迹子集上，比较官方结果分别与
（a）引擎预测、（b）BobsBuddy 预测的吻合率；并按机制关键字切分子集：
  * reborn_all / reborn_modified（复生随从数值 != 引擎 CardDef 基础值 → 有 buff）
  * venomous_any（venomous/poisonous）
  * cleave_any
  * deathrattle_cards（CardDef 有 deathrattle 标记的卡；不是"同波多亡语"的直接证据）

输出 JSON 到 stdout，供 `reversed/rea/combat_rule_evidence.md` 引用。

用法：C:\\Python313\\python.exe reversed\\rea\\scripts\\combat_rule_stats.py
"""
from __future__ import annotations

import json
import sys
from collections import Counter
from pathlib import Path

ROOT = Path(r"C:\HSagent")
sys.path.insert(0, str(ROOT))
sys.path.insert(0, str(ROOT / "tools"))

import diff_replay_combat as drc  # noqa: E402
from hsrl2.db import CardDB  # noqa: E402

REPORT = ROOT / "runs" / "diff_report.json"
BLOCKING_PREFIXES = ("no_script:", "detected_effect:", "dynamic_script_data",
                     "deathrattle_annotation", "hero_power_state",
                     "unknown_keyword:", "unsupported_keyword", "dynamic_stat_script")


def is_clean(gaps) -> bool:
    for gap in gaps or []:
        if gap.startswith(BLOCKING_PREFIXES):
            return False
        if gap in ("unsupported_keyword", "dynamic_stat_script"):
            return False
    return True


def minions(record, side):
    return record["trace"][side].get("minions") or []


def keywords(record):
    out = Counter()
    for side in ("player", "opponent"):
        for m in minions(record, side):
            for kw in m.get("keywords") or []:
                out[kw] += 1
    return out


def _metrics(records):
    n = eng_off = bb_off = both = conflict = conflict_eng = conflict_bb = det = 0
    for rec in records:
        ev = (rec.get("comparison") or {}).get("evidence") or {}
        official = ev.get("official_result")
        if official is None:
            continue
        eng_mode = ev.get("engine_mode")
        bb_mode = ev.get("bb_mode")
        n += 1
        if eng_mode == official:
            eng_off += 1
        if bb_mode == official:
            bb_off += 1
        if eng_mode == official and bb_mode == official:
            both += 1
        if eng_mode is not None and bb_mode is not None and eng_mode != bb_mode:
            conflict += 1
            if eng_mode == official:
                conflict_eng += 1
            if bb_mode == official:
                conflict_bb += 1
        bb_rates = (ev.get("bb_rates_normalized") or {})
        if bb_rates and max(bb_rates.values()) >= 0.999:
            det += 1
    return {
        "traces_with_official_result": n,
        "engine_matches_official": eng_off,
        "engine_match_rate": round(eng_off / n, 4) if n else None,
        "bb_matches_official": bb_off,
        "bb_match_rate": round(bb_off / n, 4) if n else None,
        "both_match": both,
        "engine_vs_bb_conflicts": conflict,
        "conflict_official_picks_engine": conflict_eng,
        "conflict_official_picks_bb": conflict_bb,
        "conflict_neither": max(0, conflict - conflict_eng - conflict_bb),
        "deterministic_bb_traces": det,
    }


def stats(records, db, resolver, label):
    clean = [r for r in records
             if is_clean((r.get("simulation") or {}).get("context_gaps"))]
    return {
        "subset": label,
        "all": _metrics(records),
        "clean": _metrics(clean),
    }


def main() -> int:
    db = CardDB.load(str(ROOT / "data"))
    resolver = drc.CardResolver(db, drc.OFFICIAL_DATA_255130)
    report = json.loads(REPORT.read_text(encoding="utf-8-sig"))
    records = report["records"]

    def engine_card(rec, minion):
        cid = ((rec.get("simulation") or {}).get("engine_card_ids") or {}).get(
            minion.get("name") or "")
        return db.get(cid) if cid else None

    def has_reborn(rec):
        return keywords(rec).get("reborn", 0) > 0

    def has_buffed_reborn(rec):
        for side in ("player", "opponent"):
            for m in minions(rec, side):
                if "reborn" not in (m.get("keywords") or []):
                    continue
                d = engine_card(rec, m)
                if d is None or d.atk is None or d.health is None:
                    continue
                if (m.get("atk"), m.get("health")) != (d.atk, d.health):
                    return True
        return False

    def has_kw(rec, names):
        kws = keywords(rec)
        return any(kws.get(n, 0) > 0 for n in names)

    def has_deathrattle_card(rec):
        # CardDef 无 deathrattle 布尔字段（实测 0 张），用卡面文本代理（488 张）
        for side in ("player", "opponent"):
            for m in minions(rec, side):
                d = engine_card(rec, m)
                text = (getattr(d, "text", "") or "") if d is not None else ""
                if "亡语" in text or "Deathrattle" in text:
                    return True
        return False

    subsets = {
        "baseline_all": records,
        "baseline_clean": [r for r in records
                           if is_clean((r.get("simulation") or {}).get("context_gaps"))],
        "reborn_all": [r for r in records if has_reborn(r)],
        "reborn_buffed": [r for r in records if has_buffed_reborn(r)],
        "venomous_or_poisonous": [r for r in records
                                  if has_kw(r, ("venomous", "poisonous"))],
        "cleave": [r for r in records if has_kw(r, ("cleave",))],
        "deathrattle_cards": [r for r in records if has_deathrattle_card(r)],
    }
    out = {
        "schema": "rea.combat_rule_indirect_stats.v1",
        "source_report": str(REPORT),
        "note": ("间接证据：日志无逐事件数据；这里只比较『官方结果 vs 引擎/BB 预测』。"
                 "clean = 无 no_script/detected_effect/dynamic_script_data/"
                 "deathrattle_annotation/hero_power_state/unknown_keyword/dynamic_stat_script。"),
        "subsets": [stats(recs, db, resolver, name) for name, recs in subsets.items()],
    }
    print(json.dumps(out, ensure_ascii=False, indent=1))
    return 0


if __name__ == "__main__":
    sys.exit(main())
