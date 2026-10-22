import pathlib, re
root=pathlib.Path(r"reversed\net\Assembly-CSharp")
pat=re.compile(r"DamageCapPanel|GetRealtimeBaconDamageCapEnabled|BACON_COMBAT_DAMAGE_CAP", re.I)
for f in root.rglob("*.tsv"):
    for i,line in enumerate(f.read_text(encoding="utf-8-sig",errors="replace").splitlines(),1):
        if pat.search(line):
            print(f"{f.name}:{i}: {line.strip()[:400]}")
