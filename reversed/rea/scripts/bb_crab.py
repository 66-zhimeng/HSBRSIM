p=r"reversed\rea\bobsbuddy\src_full\BobsBuddy.decompiled.cs"
lines=open(p,encoding="utf-8").read().split("\n")
import re
# locate GenericDeathrattles class and Crab
for i,l in enumerate(lines):
    if "class GenericDeathrattles" in l or "Crab" in l and len(l)<300:
        print(f"{i+1}: {l.strip()[:200]}")
