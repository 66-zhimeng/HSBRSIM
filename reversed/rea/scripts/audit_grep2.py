import re
from pathlib import Path
base=Path(r"reversed\net\Assembly-CSharp")
types=(base/"types.tsv").read_text(encoding="utf-8-sig",errors="replace").splitlines()
methods=(base/"methods.tsv").read_text(encoding="utf-8-sig",errors="replace").splitlines()

def show(title, lines, limit=60):
    print("\n=== "+title+f" ({len(lines)}) ===")
    for ln in lines[:limit]: print("  ", ln.strip()[:260])

# 1) LocalGame string context in binary
data=Path(r"C:\Program Files (x86)\Hearthstone\Hearthstone_Data\Managed\Assembly-CSharp.dll").read_bytes()
i=data.find(b"LocalGame")
print("LocalGame byte offset:", i)
if i>=0:
    seg=data[max(0,i-120):i+160]
    print("context ascii:", re.sub(rb"[^\x20-\x7e]", b".", seg).decode("ascii"))

# 2) candidate local combat executor names
exec_pat=re.compile(r"(Simulate|Resolve|Execute|Calculate|Predict|Run|Compute|Adjudicat|Replay).{0,25}(Combat|Attack|Damage|Death)|(Combat|Attack|Damage|Death).{0,25}(Simulate|Resolve|Execute|Calculate|Predict|Adjudicat)", re.I)
show("executor-like methods (top 80)", [l for l in methods if exec_pat.search(l)], 80)

# 3) test/cheat/dev/practice/tutorial + combat
aux_pat=re.compile(r"(Cheat|DevOption|Debug|Practice|Tutorial|BoardTester|LocalGame|LocalPlay|Sandbox).{0,30}(Combat|Attack|Damage|Death|Bacon|Battleground)|(Combat|Attack|Bacon|Battleground).{0,30}(Cheat|Debug|Practice|Tutorial|BoardTester|LocalGame)", re.I)
show("cheat/debug/tutorial combat candidates", [l for l in methods if aux_pat.search(l)], 60)

# 4) PowerTaskList / PowerProcessor members
show("PowerTaskList methods", [l for l in methods if l.startswith("PowerTaskList::")], 80)
show("PowerProcessor methods", [l for l in methods if l.startswith("PowerProcessor::")], 60)
show("PowerTaskList nested types", [l for l in types if l.startswith("class\tPowerTaskList") or l.startswith("struct\tPowerTaskList") or "PowerTaskList/" in l], 40)
