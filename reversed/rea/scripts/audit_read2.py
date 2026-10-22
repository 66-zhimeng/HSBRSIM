import pathlib
out=pathlib.Path(r"reversed\rea\audit")
s=(out/"PowerProcessor.cs").read_text(encoding="utf-8",errors="replace").splitlines()
for a,b in [(329,420),(696,780)]:
    print(f"===== PowerProcessor {a}-{b} =====")
    for i in range(a-1,min(b,len(s))): print(f"{i+1}: {s[i]}")
g=(out/"GameEntity.cs").read_text(encoding="utf-8",errors="replace").splitlines()
print("\n===== GameEntity 1518-1540 =====")
for i in range(1517,1540): print(f"{i+1}: {g[i]}")
print("\n===== GameEntity 650-670 =====")
for i in range(649,670): print(f"{i+1}: {g[i]}")
