import io
p=r"hsrl2\scripts\batches\batch_hero_combat3.py"
lines=open(p,encoding="utf-8").read().splitlines()
for a,b in [(441,568),(646,733)]:
    print(f"===== {a}-{b} =====")
    for i in range(a-1,b): print(f"{i+1}: {lines[i]}")
