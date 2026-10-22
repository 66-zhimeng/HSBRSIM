import re
from pathlib import Path
base=Path(r"reversed\net\Assembly-CSharp")
types=(base/"types.tsv").read_text(encoding="utf-8-sig",errors="replace").splitlines()
methods=(base/"methods.tsv").read_text(encoding="utf-8-sig",errors="replace").splitlines()
def show(t,ls,lim=80):
    print("\n=== "+t+f" ({len(ls)}) ===")
    for l in ls[:lim]: print("  ",l.strip()[:240])
show("GameState power/history/network methods",[l for l in methods if l.startswith("GameState::") and re.search(r"Power|History|Network|Task|Process|Apply|Entity",l)],90)
show("Network power/history methods",[l for l in methods if l.startswith("Network::") and re.search(r"Power|History|GameState|Packet|Receive",l)],60)
show("PowerTask methods",[l for l in methods if l.startswith("PowerTask::")],60)
show("PowerQueue methods",[l for l in methods if l.startswith("PowerQueue::")],40)
# types with Power* / EntityDef data path
show("Power*/EntityDef types",[l for l in types if re.match(r"(class|struct|enum)\t(Power|EntityDef|GameEntity)",l)],60)
