import re
from pathlib import Path
base=Path(r"reversed\net\Assembly-CSharp")
methods=(base/"methods.tsv").read_text(encoding="utf-8-sig",errors="replace").splitlines()
types=(base/"types.tsv").read_text(encoding="utf-8-sig",errors="replace").splitlines()
def show(t,ls,lim=100):
    print("\n=== "+t+f" ({len(ls)}) ===")
    for l in ls[:lim]: print("  ",l.strip()[:240])
show("methods referencing PowerHistory",[l for l in methods if "PowerHistory" in l],120)
show("Replay/Spectator/Reconnect types",[l for l in types if re.search(r"Replay|Spectat|Reconnect",l)],60)
show("Replay/Spectator/Reconnect methods",[l for l in methods if re.search(r"Replay|Spectat|Reconnect",l)],80)
