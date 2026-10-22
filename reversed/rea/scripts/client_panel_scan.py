import re
for f in [r"reversed\rea\client\PlayerLeaderboardManager.cs", r"reversed\rea\client\PlayerLeaderboardRecentCombatsPanel.cs", r"reversed\rea\client\Entity.cs"]:
    s=open(f,encoding="utf-8").read()
    lines=s.splitlines()
    print("="*70); print(f, "lines:", len(lines))
    for i,l in enumerate(lines,1):
        if re.search(r"DamageCap|BACON_COMBAT_DAMAGE_CAP|GetRealtimeBaconDamageCapEnabled", l):
            print(f"{i}: {l.strip()[:220]}")
