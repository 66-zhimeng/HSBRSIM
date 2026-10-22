import re
p = r"reversed\rea\hsmod\src\HsMod.decompiled.cs"
lines = open(p, encoding="utf-8").read().split("\n")
cur = None
out = []
for i, l in enumerate(lines, 1):
    if len(l) > 400:
        continue
    m = re.match(r"\s*public class (\w+)", l)
    if m:
        cur = m.group(1)
        out.append(f"\n== class {cur} ==")
        continue
    if cur and ("HarmonyPatch(typeof(" in l or "[Harmony" in l):
        out.append(f"  {i}: {l.strip()}")
print("\n".join(out[:400]))
