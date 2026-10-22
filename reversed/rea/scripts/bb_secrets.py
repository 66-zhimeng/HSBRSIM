p=r"reversed\rea\bobsbuddy\src_full\BobsBuddy.decompiled.cs"
lines=open(p,encoding="utf-8").read().split("\n")
# CheckPackTactics 9148, BetterPackTactics 9173
for a,b in [(9140,9195),(9222,9250)]:
    print(f"===== {a}-{b} =====")
    for i in range(a-1,b): print(f"{i+1}: {lines[i]}")
