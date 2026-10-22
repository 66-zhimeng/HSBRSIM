p=r"reversed\rea\bobsbuddy\src_full\BobsBuddy.decompiled.cs"
lines=open(p,encoding="utf-8").read().split("\n")
for a,b in [(270,340),(5530,5575)]:
    print(f"===== {a}-{b} =====")
    for i in range(a-1,b): print(f"{i+1}: {lines[i]}")
