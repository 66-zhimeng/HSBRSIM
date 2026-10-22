p=r"reversed\rea\bobsbuddy\src_full\BobsBuddy.decompiled.cs"
lines=open(p,encoding="utf-8").read().split("\n")
import re
hits=[i for i,l in enumerate(lines) if re.search(r"class TeronGorefiendHeroPower|TeronGorefiend", l)]
print("hits:", [h+1 for h in hits][:20])
if hits:
    s=hits[0]
    for i in range(s, min(s+90, len(lines))): print(f"{i+1}: {lines[i]}")
