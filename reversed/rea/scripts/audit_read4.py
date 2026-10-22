import re, pathlib
gs=pathlib.Path(r"reversed\net\decompiled\GameState.cs").read_text(encoding="utf-8",errors="replace").splitlines()
for needle in ("public bool OnTagChange(", "public bool OnFullEntity(", "public bool OnChangeEntity(", "private void OnDamageCapChanged"):
    idx=[i for i,l in enumerate(gs) if needle in l]
    print("="*60); print(needle, [i+1 for i in idx][:3])
    if idx:
        a=idx[0]
        for i in range(a,min(a+26,len(gs))): print(f"{i+1}: {gs[i]}")
gu=pathlib.Path(r"reversed\rea\audit\GameUtils.cs").read_text(encoding="utf-8",errors="replace").splitlines()
idx=[i for i,l in enumerate(gu) if "public static void ApplyPower(" in l]
print("="*60); print("GameUtils.ApplyPower at", [i+1 for i in idx])
if idx:
    a=idx[0]
    for i in range(a,min(a+40,len(gu))): print(f"{i+1}: {gu[i]}")
