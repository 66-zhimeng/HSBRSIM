import sys
sys.path.insert(0, r"C:\HSagent")
from hsrl2.db import CardDB
db=CardDB.load(r"C:\HSagent\data")
n_attr=0; attrs=set()
for d in list(db._by_id.values()):
    for a in ("deathrattle","is_deathrattle","has_deathrattle"):
        if hasattr(d,a): attrs.add(a)
    if getattr(d,"deathrattle",False): n_attr+=1
print("attrs found:", attrs, "cards with deathrattle=True:", n_attr)
# text proxy
import re
n_text=0
for d in db._by_id.values():
    t=(getattr(d,"text","") or "")
    if re.search(r"亡语|Deathrattle", t): n_text+=1
print("cards whose text mentions deathrattle:", n_text)
