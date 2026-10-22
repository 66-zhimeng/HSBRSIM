import sys, json
sys.path.insert(0, r"C:\HSagent")
from hsrl2.db import CardDB
db = CardDB.load(r"C:\HSagent\data")
# 1) names -> ids in engine data for the 6-trace cards
targets = ["Annoy-o-Tron","Crackling Cyclone","Aureate Laureate","Risen Rider","Scarlet Survivor","River Skipper","Eternal Knight","Soul Rewinder","Wildfire Elemental","Water Droplet","Catacomb Crasher"]
from collections import defaultdict
byname = defaultdict(list)
for d in db._by_id.values():
    if d.name:
        byname[d.name].append((d.id, d.atk, d.health, d.tech_level, getattr(d,"is_golden_def",False), getattr(d,"is_pool_minion",False)))
for t in targets:
    print(t, "->", byname.get(t))
# 2) build resolver exactly as harness
sys.path.insert(0, r"C:\HSagent")
import importlib.util
spec = importlib.util.spec_from_file_location("drc", r"C:\HSagent\tools\diff_replay_combat.py")
drc = importlib.util.module_from_spec(spec)
try:
    spec.loader.exec_module(drc)
except SystemExit:
    pass
res = drc.CardResolver(db, official_cards_path=None)
for t in ["Annoy-o-Tron","Crackling Cyclone","Aureate Laureate","Risen Rider","Scarlet Survivor","River Skipper"]:
    cid = res.resolve(t)
    d = db.get(cid) if cid else None
    print(f"resolver: {t!r} -> {cid} engine_name={getattr(d,'name',None)}")
