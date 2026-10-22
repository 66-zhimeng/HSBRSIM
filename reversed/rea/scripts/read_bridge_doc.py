lines=open(r"docs\REAL_GAME_BRIDGE.md",encoding="utf-8").read().splitlines()
print("total lines:", len(lines))
for i,l in enumerate(lines,1):
    if 120 <= i <= 330:
        print(f"{i}: {l}")
