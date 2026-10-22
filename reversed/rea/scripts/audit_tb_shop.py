import re
from pathlib import Path
s=Path(r"reversed\net\decompiled\TB_BaconShop.cs").read_text(encoding="utf-8",errors="replace").splitlines()
# locate OnCombatPhase / GetRemainingCombatEvents / IsInBattlegroundsCombatPhase
for needle in ("OnCombatPhase", "GetRemainingCombatEvents", "IsInBattlegroundsCombatPhase", "HandleCombatSpeedUp", "StartBGCombatSpeedManager"):
    idx=[i for i,l in enumerate(s) if needle in l]
    print("="*60); print(needle, "hits:", [i+1 for i in idx][:6])
    if idx:
        a=idx[0]
        for i in range(a, min(a+35, len(s))): print(f"{i+1}: {s[i]}")
