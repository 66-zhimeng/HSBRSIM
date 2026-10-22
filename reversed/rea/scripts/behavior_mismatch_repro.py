"""只读诊断：6 条 behavior_mismatch 的根因实验（不修改 hsrl2 / 不改 harness）。

复用 diff-harness 的轨迹加载器（tools/diff_replay_combat.py）与引擎 API；
所有变体只在本进程内通过"补上缺失上下文"来验证官方结果的可达性。

用法：
  C:\\Python313\\python.exe reversed\\rea\\scripts\\behavior_mismatch_repro.py
"""
import sys
from collections import Counter

ROOT = r"C:\HSagent"
sys.path.insert(0, ROOT)
sys.path.insert(0, ROOT + r"\tools")

import diff_replay_combat as drc  # noqa: E402  (复用轨迹加载/卡名解析)
from hsrl2.db import CardDB  # noqa: E402
from hsrl2.game import Game  # noqa: E402
from hsrl2.hero import Hero  # noqa: E402
from hsrl2.minion import Minion  # noqa: E402
from hsrl2.tags import GameTag  # noqa: E402

TRACE_DIR = ROOT + r"\runs\official_traces"
CASES = {
    "D1": ("4c3c73caa189365e", "Prestidigitation(PackTactics) vs CloningGallery"),
    "D2": ("aa4c75414541d87b", "Prestidigitation(PackTactics) vs Wingmen"),
    "D3": ("b2683903c4464b6f", "GlaiveRicochet vs RelicsOfTheDeep + Crab deathrattle"),
    "D4": ("f5d1c647b2f449d1", "Tentacular(Ozumat) vs ForTheHorde"),
    "D5": ("9e80beef2ada485a", "RapidReanimation(Teron) vs ChooseYourChampion"),
    "D6": ("11285dbbe007f132", "Teron + dynamic stats"),
}

db = CardDB.load(ROOT + r"\data")
resolver = drc.CardResolver(db, drc.OFFICIAL_DATA_255130)
hero_index = drc.HeroIndex(db)

TAG = drc.ENGINE_KEYWORD_TAGS


def build_side(game, hero, warband, extra=None):
    """按轨迹精确写入数值/关键词；extra=[(name, atk, hp, kws)] 追加。"""
    placed = []
    for tm in warband.minions:
        cid = resolver.resolve(tm.name) if tm.name else tm.card_id
        m = game.create_minion(cid, controller=hero, golden=bool(tm.golden))
        if tm.atk is not None:
            m.set(GameTag.BASE_ATK, tm.atk)
            m.set(GameTag.ATK, tm.atk)
        if tm.health is not None:
            m.set(GameTag.BASE_HEALTH, tm.health)
            m.set(GameTag.HEALTH, tm.health)
        for kw in tm.keywords:
            if kw in TAG:
                m.set(TAG[kw], True)
        game.summon(hero, m)
        placed.append(m)
    for name, atk, hp, kws in (extra or []):
        cid = resolver.resolve(name)
        m = game.create_minion(cid, controller=hero)
        m.set(GameTag.BASE_ATK, atk)
        m.set(GameTag.ATK, atk)
        m.set(GameTag.BASE_HEALTH, hp)
        m.set(GameTag.HEALTH, hp)
        for t in kws:
            m.set(t, True)
        game.summon(hero, m)
        placed.append(m)
    return placed


def run_trace(trace, seeds=16, hero_powers="on_bind", hook=None,
              player_extra=None, opp_extra=None, teron_target=False):
    """复现一条轨迹；hook(game, hero_a, hero_b) 用于注入缺失上下文。"""
    out = Counter()
    boards_at_soc = []
    for seed in range(seeds):
        hero_a_id, _ = hero_index.resolve(trace.player.hero_power_id)
        hero_b_id, _ = hero_index.resolve(trace.opponent.hero_power_id)
        a, b = Hero(hero_a_id, "P"), Hero(hero_b_id, "O")
        g = Game([a, b], db, seed=seed)
        g.turn = int(trace.turn or 1)
        if hero_powers == "on_bind":
            for h, pid in ((a, trace.player.hero_power_id),
                           (b, trace.opponent.hero_power_id)):
                if pid:
                    try:
                        g.wire_hero_power_on_bind(h, pid)
                    except Exception as exc:
                        print("  wire error", pid, exc)
        pa = build_side(g, a, trace.player, player_extra)
        ob = build_side(g, b, trace.opponent, opp_extra)
        if teron_target:
            # Teron：日志只给 hero_power_data=game_id，无法自带板内映射；
            # 本例玩家只有 1 只随从 → 目标唯一，手动接线验证"输入正确时引擎能否复现"
            if setattr and trace.player.hero_power_id == "BG25_HERO_103p" and pa:
                a._teron_uuid = pa[0].uuid
            if trace.opponent.hero_power_id == "BG25_HERO_103p" and ob:
                b._teron_uuid = ob[0].uuid
        if hook:
            hook(g, a, b, pa, ob)
        # 记录 SoC 之后（战斗开始瞬间）的棋盘
        orig_run = g.run_combat

        def run_and_snap(ha, hb, _g=g, _a=a, _b=b, _orig=orig_run):
            # run() 内部先 reset+SoC；这里通过监听 start_of_combat 之后不易取，
            # 改为在首次 before_attack 时快照（SoC 已结束）
            done = {"v": False}

            def snap(_g=None, **_kw):
                if not done["v"]:
                    done["v"] = True
                    boards_at_soc.append(
                        ([m.card_id for m in _a.board if not m.dead],
                         [m.card_id for m in _b.board if not m.dead]))
            _g.events.register(drc.Listener(event="before_attack", owner=None,
                                            callback=snap))
            return _orig(ha, hb)

        r = run_and_snap(a, b)
        out["win" if r.winner is a else "loss" if r.winner is b else "tie"] += 1
    return dict(out), boards_at_soc


def load(tid):
    return drc.T.load_trace(f"{TRACE_DIR}\\{tid}.json")


def fmt(d):
    return {k: round(v, 3) for k, v in d.items()}


def pack_tactics_hook(g, a, b, pa, ob):
    """BobsBuddy CheckPackTactics: 友方随从被攻击时召唤其 3/3 精确副本（一次）。"""
    used = {"v": False}

    def on_attack(_g, attacker=None, defender=None, **_kw):
        if used["v"] or defender is None or defender.controller is not a:
            return
        used["v"] = True
        src_def = db.get(defender.card_id)
        copy = g.create_minion(defender.card_id, controller=a,
                               golden=defender.is_golden)
        copy.set(GameTag.BASE_ATK, 3)
        copy.set(GameTag.ATK, 3)
        copy.set(GameTag.BASE_HEALTH, 3)
        copy.set(GameTag.HEALTH, 3)
        for tag in (GameTag.TAUNT, GameTag.DIVINE_SHIELD):
            if defender.has(tag):
                copy.set(tag, True)
        g.summon(a, copy)

    g.events.register(drc.Listener(event="before_attack", owner=None,
                                   callback=on_attack))


def crab_deathrattle_hook(g, a, b, pa, ob):
    """Risen Rider 获得外部亡语 'Summon a 3/2 Crab'（BG27_004t Crab Mount 效果）。"""
    used = {"v": False}

    def on_death(_g, minion=None, **_kw):
        if used["v"] or minion is None:
            return
        if minion.card_id != "BG25_001" or minion.controller is not b:
            return
        used["v"] = True
        crab = g.create_minion("BG27_004t2", controller=b)
        g.summon(b, crab)

    g.events.register(drc.Listener(event="death", owner=None, callback=on_death))


print("=" * 78)
print("PART 1 — 原样复现（hero powers on/off），对照报告")
print("=" * 78)
for key, (tid, desc) in CASES.items():
    tr = load(tid)
    on, _ = run_trace(tr, seeds=16, hero_powers="on_bind")
    off, _ = run_trace(tr, seeds=16, hero_powers="none")
    print(f"{key} {desc}")
    print(f"   official={tr.official_result}  bb={fmt(tr.bb_rates or {})}")
    print(f"   engine(power=on )={fmt(on)}   engine(power=none)={fmt(off)}")

print()
print("=" * 78)
print("PART 2 — 补上缺失上下文后，官方结果是否可达")
print("=" * 78)
# D1/D2: PackTactics 秘密
for key in ("D1", "D2"):
    tid, _ = CASES[key]
    tr = load(tid)
    r, _ = run_trace(tr, seeds=16, hook=pack_tactics_hook)
    print(f"{key} + PackTactics 3/3 copy -> {fmt(r)}  (official {tr.official_result}, bb {fmt(tr.bb_rates or {})})")

# D3: Crab 亡语
tid, _ = CASES["D3"]
tr = load(tid)
r, _ = run_trace(tr, seeds=16, hook=crab_deathrattle_hook)
print(f"D3 + Crab deathrattle(3/2) -> {fmt(r)}  (official {tr.official_result}, bb {fmt(tr.bb_rates or {})})")

# D4: Ozumat 正确行为 = 只有 1 只 Tentacle（BobsBuddy _tokenSummoned）
tid, _ = CASES["D4"]
tr = load(tid)
r_as_is, soc = run_trace(tr, seeds=16)
print(f"D4 as-is(power on) -> {fmt(r_as_is)}  (official {tr.official_result}, bb {fmt(tr.bb_rates or {})})")
if soc:
    print(f"   SoC 后棋盘快照(首 seed): player={soc[0][0]}  opponent={soc[0][1]}")
r_one, _ = run_trace(tr, seeds=16, hero_powers="none",
                     player_extra=[("Ozumat's Tentacle", 2, 2, [GameTag.TAUNT])])
print(f"D4 修正为'开局 1 只 2/2 Taunt Tentacle' -> {fmt(r_one)}")

# D5: Teron 目标接线
tid, _ = CASES["D5"]
tr = load(tid)
r_as_is, _ = run_trace(tr, seeds=16)
r_teron, _ = run_trace(tr, seeds=16, teron_target=True)
print(f"D5 as-is(power on, 无 target 接线) -> {fmt(r_as_is)}")
print(f"D5 + Teron target 接线(_teron_uuid) -> {fmt(r_teron)}  (official {tr.official_result}, bb {fmt(tr.bb_rates or {})})")

print()
print("=" * 78)
print("PART 3 — Lead 怀疑的 4 类战斗核心机制（纯引擎、无英雄技能）")
print("=" * 78)


def micro(player, opponent, seeds=16):
    out = Counter()
    for seed in range(seeds):
        a, b = Hero("BG20_HERO_100", "P"), Hero("BG20_HERO_101", "O")
        g = Game([a, b], db, seed=seed)
        for hero, specs in ((a, player), (b, opponent)):
            for cid, atk, hp, kws in specs:
                m = g.create_minion(cid, controller=hero)
                m.set(GameTag.BASE_ATK, atk)
                m.set(GameTag.ATK, atk)
                m.set(GameTag.BASE_HEALTH, hp)
                m.set(GameTag.HEALTH, hp)
                for t in kws:
                    m.set(t, True)
                g.summon(hero, m)
        r = g.run_combat(a, b)
        out["win" if r.winner is a else "loss" if r.winner is b else "tie"] += 1
    return dict(out)


print("M1 圣盾吸收+风怒第二击: Annoy 1/2 Taunt DS vs Cyclone 2/1 DS WF ->",
      micro([("BG_GVG_085", 1, 2, [GameTag.TAUNT, GameTag.DIVINE_SHIELD])],
            [("BGS_119", 2, 1, [GameTag.DIVINE_SHIELD, GameTag.WINDFURY])]))
print("M2 复生触发时机: Risen 2/1 Taunt Reborn vs plain 3/3 ->",
      micro([("BG25_001", 2, 1, [GameTag.TAUNT, GameTag.REBORN])],
            [("BG35_814", 3, 3, [])]))
print("M3 同时双向伤害双亡: 2/2 vs 2/2 ->",
      micro([("BG_GVG_085", 2, 2, [])], [("BG_GVG_085", 2, 2, [])]))
print("M4 风怒第二击重选目标: Cyclone 2/1 DS WF vs 两只 1/1 ->",
      micro([("BGS_119", 2, 1, [GameTag.DIVINE_SHIELD, GameTag.WINDFURY])],
            [("BG_GVG_085", 1, 1, []), ("BG_GVG_085", 1, 1, [])]))
print("M5 复生单位被再次攻击: Annoy 1/2 Taunt DS vs Risen 2/1 Reborn ->",
      micro([("BG_GVG_085", 1, 2, [GameTag.TAUNT, GameTag.DIVINE_SHIELD])],
            [("BG25_001", 2, 1, [GameTag.TAUNT, GameTag.REBORN])]))

