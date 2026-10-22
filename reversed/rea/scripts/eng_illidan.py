lines=open(r"hsrl2\scripts\batches\batch_hero_combat3.py",encoding="utf-8").read().splitlines()
for a,b in [(212,286)]:
    print(f"===== IllidanWingmenScript {a}-{b} =====")
    for i in range(a-1,b): print(f"{i+1}: {lines[i]}")
