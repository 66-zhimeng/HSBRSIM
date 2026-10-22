p=r"reversed\rea\bobsbuddy\src_full\BobsBuddy.decompiled.cs"
lines=open(p,encoding="utf-8").read().splitlines()
import re
for i,l in enumerate(lines):
    if re.search(r"CloneAsVanilla|CloneAsShallowExactCopy|public Minion Clone\(", l):
        print(f"{i+1}: {l.strip()}")
