p=r"reversed\rea\bobsbuddy\src_full\BobsBuddy.decompiled.cs"
lines=open(p,encoding="utf-8").read().splitlines()
for a,b in [(1186,1290),(1254,1320)]:
    print(f"===== {a}-{b} =====")
    for i in range(a-1,min(b,len(lines))): print(f"{i+1}: {lines[i]}")
