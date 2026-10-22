import re, pathlib
gp=pathlib.Path(r"reversed\rea\audit\Gameplay.cs").read_text(encoding="utf-8",errors="replace").splitlines()
print("===== Gameplay debug-info handlers 785-840 =====")
for i in range(784, min(845,len(gp))): print(f"{i+1}: {gp[i]}")
