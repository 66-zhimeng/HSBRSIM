import sys
sys.path.insert(0, r"C:\HSagent"); sys.path.insert(0, r"C:\HSagent\tools")
import diff_replay_combat as drc
print("ENGINE_ROOT:", drc.ENGINE_ROOT)
import inspect
src = inspect.getsource(drc.summarize) if hasattr(drc,"summarize") else ""
# find attributions counter construction
import re
s = open(r"C:\HSagent\tools\diff_replay_combat.py",encoding="utf-8").read()
m = re.search(r"attributions\s*=\s*Counter\([^\)]*\)", s, re.S)
print("attributions counter:", m.group(0) if m else "?")
for line in s.splitlines():
    if "attributions[" in line or "attributions =" in line:
        print("  ", line.strip())
