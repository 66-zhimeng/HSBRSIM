p=r"reversed\rea\bobsbuddy\src_full\BobsBuddy.decompiled.cs"
lines=open(p,encoding="utf-8").read().splitlines()
import re
# find Ozumat, Illidan hero power classes and YShaarj for reference
for name in ["class OzumatHeroPower","class IllidanHeroPower","class YShaarjHeroPower","class Kurtrus"]:
    hits=[i for i,l in enumerate(lines) if name in l]
    print(name, "->", [h+1 for h in hits][:5])
    if hits:
        s=hits[0]
        for i in range(s, min(s+55,len(lines))): print(f"{i+1}: {lines[i]}")
        print("-"*50)
