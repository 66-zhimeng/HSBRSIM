import re, pathlib
s=pathlib.Path(r"reversed\net\decompiled\TB_BaconShop.cs").read_text(encoding="utf-8",errors="replace").splitlines()
for needle in ("IsInBattlegroundsCombatPhase", "HistoryBlock.Type.ATTACK", "BG_COMBAT_SPEED_ATTACKS_REMAINING"):
    idx=[i for i,l in enumerate(s) if needle in l]
    print("="*60); print(needle, [i+1 for i in idx][:5])
    for a in idx[:2]:
        for i in range(max(0,a-2), min(a+16,len(s))): print(f"{i+1}: {s[i]}")
