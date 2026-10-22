import re, pathlib
managed = pathlib.Path(r"C:\Program Files (x86)\Hearthstone\Hearthstone_Data\Managed")
keywords = [b"CombatSimulator", b"SimulateCombat", b"ResolveCombat", b"CalculateCombat",
            b"PredictCombat", b"CombatResolver", b"CombatAdjudicator", b"BattleSimulator",
            b"CombatEngine", b"CombatLogic", b"SimulateFight", b"RunCombat",
            b"BaconCombat", b"BattlegroundsCombat", b"DamageCap", b"LocalGame",
            b"BaconBoardTester"]
rows=[]
for dll in sorted(managed.glob("*.dll")):
    data = dll.read_bytes()
    hits = {}
    for kw in keywords:
        c = data.count(kw)
        if c:
            hits[kw.decode()] = c
    if hits:
        rows.append((dll.name, dll.stat().st_size, hits))
print("=== DLLs with adjudicator/BG keyword byte hits ===")
for name,size,hits in rows:
    print(f"{name:45s} {size/1024:8.1f}KB  {hits}")
print("\nscanned:", len(list(managed.glob("*.dll"))), "dlls")
