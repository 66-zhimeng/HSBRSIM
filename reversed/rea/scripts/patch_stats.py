p=r"reversed\rea\scripts\combat_rule_stats.py"
s=open(p,encoding="utf-8").read()
s=s.replace('''    def has_deathrattle_card(rec):
        for side in ("player", "opponent"):
            for m in minions(rec, side):
                d = engine_card(rec, m)
                if d is not None and getattr(d, "deathrattle", False):
                    return True
        return False''',
'''    def has_deathrattle_card(rec):
        # CardDef 无 deathrattle 布尔字段（实测 0 张），用卡面文本代理（488 张）
        for side in ("player", "opponent"):
            for m in minions(rec, side):
                d = engine_card(rec, m)
                text = (getattr(d, "text", "") or "") if d is not None else ""
                if "亡语" in text or "Deathrattle" in text:
                    return True
        return False''')
# 重写 stats：返回 all / clean 两组，并支持 size 过滤
old='''def stats(records, db, resolver, label):
    n = 0
    clean_n = 0
    eng_off = bb_off = both = 0
    conflict = 0
    conflict_eng = conflict_bb = 0
    det = 0
    for rec in records:
        ev = (rec.get("comparison") or {}).get("evidence") or {}
        official = ev.get("official_result")
        if official is None:
            continue
        eng_mode = ev.get("engine_mode")
        bb_mode = ev.get("bb_mode")
        n += 1
        if is_clean((rec.get("simulation") or {}).get("context_gaps")):
            clean_n += 1
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
        "subset": label,
        "traces_with_official_result": n,
        "clean_traces": clean_n,
        "engine_matches_official": eng_off,
        "engine_match_rate": round(eng_off / n, 4) if n else None,
        "bb_matches_official": bb_off,
        "bb_match_rate": round(bb_off / n, 4) if n else None,
        "both_match": both,
        "engine_vs_bb_conflicts": conflict,
        "conflict_official_picks_engine": conflict_eng,
        "conflict_official_picks_bb": conflict_bb,
        "deterministic_bb_traces": det,
    }'''
new='''def _metrics(records):
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
    }'''
assert old in s
s=s.replace(old,new)
open(p,"w",encoding="utf-8").write(s)
print("patched stats")
