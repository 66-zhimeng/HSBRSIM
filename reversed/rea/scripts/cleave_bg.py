import sys, re
sys.path.insert(0, r"C:\HSagent")
from hsrl2.db import CardDB
db=CardDB.load(r"C:\HSagent\data")
for d in db._by_id.values():
    if d.name in ("Blade Collector","Foe Reaper 4000","Magnataur Alpha"):
        print(d.name, "->", d.id)
s=open(r"hsdata\CardDefs.xml",encoding="utf-8").read()
for cid in ["BGS_061","BG25_011"]:
    m=re.search(r'<Entity CardID="%s"'%cid, s)
    if m: print(cid, "line", s.count("\n",0,m.start())+1)
