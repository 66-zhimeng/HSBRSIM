import pathlib
net=pathlib.Path(r"reversed\rea\audit\Network.cs").read_text(encoding="utf-8",errors="replace").splitlines()
print("===== Network.GetPowerHistory 10578-10690 =====")
for i in range(10577, min(10690,len(net))): print(f"{i+1}: {net[i]}")
gp=pathlib.Path(r"reversed\rea\audit\Gameplay.cs").read_text(encoding="utf-8",errors="replace").splitlines()
print("\n===== Gameplay.OnPowerHistory 706-722 =====")
for i in range(705,722): print(f"{i+1}: {gp[i]}")
ptl=pathlib.Path(r"reversed\net\decompiled\PowerTaskList.cs").read_text(encoding="utf-8",errors="replace").splitlines()
for needle in ("public PowerTask CreateTask(", "public void DoTasks("):
    idx=[i for i,l in enumerate(ptl) if needle in l]
    print("\n===== PowerTaskList", needle, [i+1 for i in idx])
    if idx:
        a=idx[0]
        for i in range(a, min(a+30,len(ptl))): print(f"{i+1}: {ptl[i]}")
