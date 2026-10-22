import re
p=r"reversed\rea\hdt_bobsbuddy\BobsBuddyInvoker.cs"
s=open(p,encoding="utf-8").read()
lines=s.splitlines()
# locate ValidateSimulationResultAsync
for i,l in enumerate(lines,1):
    if "ValidateSimulationResultAsync" in l and ("async Task" in l or "private" in l or "internal" in l):
        print(f"DEF at {i}: {l.strip()}")
# print region around its definition
idx=[i for i,l in enumerate(lines) if "Task ValidateSimulationResultAsync" in l or "ValidateSimulationResultAsync(" in l]
print("hits:", idx[:10])
start=next((i for i,l in enumerate(lines) if "Task ValidateSimulationResultAsync" in l), None)
if start is None: start=next((i for i in idx if i>100), 0)
for i in range(start, min(start+120, len(lines))):
    print(f"{i+1}: {lines[i]}")
