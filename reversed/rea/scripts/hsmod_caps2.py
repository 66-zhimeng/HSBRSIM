import re
p = r"reversed\rea\hsmod\src\HsMod.decompiled.cs"
lines = open(p, encoding="utf-8").read().split("\n")
# PatchBattlegrounds region
start = next(i for i,l in enumerate(lines) if "public class PatchBattlegrounds" in l)
out=[]
for i in range(start, min(start+330, len(lines))):
    l = lines[i]
    if len(l) > 300: continue
    if re.search(r'HarmonyPatch|public static|ConfigValue|GameMgr|Bacon|Battleground|SpellController|GameState|PowerTask|Simulat', l):
        out.append(f'{i+1}: {l.strip()}')
print("\n".join(out))
print("\n=== global capability keywords (short lines only) ===")
kw = re.compile(r'AutoBuy|auto buy|ReadProcessMemory|OpenProcess|HearthMirror|Simulat|Power\.log|ZoneHand|ShowCardNum|IsQuickMode|IsShutUpBob|IsEnemyCardNum|RealtimeCardNum')
hits=[]
for i,l in enumerate(lines):
    if len(l) > 300: continue
    if kw.search(l):
        hits.append(f'{i+1}: {l.strip()[:160]}')
print("\n".join(hits[:60]))
