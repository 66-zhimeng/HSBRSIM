import sys
sys.path.insert(0, r"C:\HSagent"); sys.path.insert(0, r"C:\HSagent\tools")
import diff_replay_combat as drc
from hsrl2.db import CardDB
from hsrl2.game import Game
from hsrl2.hero import Hero
from hsrl2.tags import GameTag
from hsrl2.events import Listener
import hsrl2.scripts.batches.batch_hero_combat3 as bhc
from hsrl2.minion import Minion

db = CardDB.load(r"C:\HSagent\data")
resolver = drc.CardResolver(db, drc.OFFICIAL_DATA_255130)
hi = drc.HeroIndex(db)
tr = drc.T.load_trace(r"C:\HSagent\runs\official_traces\f5d1c647b2f449d1.json")

# 1) 现状：Ozumat 把空位填满
def run(seeds=8, patch=False):
    if patch:
        orig = bhc.OzumatScript.on_bind
        def one_tentacle_on_bind(hero, game):
            d = bhc._power_def(game, "BG23_HERO_201p")
            def on_soc(g, hero=None, _owner=hero, **kw):
                if hero is not _owner or _owner.board_full():
                    return
                t = g.create_minion(bhc.TENTACLE_ID, controller=_owner)
                target = d.num(0) or 0
                td = g.db.get(bhc.TENTACLE_ID)
                delta, dh = target - td.atk, target - td.health
                if delta > 0 or dh > 0:
                    from hsrl2.defs import BuffEnchant
                    t.add_buff(BuffEnchant(atk=max(0, delta), health=max(0, dh)))
                g.summon(_owner, t)
            game.events.register(Listener(event="start_of_combat", owner=hero, callback=on_soc))
            game.events.register(Listener(event="start_of_combat", owner=hero, callback=on_soc)) if False else None
        bhc.OzumatScript.on_bind = staticmethod(one_tentacle_on_bind)
        reg = __import__("hsrl2.scripts", fromlist=["REGISTRY"]).REGISTRY
        reg["BG23_HERO_201p"] = bhc.OzumatScript
    out = {}
    for seed in range(seeds):
        ha,_ = hi.resolve(tr.player.hero_power_id); hb,_ = hi.resolve(tr.opponent.hero_power_id)
        a,b = Hero(ha,"P"), Hero(hb,"O")
        g = Game([a,b], db, seed=seed); g.turn = int(tr.turn or 1)
        for h,pid in ((a,tr.player.hero_power_id),(b,tr.opponent.hero_power_id)):
            if pid: g.wire_hero_power_on_bind(h,pid)
        for hero, band in ((a,tr.player),(b,tr.opponent)):
            for tm in band.minions:
                cid = resolver.resolve(tm.name)
                m = g.create_minion(cid, controller=hero, golden=bool(tm.golden))
                m.set(GameTag.BASE_ATK, tm.atk); m.set(GameTag.ATK, tm.atk)
                m.set(GameTag.BASE_HEALTH, tm.health); m.set(GameTag.HEALTH, tm.health)
                for kw in tm.keywords:
                    if kw in drc.ENGINE_KEYWORD_TAGS: m.set(drc.ENGINE_KEYWORD_TAGS[kw], True)
                g.summon(hero, m)
        snap=[]
        g.events.register(Listener(event="before_attack", owner=None, callback=lambda _g=None,**k: snap.append(len([x for x in g.heroes[0].board if not x.dead]))))
        r = g.run_combat(a,b)
        out["win" if r.winner is a else "loss" if r.winner is b else "tie"] = out.get("win" if r.winner is a else "loss" if r.winner is b else "tie",0)+1
    return out

print("D4 现状(engine, Ozumat fill-board):", run(patch=False))
print("D4 若修成 '只召唤 1 只 Tentacle'  :", run(patch=True))
