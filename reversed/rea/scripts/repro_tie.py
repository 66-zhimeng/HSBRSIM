"""只读诊断：复现 diff-harness 的 3 个 behavior_mismatch 最小用例，并打印引擎战斗轨迹。

不修改 hsrl2；只在运行时 monkey-patch 观测（本文件写 reversed/rea/scripts）。
用法：C:\\Python313\\python.exe reversed\\rea\\scripts\\repro_tie.py
"""
import sys

sys.path.insert(0, r"C:\HSagent")

from collections import Counter

import hsrl2.combat as CB
import hsrl2.game as GM
from hsrl2.db import CardDB
from hsrl2.game import Game
from hsrl2.hero import Hero
from hsrl2.minion import Minion
from hsrl2.tags import GameTag

db = CardDB.load(r"C:\HSagent\data")

TRACE = []
DETAIL = {"on": False}


def _fmt(m):
    if m is None:
        return "None"
    return f"{m.card_id}({m.atk}/{m.health} hp={m.health} DS={m.divine_shield} WF={m.windfury} RB={m.reborn} dead={m.dead})"


def instrument():
    orig_attack = CB.CombatScheduler._execute_attack
    orig_choose = CB.CombatScheduler._choose_target
    orig_dmg = Minion.take_damage
    orig_death = GM.Game._process_single_death
    orig_first = CB.CombatScheduler._determine_first

    def choose(self, side):
        t = orig_choose(self, side)
        if DETAIL["on"]:
            TRACE.append(f"    choose_target(side={side.name}) -> {_fmt(t)}")
        return t

    def execute(self, attacker, defender):
        if DETAIL["on"]:
            TRACE.append(
                f"  ATTACK {_fmt(attacker)} -> {_fmt(defender)} | "
                f"atk_power={attacker.atk} def_power={defender.atk}")
        orig_attack(self, attacker, defender)
        if DETAIL["on"]:
            TRACE.append(
                f"  AFTER  {_fmt(attacker)} | {_fmt(defender)} | "
                f"boards={[[x.card_id for x in h.board if not x.dead] for h in (self.a, self.b)]}")

    def take_damage(self, amount, source=None):
        before = self.health
        r = orig_dmg(self, amount, source=source)
        if DETAIL["on"]:
            TRACE.append(
                f"      take_damage {self.card_id} amt={amount} "
                f"hp {before}->{self.health} actual={r} dead={self.dead}")
        return r

    def death(self, m):
        if DETAIL["on"]:
            TRACE.append(f"      DEATH {_fmt(m)}")
        return orig_death(self, m)

    def first(self):
        r = orig_first(self)
        TRACE.append(f"[determine_first] -> {r[0].name} first (sizes {len(self.a.living_minions())}/{len(self.b.living_minions())})")
        return r

    CB.CombatScheduler._execute_attack = execute
    CB.CombatScheduler._choose_target = choose
    CB.CombatScheduler._determine_first = first
    Minion.take_damage = take_damage
    GM.Game._process_single_death = death


def build(p, o):
    a, b = Hero("BG20_HERO_100", "P"), Hero("BG20_HERO_101", "O")
    g = Game([a, b], db, seed=0)
    for hero, specs in ((a, p), (b, o)):
        for cid, atk, hp, kws in specs:
            m = g.create_minion(cid, controller=hero)
            m.set(GameTag.BASE_ATK, atk)
            m.set(GameTag.ATK, atk)
            m.set(GameTag.BASE_HEALTH, hp)
            m.set(GameTag.HEALTH, hp)
            for t in kws:
                m.set(t, True)
            g.summon(hero, m)
    return g, a, b


def run(p, o, seeds=16, detail_first=True):
    out = Counter()
    for seed in range(seeds):
        a, b = Hero("BG20_HERO_100", "P"), Hero("BG20_HERO_101", "O")
        g = Game([a, b], db, seed=seed)
        for hero, specs in ((a, p), (b, o)):
            for cid, atk, hp, kws in specs:
                m = g.create_minion(cid, controller=hero)
                m.set(GameTag.BASE_ATK, atk)
                m.set(GameTag.ATK, atk)
                m.set(GameTag.BASE_HEALTH, hp)
                m.set(GameTag.HEALTH, hp)
                for t in kws:
                    m.set(t, True)
                g.summon(hero, m)
        TRACE.clear()
        DETAIL["on"] = detail_first and seed == 0
        r = g.run_combat(a, b)
        DETAIL["on"] = False
        out["win" if r.winner is a else "loss" if r.winner is b else "tie"] += 1
        if seed == 0:
            print("\n--- TRACE seed=0 ---")
            for line in TRACE:
                print(line)
            print(f"--- result={out.copy()} rounds={r.rounds} dmg={r.damage_dealt}/{r.capped_damage} ---")
    return dict(out)


CASES = {
    "D1 Annoy(1/2 Taunt+DS) vs Cyclone(2/1 DS+WF)  [official win]": (
        [("BG_GVG_085", 1, 2, [GameTag.TAUNT, GameTag.DIVINE_SHIELD])],
        [("BG25_008", 2, 1, [GameTag.DIVINE_SHIELD, GameTag.WINDFURY])],
    ),
    "D3 Aureate(2/2 DS) vs RisenRider(2/1 Taunt+Reborn)  [official loss]": (
        [("BG26_174", 2, 2, [GameTag.DIVINE_SHIELD])],
        [("BG25_001", 2, 1, [GameTag.TAUNT, GameTag.REBORN])],
    ),
    "D5 RisenRider(2/1 Taunt+Reborn) vs ScarletSurvivor(3/3)  [official win]": (
        [("BG25_001", 2, 1, [GameTag.TAUNT, GameTag.REBORN])],
        [("BG30_129", 3, 3, [])],
    ),
}

if __name__ == "__main__":
    instrument()
    print("=== engine card data sanity ===")
    for cid in ["BG_GVG_085", "BG25_008", "BG26_174", "BG25_001", "BG30_129"]:
        d = db.get(cid)
        print(f"  {cid}: name={getattr(d,'name','?')} atk={d.atk} hp={d.health} "
              f"kw={sorted(getattr(d,'keywords',[]) or [])} script={d.id in __import__('hsrl2.scripts', fromlist=['REGISTRY']).REGISTRY}")
    for name, (p, o) in CASES.items():
        print(f"\n### {name}")
        print("  result:", run(p, o, seeds=16))
