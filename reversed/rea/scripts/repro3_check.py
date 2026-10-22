import sys
sys.path.insert(0, r"C:\HSagent")
from collections import Counter
from hsrl2.db import CardDB
from hsrl2.game import Game
from hsrl2.hero import Hero
from hsrl2.tags import GameTag
db = CardDB.load(r"C:\HSagent\data")
def micro(player, opponent, seeds=16, trace_seed=None):
    out=Counter(); traces={}
    for seed in range(seeds):
        a,b=Hero("BG20_HERO_100","P"),Hero("BG20_HERO_101","O")
        g=Game([a,b],db,seed=seed)
        placed=[]
        for hero,specs in ((a,player),(b,opponent)):
            for cid,atk,hp,kws in specs:
                m=g.create_minion(cid,controller=hero)
                m.set(GameTag.BASE_ATK,atk); m.set(GameTag.ATK,atk)
                m.set(GameTag.BASE_HEALTH,hp); m.set(GameTag.HEALTH,hp)
                for t in kws: m.set(t,True)
                g.summon(hero,m); placed.append(m)
        log=[]
        if seed==trace_seed:
            import hsrl2.combat as CB
            orig=CB.CombatScheduler._execute_attack
            def ex(self,att,deff,_o=orig,_l=log):
                _l.append(f"ATTACK {att.card_id}->{deff.card_id}")
                _o(self,att,deff)
                _l.append("  after: "+str([[x.card_id for x in h.board if not x.dead] for h in (self.a,self.b)]))
            CB.CombatScheduler._execute_attack=ex
        r=g.run_combat(a,b)
        if seed==trace_seed:
            CB.CombatScheduler._execute_attack=orig
        out["win" if r.winner is a else "loss" if r.winner is b else "tie"]+=1
        if seed==trace_seed: traces['log']=log
    return dict(out), traces
print("repro3 Risen 2/1 Taunt+Reborn vs BG30_129(3/3):", micro([("BG25_001",2,1,[GameTag.TAUNT,GameTag.REBORN])],[("BG30_129",3,3,[])],16,0))
print("D5裸板 Risen 2/1 vs BG35_814(3/3):", micro([("BG25_001",2,1,[GameTag.TAUNT,GameTag.REBORN])],[("BG35_814",3,3,[])]))
