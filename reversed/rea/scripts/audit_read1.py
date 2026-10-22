import re, pathlib
out=pathlib.Path(r"reversed\rea\audit")
print("=== Network_PowerHistory.cs ===")
print((out/"Network_PowerHistory.cs").read_text(encoding="utf-8",errors="replace")[:1500])
for name in ("PowerTask.cs","PowerProcessor.cs","GameEntity.cs","PetGameplayManager.cs"):
    s=(out/name).read_text(encoding="utf-8",errors="replace")
    ls=s.splitlines()
    print("\n"+"="*70); print(name, "lines:", len(ls))
    for i,l in enumerate(ls,1):
        if re.search(r"PowerHistory|DoTask|OnPowerHistory|Bacon|Battleground|Combat|Network\.Get|Pegasus", l):
            print(f"{i}: {l.strip()[:220]}")
